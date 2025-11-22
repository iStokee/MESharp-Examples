using MESharp.API;
using StokeeFishing.Data;

namespace StokeeFishing.Navigation;

/// <summary>
/// Service for handling player movement and navigation.
/// Uses the MESharp.API traversal helpers with logging and path support.
/// </summary>
public class NavigationService
{
    private readonly Action<string> _log;
    private CancellationTokenSource? _navCts;

    public NavigationService(Action<string> log)
    {
        _log = log;
    }

    /// <summary>
    /// Whether navigation is currently in progress.
    /// </summary>
    public bool IsNavigating { get; private set; }

    /// <summary>
    /// Walk to a specific tile coordinate.
    /// </summary>
    public void WalkTo(WorldPoint target, Action? onArrival = null, int timeoutMs = 30000)
    {
        WalkToAsync(target, onArrival, timeoutMs);
    }

    /// <summary>
    /// Walk to a specific tile coordinate asynchronously.
    /// </summary>
    public async void WalkToAsync(WorldPoint target, Action? onArrival = null, int timeoutMs = 30000)
    {
        if (IsNavigating)
        {
            _log("Already navigating, cancelling previous navigation...");
            CancelNavigation();
        }

        IsNavigating = true;
        _navCts = new CancellationTokenSource();

        try
        {
            // Check for predefined path
            var currentPos = Traversal.GetCurrentPosition();
            var path = Paths.FindPath(currentPos, target);

            bool result;
            if (path != null)
            {
                _log($"Using predefined path: {path.Name}");
                result = await Traversal.WalkPathAsync(path.Waypoints, 8, timeoutMs, _navCts.Token);
            }
            else
            {
                _log($"Walking directly to {target}");
                result = await Traversal.WalkToAsync(target, 5, timeoutMs, _navCts.Token);
            }

            if (result)
            {
                _log($"Arrived at {target}");
                onArrival?.Invoke();
            }
            else
            {
                _log($"Failed to reach {target}");
            }
        }
        catch (OperationCanceledException)
        {
            _log("Navigation cancelled");
        }
        finally
        {
            IsNavigating = false;
        }
    }

    /// <summary>
    /// Use a lodestone to teleport.
    /// </summary>
    public void UseLodestone(Lodestone destination, Action? onComplete = null, int timeoutMs = 15000)
    {
        UseLodestoneAsync(destination, onComplete, timeoutMs);
    }

    /// <summary>
    /// Use a lodestone to teleport asynchronously.
    /// </summary>
    public async void UseLodestoneAsync(Lodestone destination, Action? onComplete = null, int timeoutMs = 15000)
    {
        if (IsNavigating)
        {
            _log("Already navigating, cancelling previous navigation...");
            CancelNavigation();
        }

        IsNavigating = true;
        _navCts = new CancellationTokenSource();

        try
        {
            _log($"Teleporting to {LodestoneData.GetName(destination)} lodestone...");
            var result = await Traversal.LodestoneAsync(destination, timeoutMs, _navCts.Token);

            if (result)
            {
                _log($"Teleported to {LodestoneData.GetName(destination)}");
                onComplete?.Invoke();
            }
            else
            {
                _log($"Failed to teleport to {LodestoneData.GetName(destination)}");
            }
        }
        catch (OperationCanceledException)
        {
            _log("Teleport cancelled");
        }
        finally
        {
            IsNavigating = false;
        }
    }

    /// <summary>
    /// Cancel any ongoing navigation.
    /// </summary>
    public void CancelNavigation()
    {
        _navCts?.Cancel();
        _navCts = null;
        IsNavigating = false;
    }

    /// <summary>
    /// Get the current player position.
    /// </summary>
    public static WorldPoint GetCurrentPosition() => Traversal.GetCurrentPosition();

    /// <summary>
    /// Check if player is within a certain distance of a point.
    /// </summary>
    public static bool IsNear(WorldPoint target, int distance = 10)
        => Traversal.IsWithinDistance(target, distance);

    /// <summary>
    /// Wait until player stops moving or timeout.
    /// </summary>
    public static bool WaitUntilStopped(int timeoutMs = 10000)
        => Traversal.WaitWhileMoving(timeoutMs);
}

/// <summary>
/// Extension methods for navigation.
/// </summary>
public static class NavigationExtensions
{
    /// <summary>
    /// Walk to a bank location.
    /// </summary>
    public static void WalkToBank(this NavigationService nav, BankLocation bank, Action? onArrival = null)
    {
        nav.WalkTo(bank.Area.Center, onArrival);
    }

    /// <summary>
    /// Walk to a fishing location.
    /// </summary>
    public static void WalkToFishing(this NavigationService nav, FishingLocation loc, Action? onArrival = null)
    {
        nav.WalkTo(loc.Area.Center, onArrival);
    }

    /// <summary>
    /// Teleport to nearest lodestone for a fishing location.
    /// </summary>
    public static void TeleportToFishing(this NavigationService nav, FishingLocation loc, Action? onComplete = null)
    {
        if (loc.NearestLodestone == null)
        {
            onComplete?.Invoke();
            return;
        }

        nav.UseLodestone(loc.NearestLodestone.Value, onComplete);
    }

    /// <summary>
    /// Teleport to nearest lodestone for a bank.
    /// </summary>
    public static void TeleportToBank(this NavigationService nav, BankLocation bank, Action? onComplete = null)
    {
        if (bank.NearestLodestone == null)
        {
            onComplete?.Invoke();
            return;
        }

        nav.UseLodestone(bank.NearestLodestone.Value, onComplete);
    }
}
