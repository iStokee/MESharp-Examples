using MESharp.API;

namespace StokeeFishing.Data;

/// <summary>
/// Known bank locations in the game.
/// </summary>
public record BankLocation(
    string Name,
    WorldArea Area,
    Lodestone? NearestLodestone = null,
    int LodestoneDistance = 0)
{
    // Placeholder coordinates - need to be filled in with actual RS3 coordinates

    // Major banks
    public static readonly BankLocation Lumbridge = new(
        "Lumbridge Castle",
        new WorldArea(new(3207, 3220), new(3210, 3216)),
        Lodestone.Lumbridge, 50);

    public static readonly BankLocation VarrockWest = new(
        "Varrock West",
        new WorldArea(new(3180, 3436), new(3185, 3432)),
        Lodestone.Varrock, 100);

    public static readonly BankLocation VarrockEast = new(
        "Varrock East",
        new WorldArea(new(3250, 3422), new(3257, 3418)),
        Lodestone.Varrock, 150);

    public static readonly BankLocation Edgeville = new(
        "Edgeville",
        new WorldArea(new(3091, 3498), new(3094, 3489)),
        Lodestone.Edgeville, 30);

    public static readonly BankLocation FaladorWest = new(
        "Falador West",
        new WorldArea(new(2943, 3373), new(2949, 3368)),
        Lodestone.Falador, 100);

    public static readonly BankLocation FaladorEast = new(
        "Falador East",
        new WorldArea(new(3009, 3358), new(3018, 3355)),
        Lodestone.Falador, 120);

    public static readonly BankLocation Draynor = new(
        "Draynor Village",
        new WorldArea(new(3092, 3245), new(3097, 3240)),
        Lodestone.Draynor, 20);

    public static readonly BankLocation AlKharid = new(
        "Al Kharid",
        new WorldArea(new(3269, 3167), new(3272, 3161)),
        Lodestone.AlKharid, 50);

    public static readonly BankLocation Catherby = new(
        "Catherby",
        new WorldArea(new(2806, 3441), new(2812, 3438)),
        Lodestone.Catherby, 30);

    public static readonly BankLocation Seers = new(
        "Seers' Village",
        new WorldArea(new(2721, 3493), new(2730, 3490)),
        Lodestone.Seers, 50);

    public static readonly BankLocation FishingGuild = new(
        "Fishing Guild",
        new WorldArea(new(2585, 3422), new(2590, 3418)),
        Lodestone.Seers, 200); // Closest lodestone

    public static readonly BankLocation Ardougne = new(
        "Ardougne North",
        new WorldArea(new(2612, 3332), new(2621, 3330)),
        Lodestone.Ardougne, 60);

    public static readonly BankLocation Menaphos = new(
        "Menaphos Port District",
        new WorldArea(new(3118, 2713), new(3126, 2709)),
        Lodestone.Menaphos, 100);

    public static readonly BankLocation Prifddinas = new(
        "Prifddinas",
        new WorldArea(new(2219, 3262), new(2228, 3257)),
        Lodestone.Prifddinas, 50);

    public static readonly BankLocation DeepSeaHub = new(
        "Deep Sea Fishing Hub",
        new WorldArea(new(2595, 3415), new(2605, 3410)), // Placeholder - DSF is instanced
        null, 0);

    public static IReadOnlyList<BankLocation> All => new[]
    {
        Lumbridge, VarrockWest, VarrockEast, Edgeville,
        FaladorWest, FaladorEast, Draynor, AlKharid,
        Catherby, Seers, FishingGuild, Ardougne,
        Menaphos, Prifddinas, DeepSeaHub
    };
}

