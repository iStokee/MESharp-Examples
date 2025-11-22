using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MESharp.API;
using StokeeFishing.Data;
using StokeeFishing.Navigation;
using StokeeFishing.Services;
using StokeeFishing.StateMachine;

namespace StokeeFishing.ViewModels;

/// <summary>
/// Main ViewModel for the fishing script UI.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly MetricsService _metrics;
    private readonly NavigationService _navigation;
    private FishingMachine? _machine;
    private readonly List<string> _logMessages = new();
    private System.Windows.Threading.DispatcherTimer? _updateTimer;

    public MainViewModel()
    {
        _metrics = new MetricsService();
        _navigation = new NavigationService(Log);

        // Initialize collections
        AvailableLocations = new ObservableCollection<FishingLocation>(FishingLocation.All);
        AvailableFishTypes = new ObservableCollection<FishType>(FishType.All);
        AvailableBanks = new ObservableCollection<BankLocation>(BankLocation.All);

        // Set defaults
        SelectedInventoryAction = InventoryFullAction.DropFish;
        SelectedReturnMethod = ReturnToFishingMethod.Walk;

        // Load GE prices in background
        _ = LoadPricesAsync();
    }

    #region Observable Properties - Status

    [ObservableProperty]
    private string _gameStatus = "Waiting for injection...";

    [ObservableProperty]
    private string _scriptStatus = "Stopped";

    [ObservableProperty]
    private string _playerPosition = "Unknown";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _currentState = "Stopped";

    #endregion

    #region Observable Properties - Metrics

    [ObservableProperty]
    private string _runtime = "00:00:00";

    [ObservableProperty]
    private long _totalFishCaught;

    [ObservableProperty]
    private string _fishPerHour = "0/hr";

    [ObservableProperty]
    private int _fishingXpGained;

    [ObservableProperty]
    private string _fishingXpPerHour = "0/hr";

    [ObservableProperty]
    private int _currentFishingLevel;

    [ObservableProperty]
    private int _levelsGained;

    [ObservableProperty]
    private string _xpToNextLevel = "0";

    [ObservableProperty]
    private string _timeToNextLevel = "--:--:--";

    [ObservableProperty]
    private string _totalGp = "0";

    [ObservableProperty]
    private string _gpPerHour = "0/hr";

    [ObservableProperty]
    private int _tripCount;

    [ObservableProperty]
    private int _currentTripFish;

    [ObservableProperty]
    private string _avgFishPerTrip = "0";

    #endregion

    #region Observable Properties - Configuration

    [ObservableProperty]
    private FishingLocation? _selectedLocation;

    [ObservableProperty]
    private FishingSpotType? _selectedSpotType;

    [ObservableProperty]
    private FishType? _selectedFishType;

    [ObservableProperty]
    private BankLocation? _selectedBank;

    [ObservableProperty]
    private InventoryFullAction _selectedInventoryAction;

    [ObservableProperty]
    private ReturnToFishingMethod _selectedReturnMethod;

    [ObservableProperty]
    private bool _useBoostPotions;

    [ObservableProperty]
    private bool _useBankTeleport;

    public ObservableCollection<FishingLocation> AvailableLocations { get; }
    public ObservableCollection<FishType> AvailableFishTypes { get; }
    public ObservableCollection<BankLocation> AvailableBanks { get; }

    public IEnumerable<InventoryFullAction> InventoryActions => Enum.GetValues<InventoryFullAction>();
    public IEnumerable<ReturnToFishingMethod> ReturnMethods => Enum.GetValues<ReturnToFishingMethod>();

    #endregion

    #region Observable Properties - Log

    [ObservableProperty]
    private string _logText = "";

    #endregion

    #region Commands

    [RelayCommand]
    private void Start()
    {
        if (IsRunning)
        {
            Stop();
            return;
        }

        if (!ValidateConfiguration())
        {
            Log("Invalid configuration. Please select a fishing location.");
            return;
        }

        var config = CreateConfiguration();
        _machine = new FishingMachine(config, _metrics, _navigation, Log);

        _machine.Start();
        IsRunning = true;
        ScriptStatus = "Running";

        StartUpdateTimer();
        Log("Script started!");
    }

    [RelayCommand]
    public void Stop()
    {
        _machine?.Stop();
        IsRunning = false;
        ScriptStatus = "Stopped";
        CurrentState = "Stopped";

        StopUpdateTimer();
        Log("Script stopped.");
    }

    [RelayCommand]
    private void ClearLog()
    {
        _logMessages.Clear();
        LogText = "";
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update the UI state. Call this from a timer.
    /// </summary>
    public void Update()
    {
        UpdateGameStatus();
        UpdateMetrics();

        if (_machine != null)
        {
            CurrentState = _machine.CurrentState.ToString();
            ScriptStatus = _machine.StatusMessage;
            _machine.Tick();
        }
    }

    /// <summary>
    /// Initialize the update timer.
    /// </summary>
    public void StartUpdateTimer()
    {
        if (_updateTimer != null) return;

        _updateTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _updateTimer.Tick += (s, e) => Update();
        _updateTimer.Start();
    }

    /// <summary>
    /// Stop the update timer.
    /// </summary>
    public void StopUpdateTimer()
    {
        _updateTimer?.Stop();
        _updateTimer = null;
    }

    #endregion

    #region Private Methods

    private void UpdateGameStatus()
    {
        if (!Game.IsInjected || !Game.HasClientPointers)
        {
            GameStatus = "Waiting for injection...";
            PlayerPosition = "Unknown";
            return;
        }

        if (!LocalPlayer.IsLoggedIn())
        {
            GameStatus = "Waiting for login...";
            PlayerPosition = "Unknown";
            return;
        }

        GameStatus = $"Connected ({Game.State})";
        var (x, y, z) = LocalPlayer.GetTilePosition();
        PlayerPosition = $"{Game.LocalPlayerName} @ ({x}, {y}, {z})";
    }

    private void UpdateMetrics()
    {
        Runtime = _metrics.RuntimeFormatted;
        TotalFishCaught = _metrics.TotalFishCaught;
        FishPerHour = $"{_metrics.FishPerHour:N0}/hr";
        FishingXpGained = _metrics.FishingXpGained;
        FishingXpPerHour = $"{_metrics.FishingXpPerHour:N0}/hr";
        CurrentFishingLevel = _metrics.CurrentFishingLevel;
        LevelsGained = _metrics.FishingLevelsGained;
        XpToNextLevel = $"{_metrics.XpToNextLevel:N0}";
        TimeToNextLevel = _metrics.TimeToNextLevelFormatted;
        TotalGp = _metrics.TotalGpFormatted;
        GpPerHour = _metrics.GpPerHourFormatted + "/hr";
        TripCount = _metrics.TripCount;
        CurrentTripFish = _metrics.CurrentTripFish;
        AvgFishPerTrip = $"{_metrics.AverageFishPerTrip:F1}";
    }

    private bool ValidateConfiguration()
    {
        return SelectedLocation != null;
    }

    private FishingConfiguration CreateConfiguration()
    {
        return new FishingConfiguration
        {
            FishingLocation = SelectedLocation,
            FishingSpotType = SelectedSpotType ?? SelectedLocation?.SpotType,
            TargetFish = SelectedFishType,
            BankLocation = SelectedBank ?? SelectedLocation?.NearestBank,
            InventoryFullAction = SelectedInventoryAction,
            ReturnMethod = SelectedReturnMethod,
            UseBoostPotions = UseBoostPotions,
            UseBankTeleport = UseBankTeleport
        };
    }

    private async Task LoadPricesAsync()
    {
        try
        {
            Log("Loading GE prices...");
            await GrandExchange.PreloadFishPricesAsync();
            Log("GE prices loaded.");
        }
        catch (Exception ex)
        {
            Log($"Failed to load GE prices: {ex.Message}");
        }
    }

    private void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logLine = $"[{timestamp}] {message}";

        _logMessages.Add(logLine);

        // Keep only last 100 messages
        while (_logMessages.Count > 100)
            _logMessages.RemoveAt(0);

        LogText = string.Join(Environment.NewLine, _logMessages);
    }

    #endregion

    #region Property Change Handlers

    partial void OnSelectedLocationChanged(FishingLocation? value)
    {
        if (value != null)
        {
            // Auto-select the spot type and bank
            SelectedSpotType = value.SpotType;
            SelectedBank = value.NearestBank;

            // Filter available fish types for this location
            var availableFish = value.SpotType.Fish;
            if (availableFish.Length > 0)
                SelectedFishType = availableFish[0];

            Log($"Selected location: {value.Name}");
        }
    }

    partial void OnSelectedInventoryActionChanged(InventoryFullAction value)
    {
        // If power fishing, bank selection is not needed
        if (value == InventoryFullAction.DropFish)
        {
            Log("Power fishing mode - fish will be dropped");
        }
        else
        {
            Log($"Banking mode: {value}");
        }
    }

    #endregion
}
