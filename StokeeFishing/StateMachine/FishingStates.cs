namespace StokeeFishing.StateMachine;

/// <summary>
/// All possible states for the fishing state machine.
/// </summary>
public enum FishingState
{
    /// <summary>Script is stopped/idle.</summary>
    Stopped,

    /// <summary>Initial state - checking player location and status.</summary>
    Initializing,

    /// <summary>Checking current player position to determine next action.</summary>
    CheckingLocation,

    /// <summary>Walking to the fishing spot.</summary>
    WalkingToFishingSpot,

    /// <summary>Teleporting to lodestone near fishing spot.</summary>
    TeleportingToFishingArea,

    /// <summary>Looking for a fishing spot NPC.</summary>
    FindingFishingSpot,

    /// <summary>Actively fishing.</summary>
    Fishing,

    /// <summary>Waiting for fishing animation to complete.</summary>
    WaitingForFish,

    /// <summary>Inventory is full, deciding what to do.</summary>
    InventoryFull,

    /// <summary>Dropping fish (power fishing mode).</summary>
    DroppingFish,

    /// <summary>Using bank teleport from inventory.</summary>
    UsingBankTeleport,

    /// <summary>Walking to a bank.</summary>
    WalkingToBank,

    /// <summary>Teleporting to lodestone near bank.</summary>
    TeleportingToBank,

    /// <summary>Opening the bank interface.</summary>
    OpeningBank,

    /// <summary>Depositing fish into bank.</summary>
    Banking,

    /// <summary>Closing the bank interface.</summary>
    ClosingBank,

    /// <summary>Returning to fishing after banking.</summary>
    ReturningToFishing,

    /// <summary>Using boost potion from inventory.</summary>
    UsingBoostPotion,

    /// <summary>Handling random event or interruption.</summary>
    HandlingInterruption,

    /// <summary>Waiting/idling (anti-pattern).</summary>
    Idling,

    /// <summary>Error state - something went wrong.</summary>
    Error
}

/// <summary>
/// All possible triggers that can cause state transitions.
/// </summary>
public enum FishingTrigger
{
    // Control triggers
    Start,
    Stop,
    Pause,
    Resume,

    // Location triggers
    AtFishingSpot,
    AtBank,
    AtLodestone,
    LocationUnknown,
    NearFishingSpot,
    NearBank,

    // Fishing triggers
    FishingSpotFound,
    FishingSpotNotFound,
    FishingSpotMoved,
    StartedFishing,
    CaughtFish,
    StoppedFishing,

    // Inventory triggers
    InventoryFull,
    InventoryNotFull,
    InventoryEmpty,
    HasFishToDrop,
    AllFishDropped,
    HasBankTeleport,
    NoBankTeleport,

    // Banking triggers
    BankOpened,
    BankClosed,
    DepositComplete,
    BankFailed,

    // Movement triggers
    ArrivedAtDestination,
    MovementFailed,
    TeleportComplete,
    TeleportFailed,

    // Boost triggers
    BoostNeeded,
    BoostUsed,
    NoBoostAvailable,

    // Error triggers
    ErrorOccurred,
    ErrorResolved,
    Timeout,

    // Misc triggers
    InterruptionDetected,
    InterruptionHandled,
    IdleComplete
}

/// <summary>
/// Configuration for how the script should handle full inventory.
/// </summary>
public enum InventoryFullAction
{
    /// <summary>Drop all fish (power fishing).</summary>
    DropFish,

    /// <summary>Walk to the nearest bank.</summary>
    WalkToBank,

    /// <summary>Use a bank teleport item from inventory.</summary>
    UseBankTeleport,

    /// <summary>Teleport via lodestone network to bank.</summary>
    UseLodestone
}

/// <summary>
/// Configuration for how to return to fishing after banking.
/// </summary>
public enum ReturnToFishingMethod
{
    /// <summary>Walk back from bank.</summary>
    Walk,

    /// <summary>Use lodestone teleport.</summary>
    UseLodestone,

    /// <summary>Use a teleport item in inventory.</summary>
    UseTeleportItem
}