/// <summary>
/// Known fishing locations in the game.
/// </summary>
public record FishingLocation(
    string Name,
    WorldArea Area,
    FishingSpotType SpotType,
    BankLocation? NearestBank = null,
    Lodestone? NearestLodestone = null,
    string? Requirements = null)
{
    // Placeholder coordinates - need actual RS3 coords

    // Lumbridge area
    public static readonly FishingLocation LumbridgeCrayfish = new(
        "Lumbridge Swamp (Crayfish)",
        new WorldArea(new(3238, 3254), new(3248, 3242)),
        FishingSpotType.NetBaitLow,
        BankLocation.Lumbridge,
        Lodestone.Lumbridge);

    public static readonly FishingLocation LumbridgeRiver = new(
        "Lumbridge River",
        new WorldArea(new(3238, 3254), new(3248, 3242)),
        FishingSpotType.LureRiver,
        BankLocation.Lumbridge,
        Lodestone.Lumbridge);

    // Barbarian Village
    public static readonly FishingLocation BarbarianVillage = new(
        "Barbarian Village",
        new WorldArea(new(3100, 3435), new(3110, 3425)),
        FishingSpotType.LureRiver,
        BankLocation.Edgeville,
        Lodestone.Edgeville);

    // Draynor area
    public static readonly FishingLocation DraynorVillage = new(
        "Draynor Village",
        new WorldArea(new(3085, 3230), new(3090, 3225)),
        FishingSpotType.NetBaitLow,
        BankLocation.Draynor,
        Lodestone.Draynor);

    // Karamja
    public static readonly FishingLocation KaramjaDock = new(
        "Karamja (Musa Point)",
        new WorldArea(new(2920, 3180), new(2930, 3170)),
        FishingSpotType.CageLobster,
        null, // No nearby bank
        Lodestone.Karamja);

    // Catherby
    public static readonly FishingLocation Catherby = new(
        "Catherby",
        new WorldArea(new(2836, 3435), new(2860, 3425)),
        FishingSpotType.HarpoonTunaSwordfish,
        BankLocation.Catherby,
        Lodestone.Catherby);

    public static readonly FishingLocation CatherbyCage = new(
        "Catherby (Lobster/Tuna)",
        new WorldArea(new(2836, 3435), new(2860, 3425)),
        FishingSpotType.CageLobster,
        BankLocation.Catherby,
        Lodestone.Catherby);

    // Fishing Guild
    public static readonly FishingLocation FishingGuild = new(
        "Fishing Guild",
        new WorldArea(new(2595, 3422), new(2615, 3405)),
        FishingSpotType.HarpoonShark,
        BankLocation.FishingGuild,
        null,
        "63 Fishing required");

    // Barbarian Fishing
    public static readonly FishingLocation OttosGrotto = new(
        "Otto's Grotto",
        new WorldArea(new(2500, 3495), new(2510, 3485)),
        FishingSpotType.Barbarian,
        null, // No nearby bank - powerfish
        Lodestone.Seers,
        "Barbarian Training started, 48 Fishing, 45 Strength, 45 Agility");

    // Piscatoris
    public static readonly FishingLocation Piscatoris = new(
        "Piscatoris Fishing Colony",
        new WorldArea(new(2307, 3700), new(2330, 3690)),
        FishingSpotType.Monkfish,
        null, // Has deposit box after quest
        null,
        "Swan Song quest completed");

    // Living Rock Caverns
    public static readonly FishingLocation LivingRockCaverns = new(
        "Living Rock Caverns",
        new WorldArea(new(3640, 5100), new(3660, 5080)),
        FishingSpotType.Rocktail,
        null, // Has deposit box
        Lodestone.Falador,
        "77 Mining recommended for safety, 90 Fishing for rocktail");

    // Menaphos
    public static readonly FishingLocation MenaphosPorts = new(
        "Menaphos Port District",
        new WorldArea(new(3100, 2720), new(3130, 2700)),
        FishingSpotType.Menaphos,
        BankLocation.Menaphos,
        Lodestone.Menaphos,
        "The Jack of Spades partial completion");

    // Deep Sea Fishing Hub
    public static readonly FishingLocation DeepSeaHub = new(
        "Deep Sea Fishing Hub",
        new WorldArea(new(2595, 3415), new(2620, 3400)),
        FishingSpotType.HarpoonShark, // Multiple spot types available
        BankLocation.DeepSeaHub,
        null,
        "68 Fishing minimum");

    // Shilo Village
    public static readonly FishingLocation ShiloVillage = new(
        "Shilo Village",
        new WorldArea(new(2855, 2970), new(2870, 2955)),
        FishingSpotType.LureRiver,
        null, // Has bank inside village
        null,
        "Shilo Village quest completed");

    public static IReadOnlyList<FishingLocation> All => new[]
    {
        LumbridgeCrayfish, LumbridgeRiver, BarbarianVillage, DraynorVillage,
        KaramjaDock, Catherby, CatherbyCage, FishingGuild,
        OttosGrotto, Piscatoris, LivingRockCaverns,
        MenaphosPorts, DeepSeaHub, ShiloVillage
    };

    /// <summary>
    /// Get fishing locations that support a specific fish type.
    /// </summary>
    public static IEnumerable<FishingLocation> GetLocationsForFish(FishType fish)
        => All.Where(loc => loc.SpotType.Fish.Contains(fish));

    /// <summary>
    /// Get fishing locations that have nearby banking.
    /// </summary>
    public static IEnumerable<FishingLocation> GetLocationsWithBank()
        => All.Where(loc => loc.NearestBank != null);
}

