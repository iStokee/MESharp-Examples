using MESharp.API;
using StokeeFishing.Data;

namespace StokeeFishing.Services;

/// <summary>
/// Tracks fishing session metrics including fish caught, XP, GP, and timing.
/// </summary>
public class MetricsService
{
    private readonly DateTime _sessionStart;
    private readonly SkillSession _skillSession;
    private readonly Dictionary<int, long> _fishCaught = new();
    private readonly object _lock = new();

    private int _tripCount;
    private int _currentTripFish;
    private readonly List<int> _tripFishCounts = new();
    private DateTime _tripStartTime;

    public MetricsService()
    {
        _sessionStart = DateTime.UtcNow;
        _skillSession = new SkillSession();
        _tripStartTime = _sessionStart;
    }

    #region Session Metrics

    /// <summary>
    /// Total runtime of the session.
    /// </summary>
    public TimeSpan Runtime => DateTime.UtcNow - _sessionStart;

    /// <summary>
    /// Formatted runtime string (HH:MM:SS).
    /// </summary>
    public string RuntimeFormatted => Runtime.ToString(@"hh\:mm\:ss");

    /// <summary>
    /// Total fish caught this session.
    /// </summary>
    public long TotalFishCaught
    {
        get
        {
            lock (_lock)
            {
                return _fishCaught.Values.Sum();
            }
        }
    }

    /// <summary>
    /// Fish caught per hour rate.
    /// </summary>
    public double FishPerHour
    {
        get
        {
            var hours = Runtime.TotalHours;
            return hours > 0 ? TotalFishCaught / hours : 0;
        }
    }

    /// <summary>
    /// Get count of a specific fish type caught.
    /// </summary>
    public long GetFishCount(int itemId)
    {
        lock (_lock)
        {
            return _fishCaught.TryGetValue(itemId, out var count) ? count : 0;
        }
    }

    /// <summary>
    /// Get count of a specific fish type caught.
    /// </summary>
    public long GetFishCount(FishType fish) => GetFishCount(fish.ItemId);

    /// <summary>
    /// Get all fish caught with their counts.
    /// </summary>
    public IReadOnlyDictionary<int, long> GetAllFishCounts()
    {
        lock (_lock)
        {
            return new Dictionary<int, long>(_fishCaught);
        }
    }

    #endregion

    #region XP Metrics

    /// <summary>
    /// Fishing XP gained this session.
    /// </summary>
    public int FishingXpGained => _skillSession.GetXpGained(SkillName.Fishing);

    /// <summary>
    /// Fishing XP per hour rate.
    /// </summary>
    public double FishingXpPerHour => _skillSession.GetXpPerHour(SkillName.Fishing);

    /// <summary>
    /// Fishing levels gained this session.
    /// </summary>
    public int FishingLevelsGained => _skillSession.GetLevelsGained(SkillName.Fishing);

    /// <summary>
    /// Current fishing level.
    /// </summary>
    public int CurrentFishingLevel => Skills.Get(SkillName.Fishing).CurrentLevel;

    /// <summary>
    /// XP remaining until next fishing level.
    /// </summary>
    public int XpToNextLevel
    {
        get
        {
            var skill = Skills.Get(SkillName.Fishing);
            return Skills.GetXpToNextLevel(skill);
        }
    }

    /// <summary>
    /// Estimated time to next level at current XP rate.
    /// </summary>
    public TimeSpan TimeToNextLevel => _skillSession.GetTimeToNextLevel(SkillName.Fishing);

    /// <summary>
    /// Formatted TTL string.
    /// </summary>
    public string TimeToNextLevelFormatted
    {
        get
        {
            var ttl = TimeToNextLevel;
            if (ttl == TimeSpan.MaxValue)
                return "--:--:--";
            if (ttl.TotalDays >= 1)
                return $"{(int)ttl.TotalDays}d {ttl.Hours}h {ttl.Minutes}m";
            return ttl.ToString(@"hh\:mm\:ss");
        }
    }

    #endregion

    #region GP Metrics

    /// <summary>
    /// Calculate total GP value of all fish caught.
    /// Uses cached GE prices.
    /// </summary>
    public long TotalGpValue
    {
        get
        {
            lock (_lock)
            {
                return GrandExchange.Instance.CalculateTotalValue(
                    _fishCaught.Select(kvp => (kvp.Key, kvp.Value)));
            }
        }
    }

    /// <summary>
    /// GP per hour rate.
    /// </summary>
    public double GpPerHour
    {
        get
        {
            var hours = Runtime.TotalHours;
            return hours > 0 ? TotalGpValue / hours : 0;
        }
    }

