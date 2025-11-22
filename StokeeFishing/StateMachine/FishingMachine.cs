using MESharp.API;
using Stateless;
using StokeeFishing.Data;
using StokeeFishing.Navigation;
using StokeeFishing.Services;

namespace StokeeFishing.StateMachine;

/// <summary>
/// The main fishing state machine that orchestrates all fishing logic.
/// </summary>
public class FishingMachine
{
    private readonly StateMachine<FishingState, FishingTrigger> _machine;
    private FishingState _state = FishingState.Stopped;

    private readonly FishingConfiguration _config;
    private readonly MetricsService _metrics;
    private readonly NavigationService _navigation;
    private readonly Action<string> _log;

    private DateTime _lastFishingCheck = DateTime.MinValue;
    private int _lastInventoryCount = 0;
    private int _fishingAttempts = 0;
    private int _idleCounter = 0;

    public FishingMachine(
        FishingConfiguration config,
        MetricsService metrics,
        NavigationService navigation,
        Action<string> log)
    {
        _config = config;
        _metrics = metrics;
        _navigation = navigation;
        _log = log;

        _machine = new StateMachine<FishingState, FishingTrigger>(() => _state, s => _state = s);
        ConfigureStateMachine();
    }

    /// <summary>
    /// Current state of the machine.
    /// </summary>
    public FishingState CurrentState => _state;

    /// <summary>
    /// Whether the machine is actively running (not stopped).
    /// </summary>
    public bool IsRunning => _state != FishingState.Stopped && _state != FishingState.Error;

    /// <summary>
    /// Human-readable status message.
    /// </summary>
    public string StatusMessage { get; private set; } = "Stopped";

    private void ConfigureStateMachine()
    {
        // STOPPED state
        _machine.Configure(FishingState.Stopped)
            .Permit(FishingTrigger.Start, FishingState.Initializing)
            .OnEntry(() => SetStatus("Stopped"));

        // INITIALIZING state
        _machine.Configure(FishingState.Initializing)
            .Permit(FishingTrigger.AtFishingSpot, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.NearFishingSpot, FishingState.WalkingToFishingSpot)
            .Permit(FishingTrigger.AtBank, FishingState.Banking)
            .Permit(FishingTrigger.LocationUnknown, FishingState.TeleportingToFishingArea)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .Permit(FishingTrigger.ErrorOccurred, FishingState.Error)
            .OnEntry(OnInitializing);

        // CHECKING LOCATION state
        _machine.Configure(FishingState.CheckingLocation)
            .Permit(FishingTrigger.AtFishingSpot, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.NearFishingSpot, FishingState.WalkingToFishingSpot)
            .Permit(FishingTrigger.AtBank, FishingState.Banking)
            .Permit(FishingTrigger.NearBank, FishingState.WalkingToBank)
            .Permit(FishingTrigger.LocationUnknown, FishingState.TeleportingToFishingArea)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnCheckingLocation);

