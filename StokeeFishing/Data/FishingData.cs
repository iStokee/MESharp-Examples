namespace StokeeFishing.Data;

/// <summary>
/// Type of fishing action to perform at a fishing spot.
/// </summary>
public enum FishingAction
{
    Net,
    Bait,
    Lure,
    Cage,
    Harpoon,
    BigNet,
    BarbarianFish,
    Frenzy
}

/// <summary>
/// Equipment required for fishing.
/// </summary>
public enum FishingEquipment
{
    None,
    SmallFishingNet,
    BigFishingNet,
    FishingRod,
    FlyFishingRod,
    BarbarianRod,
    Harpoon,
    LobsterPot,
    CrayfishCage,
    FishingRodOMatic // Invention item
}

/// <summary>
/// Bait required for certain fishing methods.
/// </summary>
public enum FishingBait
{
    None,
    FishingBait,      // Standard bait
    Feathers,         // Fly fishing
    StripyFeathers,   // Rainbow fish
    LivingMinerals,   // Rocktails
    Roe,              // Barbarian
    Caviar,           // Barbarian
    Offcuts           // Prawn perks
}

/// <summary>
/// Represents a type of fish that can be caught.
/// </summary>
public record FishType(
    string Name,
    int ItemId,
    int LevelRequired,
    double XpPerCatch,
    FishingAction Action,
    FishingEquipment Equipment,
    FishingBait Bait = FishingBait.None)
{
    // Placeholder IDs - these need to be filled in with actual RS3 item IDs
    // Can be looked up at https://runescape.wiki/w/Module:GEIDs/data

    // Low level fish (1-30)
    public static readonly FishType Shrimp = new("Raw shrimps", 317, 1, 10, FishingAction.Net, FishingEquipment.SmallFishingNet);
    public static readonly FishType Crayfish = new("Raw crayfish", 13435, 1, 10, FishingAction.Cage, FishingEquipment.CrayfishCage);
    public static readonly FishType Sardine = new("Raw sardine", 327, 5, 20, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);
    public static readonly FishType Herring = new("Raw herring", 345, 10, 30, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);
    public static readonly FishType Anchovies = new("Raw anchovies", 321, 15, 40, FishingAction.Net, FishingEquipment.SmallFishingNet);
    public static readonly FishType Trout = new("Raw trout", 335, 20, 50, FishingAction.Lure, FishingEquipment.FlyFishingRod, FishingBait.Feathers);
    public static readonly FishType Pike = new("Raw pike", 349, 25, 60, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);
    public static readonly FishType Salmon = new("Raw salmon", 331, 30, 70, FishingAction.Lure, FishingEquipment.FlyFishingRod, FishingBait.Feathers);

    // Mid level fish (35-62)
    public static readonly FishType Tuna = new("Raw tuna", 359, 35, 80, FishingAction.Harpoon, FishingEquipment.Harpoon);
    public static readonly FishType Lobster = new("Raw lobster", 377, 40, 90, FishingAction.Cage, FishingEquipment.LobsterPot);
    public static readonly FishType Bass = new("Raw bass", 363, 46, 100, FishingAction.BigNet, FishingEquipment.BigFishingNet);
    public static readonly FishType Swordfish = new("Raw swordfish", 371, 50, 100, FishingAction.Harpoon, FishingEquipment.Harpoon);
    public static readonly FishType Monkfish = new("Raw monkfish", 7944, 62, 120, FishingAction.Net, FishingEquipment.SmallFishingNet);

    // High level fish (65+)
    public static readonly FishType Shark = new("Raw shark", 383, 76, 110, FishingAction.Harpoon, FishingEquipment.Harpoon);
    public static readonly FishType Cavefish = new("Raw cavefish", 15264, 85, 300, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);
    public static readonly FishType Rocktail = new("Raw rocktail", 15270, 90, 380, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.LivingMinerals);
    public static readonly FishType Sailfish = new("Raw sailfish", 42247, 97, 420, FishingAction.Harpoon, FishingEquipment.Harpoon);

    // Barbarian fishing (48+)
    public static readonly FishType LeapingTrout = new("Leaping trout", 11328, 48, 50, FishingAction.BarbarianFish, FishingEquipment.BarbarianRod);
    public static readonly FishType LeapingSalmon = new("Leaping salmon", 11330, 58, 70, FishingAction.BarbarianFish, FishingEquipment.BarbarianRod);
    public static readonly FishType LeapingSturgeon = new("Leaping sturgeon", 11332, 70, 80, FishingAction.BarbarianFish, FishingEquipment.BarbarianRod);

    // Menaphos fish (52-72)
    public static readonly FishType DesertSole = new("Desert sole", 43206, 52, 60, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);
    public static readonly FishType Catfish = new("Catfish", 43208, 60, 85, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);
    public static readonly FishType Beltfish = new("Beltfish", 43210, 72, 130, FishingAction.Bait, FishingEquipment.FishingRod, FishingBait.FishingBait);

    // Deep Sea Hub fish
    public static readonly FishType MagneticMinnow = new("Magnetic minnow", 42250, 68, 16, FishingAction.Net, FishingEquipment.SmallFishingNet);
    public static readonly FishType GreenBlubberJellyfish = new("Raw green blubber jellyfish", 42256, 72, 130, FishingAction.Net, FishingEquipment.SmallFishingNet);
    public static readonly FishType BlueBlubberJellyfish = new("Raw blue blubber jellyfish", 42258, 91, 185, FishingAction.Net, FishingEquipment.SmallFishingNet);

    /// <summary>
    /// Get all defined fish types.
    /// </summary>
    public static IReadOnlyList<FishType> All => new[]
    {
        Shrimp, Crayfish, Sardine, Herring, Anchovies, Trout, Pike, Salmon,
        Tuna, Lobster, Bass, Swordfish, Monkfish,
        Shark, Cavefish, Rocktail, Sailfish,
        LeapingTrout, LeapingSalmon, LeapingSturgeon,
        DesertSole, Catfish, Beltfish,
        MagneticMinnow, GreenBlubberJellyfish, BlueBlubberJellyfish
    };

    /// <summary>
    /// Get fish types available at a given fishing level.
    /// </summary>
    public static IEnumerable<FishType> GetAvailableForLevel(int level)
        => All.Where(f => f.LevelRequired <= level).OrderByDescending(f => f.LevelRequired);
}