    /// <summary>
    /// Formatted GP value with K/M suffix.
    /// </summary>
    public string TotalGpFormatted => FormatGp(TotalGpValue);

    /// <summary>
    /// Formatted GP/hr with K/M suffix.
    /// </summary>
    public string GpPerHourFormatted => FormatGp((long)GpPerHour);

    #endregion

    #region Trip Metrics

    /// <summary>
    /// Number of completed banking trips.
    /// </summary>
    public int TripCount => _tripCount;

    /// <summary>
    /// Fish caught in current trip.
    /// </summary>
    public int CurrentTripFish => _currentTripFish;

    /// <summary>
    /// Duration of current trip.
    /// </summary>
    public TimeSpan CurrentTripDuration => DateTime.UtcNow - _tripStartTime;

    /// <summary>
    /// Average fish per trip.
    /// </summary>
    public double AverageFishPerTrip
    {
        get
        {
            if (_tripCount == 0) return _currentTripFish;
            return _tripFishCounts.Count > 0 ? _tripFishCounts.Average() : 0;
        }
    }

    /// <summary>
    /// Average trip duration.
    /// </summary>
    public TimeSpan AverageTripDuration
    {
        get
        {
            if (_tripCount == 0) return CurrentTripDuration;
            return TimeSpan.FromSeconds(Runtime.TotalSeconds / Math.Max(_tripCount, 1));
        }
    }

    #endregion

    #region Recording Methods

    /// <summary>
    /// Record a fish being caught.
    /// </summary>
    public void RecordFishCaught(int itemId, int amount = 1)
    {
        lock (_lock)
        {
            if (!_fishCaught.ContainsKey(itemId))
                _fishCaught[itemId] = 0;
            _fishCaught[itemId] += amount;
            _currentTripFish += amount;
        }
    }

    /// <summary>
    /// Record a fish being caught.
    /// </summary>
    public void RecordFishCaught(FishType fish, int amount = 1)
        => RecordFishCaught(fish.ItemId, amount);

    /// <summary>
    /// Record that a banking trip was completed.
    /// </summary>
    public void RecordTripCompleted()
    {
        lock (_lock)
        {
            _tripCount++;
            if (_currentTripFish > 0)
                _tripFishCounts.Add(_currentTripFish);
            _currentTripFish = 0;
            _tripStartTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Record fish dropped (for power fishing - still counts as caught).
    /// </summary>
    public void RecordFishDropped(int itemId, int amount = 1)
    {
        // Fish dropped still counts toward total caught
        // We might want to track separately in the future
    }

    #endregion

    #region Formatting Helpers

    public static string FormatGp(long value)
    {
        if (value >= 1_000_000_000)
            return $"{value / 1_000_000_000.0:F2}B";
        if (value >= 1_000_000)
            return $"{value / 1_000_000.0:F2}M";
        if (value >= 1_000)
            return $"{value / 1_000.0:F1}K";
        return value.ToString("N0");
    }

    public static string FormatNumber(long value)
    {
        if (value >= 1_000_000)
            return $"{value / 1_000_000.0:F2}M";
        if (value >= 1_000)
            return $"{value / 1_000.0:F1}K";
        return value.ToString("N0");
    }

    #endregion
}

/// <summary>
/// Snapshot of metrics for UI binding.
/// </summary>
public record MetricsSnapshot(
    string Runtime,
    long TotalFishCaught,
    string FishPerHour,
    int FishingXpGained,
    string FishingXpPerHour,
    int CurrentLevel,
    int LevelsGained,
    string XpToNextLevel,
    string TimeToNextLevel,
    string TotalGp,
    string GpPerHour,
    int TripCount,
    int CurrentTripFish,
    string AverageFishPerTrip)
{
    public static MetricsSnapshot FromService(MetricsService service)
    {
        return new MetricsSnapshot(
            service.RuntimeFormatted,
            service.TotalFishCaught,
            $"{service.FishPerHour:N0}/hr",
            service.FishingXpGained,
            $"{service.FishingXpPerHour:N0}/hr",
            service.CurrentFishingLevel,
            service.FishingLevelsGained,
            $"{service.XpToNextLevel:N0}",
            service.TimeToNextLevelFormatted,
            service.TotalGpFormatted,
            service.GpPerHourFormatted,
            service.TripCount,
            service.CurrentTripFish,
            $"{service.AverageFishPerTrip:F1}");
    }
}