        // WALKING TO FISHING SPOT state
        _machine.Configure(FishingState.WalkingToFishingSpot)
            .Permit(FishingTrigger.ArrivedAtDestination, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.MovementFailed, FishingState.TeleportingToFishingArea)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnWalkingToFishingSpot);

        // TELEPORTING TO FISHING AREA state
        _machine.Configure(FishingState.TeleportingToFishingArea)
            .Permit(FishingTrigger.TeleportComplete, FishingState.WalkingToFishingSpot)
            .Permit(FishingTrigger.TeleportFailed, FishingState.Error)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnTeleportingToFishingArea);

        // FINDING FISHING SPOT state
        _machine.Configure(FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.FishingSpotFound, FishingState.Fishing)
            .Permit(FishingTrigger.FishingSpotNotFound, FishingState.Idling)
            .Permit(FishingTrigger.InventoryFull, FishingState.InventoryFull)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnFindingFishingSpot);

        // FISHING state
        _machine.Configure(FishingState.Fishing)
            .Permit(FishingTrigger.CaughtFish, FishingState.WaitingForFish)
            .Permit(FishingTrigger.FishingSpotMoved, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.InventoryFull, FishingState.InventoryFull)
            .Permit(FishingTrigger.StoppedFishing, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.BoostNeeded, FishingState.UsingBoostPotion)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnFishing);

        // WAITING FOR FISH state
        _machine.Configure(FishingState.WaitingForFish)
            .Permit(FishingTrigger.CaughtFish, FishingState.WaitingForFish)
            .Permit(FishingTrigger.StoppedFishing, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.FishingSpotMoved, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.InventoryFull, FishingState.InventoryFull)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnWaitingForFish);

        // INVENTORY FULL state
        _machine.Configure(FishingState.InventoryFull)
            .Permit(FishingTrigger.HasFishToDrop, FishingState.DroppingFish)
            .Permit(FishingTrigger.HasBankTeleport, FishingState.UsingBankTeleport)
            .Permit(FishingTrigger.NearBank, FishingState.WalkingToBank)
            .Permit(FishingTrigger.NoBankTeleport, FishingState.TeleportingToBank)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnInventoryFull);

        // DROPPING FISH state
        _machine.Configure(FishingState.DroppingFish)
            .Permit(FishingTrigger.AllFishDropped, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.InventoryNotFull, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnDroppingFish);

        // USING BANK TELEPORT state
        _machine.Configure(FishingState.UsingBankTeleport)
            .Permit(FishingTrigger.TeleportComplete, FishingState.OpeningBank)
            .Permit(FishingTrigger.TeleportFailed, FishingState.TeleportingToBank)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnUsingBankTeleport);

        // WALKING TO BANK state
        _machine.Configure(FishingState.WalkingToBank)
            .Permit(FishingTrigger.ArrivedAtDestination, FishingState.OpeningBank)
            .Permit(FishingTrigger.MovementFailed, FishingState.TeleportingToBank)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnWalkingToBank);

        // TELEPORTING TO BANK state
        _machine.Configure(FishingState.TeleportingToBank)
            .Permit(FishingTrigger.TeleportComplete, FishingState.WalkingToBank)
            .Permit(FishingTrigger.TeleportFailed, FishingState.Error)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnTeleportingToBank);

        // OPENING BANK state
        _machine.Configure(FishingState.OpeningBank)
            .Permit(FishingTrigger.BankOpened, FishingState.Banking)
            .Permit(FishingTrigger.BankFailed, FishingState.WalkingToBank)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnOpeningBank);

        // BANKING state
        _machine.Configure(FishingState.Banking)
            .Permit(FishingTrigger.DepositComplete, FishingState.ClosingBank)
            .Permit(FishingTrigger.BankFailed, FishingState.Error)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnBanking);

        // CLOSING BANK state
        _machine.Configure(FishingState.ClosingBank)
            .Permit(FishingTrigger.BankClosed, FishingState.ReturningToFishing)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnClosingBank);

        // RETURNING TO FISHING state
        _machine.Configure(FishingState.ReturningToFishing)
            .Permit(FishingTrigger.ArrivedAtDestination, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.TeleportComplete, FishingState.WalkingToFishingSpot)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnReturningToFishing);

        // USING BOOST POTION state
        _machine.Configure(FishingState.UsingBoostPotion)
            .Permit(FishingTrigger.BoostUsed, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.NoBoostAvailable, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnUsingBoostPotion);

        // IDLING state
        _machine.Configure(FishingState.Idling)
            .Permit(FishingTrigger.IdleComplete, FishingState.FindingFishingSpot)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnIdling);

        // ERROR state
        _machine.Configure(FishingState.Error)
            .Permit(FishingTrigger.ErrorResolved, FishingState.Initializing)
            .Permit(FishingTrigger.Stop, FishingState.Stopped)
            .OnEntry(OnError);

        // Log all transitions
        _machine.OnTransitioned(t =>
        {
            _log($"State: {t.Source} -> {t.Destination} (via {t.Trigger})");
        });
    }

    #region Public Control Methods

    public void Start()
    {
        if (_machine.CanFire(FishingTrigger.Start))
            _machine.Fire(FishingTrigger.Start);
    }

    public void Stop()
    {
        if (_machine.CanFire(FishingTrigger.Stop))
            _machine.Fire(FishingTrigger.Stop);
    }

    public void Fire(FishingTrigger trigger)
    {
        if (_machine.CanFire(trigger))
            _machine.Fire(trigger);
    }

    /// <summary>
    /// Main tick method - call this regularly to drive the state machine.
    /// </summary>
    public void Tick()
    {
        if (!IsRunning) return;

        // Check for common conditions across states
        CheckCommonConditions();
    }

    #endregion

    #region State Entry Handlers

    private void OnInitializing()
    {
        SetStatus("Initializing...");

        if (!Game.IsInjected || !Game.HasClientPointers)
        {
            _log("Waiting for game injection...");
            return;
        }

        if (!LocalPlayer.IsLoggedIn())
        {
            _log("Waiting for player to log in...");
            return;
        }

        DetermineLocation();
    }

    private void OnCheckingLocation()
    {
        SetStatus("Checking location...");
        DetermineLocation();
    }

    private void DetermineLocation()
    {
        var pos = LocalPlayer.GetTilePosition();
        var currentPos = new WorldPoint(pos.x, pos.y, pos.z);

        // Check if at fishing spot
        if (_config.FishingLocation?.Area.Contains(currentPos) == true)
        {
            Fire(FishingTrigger.AtFishingSpot);
            return;
        }

        // Check if near fishing spot (within walking distance)
        if (_config.FishingLocation != null)
        {
            var dist = currentPos.DistanceTo(_config.FishingLocation.Area.Center);
            if (dist < 100)
            {
                Fire(FishingTrigger.NearFishingSpot);
                return;
            }
        }

        // Check if at bank
        if (_config.BankLocation?.Area.Contains(currentPos) == true)
        {
            Fire(FishingTrigger.AtBank);
            return;
        }

        // Check if near bank
        if (_config.BankLocation != null)
        {
            var dist = currentPos.DistanceTo(_config.BankLocation.Area.Center);
            if (dist < 50)
            {
                Fire(FishingTrigger.NearBank);
                return;
            }
        }

        // Unknown location - need to teleport
        Fire(FishingTrigger.LocationUnknown);
    }

    private void OnWalkingToFishingSpot()
    {
        SetStatus("Walking to fishing spot...");

        if (_config.FishingLocation == null)
        {
            Fire(FishingTrigger.MovementFailed);
            return;
        }

        var target = _config.FishingLocation.Area.Center;
        _navigation.WalkTo(target, () => Fire(FishingTrigger.ArrivedAtDestination));
    }

    private void OnTeleportingToFishingArea()
    {
        SetStatus("Teleporting to fishing area...");

        if (_config.FishingLocation?.NearestLodestone == null)
        {
            Fire(FishingTrigger.TeleportFailed);
            return;
        }

        var lodestone = _config.FishingLocation.NearestLodestone.Value;
        _navigation.UseLodestone(lodestone, () => Fire(FishingTrigger.TeleportComplete));
    }

    private void OnFindingFishingSpot()
    {
        SetStatus("Looking for fishing spot...");

        // Check inventory first
        if (Inventory.IsFull)
        {
            Fire(FishingTrigger.InventoryFull);
            return;
        }

        // Look for fishing spot NPC
        var spotName = _config.FishingSpotType?.SpotName ?? "Fishing spot";
        var spots = Objects.ByName(spotName);

        if (spots.Count == 0)
        {
            _log($"No fishing spots found with name: {spotName}");
            Fire(FishingTrigger.FishingSpotNotFound);
            return;
        }

        // Find nearest spot
        var nearest = spots.OrderBy(s => s.Distance).First();
        _log($"Found fishing spot at ({nearest.TileX}, {nearest.TileY}) - distance: {nearest.Distance:F1}");

        // Interact with the fishing spot
        var actionName = GetFishingActionName(_config.FishingSpotType?.Action ?? FishingAction.Net);
        if (nearest.DoAction(1)) // 1 = first action (usually the fishing action)
        {
            Fire(FishingTrigger.FishingSpotFound);
        }
        else
        {
            Fire(FishingTrigger.FishingSpotNotFound);
        }
    }

    private void OnFishing()
    {
        SetStatus($"Fishing at {_config.FishingLocation?.Name ?? "spot"}...");
        _lastFishingCheck = DateTime.UtcNow;
        _lastInventoryCount = 28 - Inventory.FreeSlots;
    }

    private void OnWaitingForFish()
    {
        SetStatus("Fishing...");

        // Check if inventory is full
        if (Inventory.IsFull)
        {
            Fire(FishingTrigger.InventoryFull);
            return;
        }

        // Check if we caught a fish (inventory count increased)
        var currentCount = 28 - Inventory.FreeSlots;
        if (currentCount > _lastInventoryCount)
        {
            var caught = currentCount - _lastInventoryCount;
            for (int i = 0; i < caught; i++)
            {
                // Record fish caught (we'd need to identify which fish)
                if (_config.TargetFish != null)
                    _metrics.RecordFishCaught(_config.TargetFish);
            }
            _lastInventoryCount = currentCount;
            _log($"Caught {caught} fish! Total: {_metrics.TotalFishCaught}");
        }

        // Check if we're still fishing (animation check)
        var animation = LocalPlayer.GetAnimation();
        if (animation <= 0)
        {
            // No longer animating - spot may have moved or depleted
            Fire(FishingTrigger.StoppedFishing);
        }
    }

    private void OnInventoryFull()
    {
        SetStatus("Inventory full - deciding action...");
        _metrics.RecordTripCompleted();

        switch (_config.InventoryFullAction)
        {
            case InventoryFullAction.DropFish:
                Fire(FishingTrigger.HasFishToDrop);
                break;

            case InventoryFullAction.UseBankTeleport:
                if (HasBankTeleport())
                    Fire(FishingTrigger.HasBankTeleport);
                else
                    Fire(FishingTrigger.NoBankTeleport);
                break;

            case InventoryFullAction.WalkToBank:
                Fire(FishingTrigger.NearBank);
                break;

            case InventoryFullAction.UseLodestone:
            default:
                Fire(FishingTrigger.NoBankTeleport);
                break;
        }
    }

    private void OnDroppingFish()
    {
        SetStatus("Dropping fish...");

        // Drop all fish
        var fishIds = _config.FishingSpotType?.Fish.Select(f => f.ItemId).ToArray() ?? Array.Empty<int>();

        foreach (var id in fishIds)
        {
            while (Inventory.ContainsId(id))
            {
                Inventory.Drop(id);
                Thread.Sleep(100);
            }
        }

        _log("All fish dropped");
        Fire(FishingTrigger.AllFishDropped);
    }

    private void OnUsingBankTeleport()
    {
        SetStatus("Using bank teleport...");

        // TODO: Implement bank teleport logic based on configuration
        // For now, fall back to lodestone
        Fire(FishingTrigger.TeleportFailed);
    }

    private void OnWalkingToBank()
    {
        SetStatus("Walking to bank...");

        if (_config.BankLocation == null)
        {
            Fire(FishingTrigger.MovementFailed);
            return;
        }

        var target = _config.BankLocation.Area.Center;
        _navigation.WalkTo(target, () => Fire(FishingTrigger.ArrivedAtDestination));
    }

    private void OnTeleportingToBank()
    {
        SetStatus("Teleporting to bank...");

        if (_config.BankLocation?.NearestLodestone == null)
        {
            Fire(FishingTrigger.TeleportFailed);
            return;
        }

        var lodestone = _config.BankLocation.NearestLodestone.Value;
        _navigation.UseLodestone(lodestone, () => Fire(FishingTrigger.TeleportComplete));
    }

    private void OnOpeningBank()
    {
        SetStatus("Opening bank...");

        // Find bank booth or NPC
        var bankers = Objects.ByName("Bank booth");
        if (bankers.Count == 0)
            bankers = Objects.ByName("Banker");

        if (bankers.Count == 0)
        {
            _log("No bank found!");
            Fire(FishingTrigger.BankFailed);
            return;
        }

        var nearest = bankers.OrderBy(b => b.Distance).First();
        if (nearest.DoAction(1))
        {
            // Wait for bank to open
            Thread.Sleep(1000);
            if (Bank.IsOpen)
                Fire(FishingTrigger.BankOpened);
            else
                Fire(FishingTrigger.BankFailed);
        }
        else
        {
            Fire(FishingTrigger.BankFailed);
        }
    }

    private void OnBanking()
    {
        SetStatus("Depositing fish...");

        if (!Bank.IsOpen)
        {
            Fire(FishingTrigger.BankFailed);
            return;
        }

        // Deposit all fish
        var fishIds = _config.FishingSpotType?.Fish.Select(f => f.ItemId).ToArray() ?? Array.Empty<int>();

        if (fishIds.Length > 0)
        {
            // Deposit all except equipment/bait
            Bank.DepositAll();
        }
        else
        {
            Bank.DepositAll();
        }

        Thread.Sleep(500);
        _log("Fish deposited");
        Fire(FishingTrigger.DepositComplete);
    }

    private void OnClosingBank()
    {
        SetStatus("Closing bank...");
        Bank.Close();
        Thread.Sleep(300);
        Fire(FishingTrigger.BankClosed);
    }

    private void OnReturningToFishing()
    {
        SetStatus("Returning to fishing spot...");
        _metrics.RecordTripCompleted();

        switch (_config.ReturnMethod)
        {
            case ReturnToFishingMethod.Walk:
                if (_config.FishingLocation != null)
                {
                    var target = _config.FishingLocation.Area.Center;
                    _navigation.WalkTo(target, () => Fire(FishingTrigger.ArrivedAtDestination));
                }
                break;

            case ReturnToFishingMethod.UseLodestone:
                if (_config.FishingLocation?.NearestLodestone != null)
                {
                    var lodestone = _config.FishingLocation.NearestLodestone.Value;
                    _navigation.UseLodestone(lodestone, () => Fire(FishingTrigger.TeleportComplete));
                }
                break;

            default:
                Fire(FishingTrigger.ArrivedAtDestination);
                break;
        }
    }

    private void OnUsingBoostPotion()
    {
        SetStatus("Using fishing boost...");

        // TODO: Implement boost potion logic based on configuration
        // For now, just continue
        Fire(FishingTrigger.NoBoostAvailable);
    }

    private void OnIdling()
    {
        SetStatus("Waiting...");
        _idleCounter++;

        // Wait a bit then try again
        Thread.Sleep(Random.Shared.Next(2000, 4000));

        if (_idleCounter >= 3)
        {
            _idleCounter = 0;
            _log("Too many idle attempts, rechecking location...");
        }

        Fire(FishingTrigger.IdleComplete);
    }

    private void OnError()
    {
        SetStatus("Error occurred!");
        _log("Script encountered an error. Attempting recovery...");
    }

    #endregion

    #region Helper Methods

    private void SetStatus(string status)
    {
        StatusMessage = status;
    }

    private void CheckCommonConditions()
    {
        // Could add checks here for:
        // - Level up detection
        // - Random event detection
        // - Combat detection
        // - etc.
    }

    private bool HasBankTeleport()
    {
        // Check for common bank teleport items
        // TODO: Make this configurable
        var teleportItems = new[]
        {
            "Ring of duelling",
            "Ring of wealth",
            "TokKul-Zo",
            "Wicked hood"
        };

        foreach (var item in teleportItems)
        {
            if (Inventory.Contains(item))
                return true;
        }

        return false;
    }

    private static string GetFishingActionName(FishingAction action)
    {
        return action switch
        {
            FishingAction.Net => "Net",
            FishingAction.Bait => "Bait",
            FishingAction.Lure => "Lure",
            FishingAction.Cage => "Cage",
            FishingAction.Harpoon => "Harpoon",
            FishingAction.BigNet => "Big net",
            FishingAction.BarbarianFish => "Use-rod",
            FishingAction.Frenzy => "Frenzy",
            _ => "Fish"
        };
    }

    #endregion
}

/// <summary>
/// Configuration for the fishing script.
/// </summary>
public class FishingConfiguration
{
    public FishingLocation? FishingLocation { get; set; }
    public FishingSpotType? FishingSpotType { get; set; }
    public FishType? TargetFish { get; set; }
    public BankLocation? BankLocation { get; set; }

    public InventoryFullAction InventoryFullAction { get; set; } = InventoryFullAction.DropFish;
    public ReturnToFishingMethod ReturnMethod { get; set; } = ReturnToFishingMethod.Walk;

    public bool UseBoostPotions { get; set; } = false;
    public string? BoostPotionName { get; set; }

    public bool UseBankTeleport { get; set; } = false;
    public string? BankTeleportItemName { get; set; }

    /// <summary>
    /// Validate the configuration.
    /// </summary>
    public bool IsValid()
    {
        if (FishingLocation == null) return false;
        if (FishingSpotType == null) return false;

        if (InventoryFullAction != InventoryFullAction.DropFish && BankLocation == null)
            return false;

        return true;
    }
}
