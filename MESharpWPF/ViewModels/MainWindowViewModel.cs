using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using MESharp.API;

namespace MESharpExamples.WPF.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    private string _gameStateText = "Waiting for MemoryError...";
    private string _playerTileText = "—";
    private string _injectionFlagsText = "—";
    private string _logText = string.Empty;

    public MainWindowViewModel()
    {
        RefreshInventoryCommand = new RelayCommand(RefreshInventory);
        DescribeInteractionCommand = new RelayCommand(DescribeInteraction);

        _statusTimer.Tick += OnStatusTick;
        _statusTimer.Start();
        RefreshStatus();
    }

    public ObservableCollection<InventoryItemViewModel> InventoryItems { get; } = new();

    public ICommand RefreshInventoryCommand { get; }

    public ICommand DescribeInteractionCommand { get; }

    public string GameStateText
    {
        get => _gameStateText;
        private set => SetField(ref _gameStateText, value);
    }

    public string PlayerTileText
    {
        get => _playerTileText;
        private set => SetField(ref _playerTileText, value);
    }

    public string InjectionFlagsText
    {
        get => _injectionFlagsText;
        private set => SetField(ref _injectionFlagsText, value);
    }

    public string LogText
    {
        get => _logText;
        private set => SetField(ref _logText, value);
    }

    private void OnStatusTick(object? sender, EventArgs e) => RefreshStatus();

    private void RefreshStatus()
    {
        if (!Game.IsInjected || !Game.HasClientPointers)
        {
            GameStateText = "Waiting for MemoryError injection";
            PlayerTileText = "—";
            InjectionFlagsText = Game.IsInjected ? "Pointers not ready" : "Not injected";
            return;
        }

        GameStateText = $"{Game.State} ({(LocalPlayer.IsLoggedIn() ? "Logged In" : "Menu")})";
        var (x, y, z) = LocalPlayer.GetTilePosition();
        PlayerTileText = $"{Game.LocalPlayerName} @ {x}, {y}, {z}";
        InjectionFlagsText = Game.InjectionFlags.ToString();
    }

    private void RefreshInventory()
    {
        try
        {
            var items = Inventory.GetAll();
            InventoryItems.Clear();
            foreach (var item in items)
            {
                InventoryItems.Add(new InventoryItemViewModel(item.Slot, item.Name, item.Amount));
            }

            AppendLog($"Inventory refreshed ({items.Count} items, {Inventory.FreeSlots} free slots).");
        }
        catch (Exception ex)
        {
            AppendLog($"Inventory refresh failed: {ex.Message}");
        }
    }

    private void DescribeInteraction()
    {
        try
        {
            if (!Game.HasClientPointers)
            {
                AppendLog("Client is not ready yet.");
                return;
            }

            if (!LocalPlayer.IsLoggedIn())
            {
                AppendLog("Player is not logged in.");
                return;
            }

            var interactingName = LocalPlayer.GetInteractingWith();
            var interactionId = LocalPlayer.GetInteractingWithId();

            if (string.IsNullOrWhiteSpace(interactingName))
            {
                AppendLog("You are not currently interacting with an NPC or object.");
            }
            else
            {
                AppendLog($"Interacting with '{interactingName}' (id {interactionId}).");
            }

            var totalCoins = Inventory.FindById(995).Aggregate(0UL, (acc, item) => acc + item.Amount);
            AppendLog($"Detected approximately {totalCoins:N0} coins in inventory.");
        }
        catch (Exception ex)
        {
            AppendLog($"Describe interaction failed: {ex.Message}");
        }
    }

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:T}] {message}";
        LogText = string.IsNullOrEmpty(LogText) ? line : $"{LogText}{Environment.NewLine}{line}";
    }

    public void Dispose()
    {
        _statusTimer.Stop();
        _statusTimer.Tick -= OnStatusTick;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public sealed record InventoryItemViewModel(int Slot, string Name, ulong Amount);