/// <summary>
/// Waypoint paths between locations.
/// </summary>
public static class Paths
{
    /// <summary>
    /// Represents a path with multiple waypoints.
    /// </summary>
    public record WaypointPath(string Name, params WorldPoint[] Waypoints)
    {
        public WorldPoint Start => Waypoints.FirstOrDefault();
        public WorldPoint End => Waypoints.LastOrDefault();

        /// <summary>
        /// Get the reversed path (for return trips).
        /// </summary>
        public WaypointPath Reversed() => new($"{Name} (Reversed)", Waypoints.Reverse().ToArray());
    }

    // Placeholder paths - need actual RS3 coordinates and tested routes

    public static readonly WaypointPath EdgevilleLodestoneToBank = new(
        "Edgeville Lodestone to Bank",
        new WorldPoint(3067, 3505),  // Lodestone
        new WorldPoint(3093, 3493)   // Bank
    );

    public static readonly WaypointPath CatherbyLodestoneToFishing = new(
        "Catherby Lodestone to Fishing",
        new WorldPoint(2811, 3449),  // Lodestone
        new WorldPoint(2836, 3431)   // Fishing spot
    );

    public static readonly WaypointPath CatherbyBankToFishing = new(
        "Catherby Bank to Fishing",
        new WorldPoint(2809, 3440),  // Bank
        new WorldPoint(2836, 3431)   // Fishing spot
    );

    public static readonly WaypointPath BarbarianVillageLodestoneToFishing = new(
        "Edgeville Lodestone to Barbarian Village Fishing",
        new WorldPoint(3067, 3505),  // Edgeville Lodestone
        new WorldPoint(3100, 3430)   // Barbarian Village fishing
    );

    public static readonly WaypointPath FishingGuildToBank = new(
        "Fishing Guild Fishing to Bank",
        new WorldPoint(2605, 3414),  // Fishing spot
        new WorldPoint(2588, 3420)   // Bank
    );

    /// <summary>
    /// Try to find a path between two locations.
    /// Returns null if no predefined path exists.
    /// </summary>
    public static WaypointPath? FindPath(WorldPoint from, WorldPoint to, int threshold = 20)
    {
        // Check all defined paths
        var allPaths = new[]
        {
            EdgevilleLodestoneToBank,
            CatherbyLodestoneToFishing,
            CatherbyBankToFishing,
            BarbarianVillageLodestoneToFishing,
            FishingGuildToBank
        };

        foreach (var path in allPaths)
        {
            if (path.Start.IsWithin(from, threshold) && path.End.IsWithin(to, threshold))
                return path;

            // Check reversed
            var reversed = path.Reversed();
            if (reversed.Start.IsWithin(from, threshold) && reversed.End.IsWithin(to, threshold))
                return reversed;
        }

        return null;
    }
}