/// <summary>
/// Fish that can be caught together at the same spot.
/// </summary>
public record FishingSpotType(
    string Name,
    string SpotName,      // NPC name of the fishing spot
    FishingAction Action,
    params FishType[] Fish)
{
    // Net/Bait spots
    public static readonly FishingSpotType NetBaitLow = new("Net/Bait (Low)", "Fishing spot", FishingAction.Net, FishType.Shrimp, FishType.Anchovies);
    public static readonly FishingSpotType BaitLow = new("Bait (Low)", "Fishing spot", FishingAction.Bait, FishType.Sardine, FishType.Herring);

    // Lure/Bait spots (rivers)
    public static readonly FishingSpotType LureRiver = new("Fly Fishing", "Fishing spot", FishingAction.Lure, FishType.Trout, FishType.Salmon);
    public static readonly FishingSpotType BaitRiver = new("Bait (Pike)", "Fishing spot", FishingAction.Bait, FishType.Pike);

    // Cage/Harpoon spots
    public static readonly FishingSpotType CageLobster = new("Cage (Lobster)", "Fishing spot", FishingAction.Cage, FishType.Lobster);
    public static readonly FishingSpotType HarpoonTunaSwordfish = new("Harpoon", "Fishing spot", FishingAction.Harpoon, FishType.Tuna, FishType.Swordfish);

    // Net/Harpoon spots (sharks)
    public static readonly FishingSpotType HarpoonShark = new("Harpoon (Shark)", "Fishing spot", FishingAction.Harpoon, FishType.Shark);
    public static readonly FishingSpotType BigNetBass = new("Big Net", "Fishing spot", FishingAction.BigNet, FishType.Bass);

    // Special spots
    public static readonly FishingSpotType Monkfish = new("Net (Monkfish)", "Fishing spot", FishingAction.Net, FishType.Monkfish);
    public static readonly FishingSpotType Barbarian = new("Barbarian", "Fishing spot", FishingAction.BarbarianFish, FishType.LeapingTrout, FishType.LeapingSalmon, FishType.LeapingSturgeon);
    public static readonly FishingSpotType Rocktail = new("Rocktail", "Fishing spot", FishingAction.Bait, FishType.Rocktail);
    public static readonly FishingSpotType Menaphos = new("Menaphos", "Fishing spot", FishingAction.Bait, FishType.DesertSole, FishType.Catfish, FishType.Beltfish);

    public static IReadOnlyList<FishingSpotType> All => new[]
    {
        NetBaitLow, BaitLow, LureRiver, BaitRiver,
        CageLobster, HarpoonTunaSwordfish, HarpoonShark, BigNetBass,
        Monkfish, Barbarian, Rocktail, Menaphos
    };
}
