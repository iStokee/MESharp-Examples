using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MESharp.API;

namespace MESharpExamples.Portables.Internal
{
    internal static class StationCatalog
    {
        public static IReadOnlyList<PortableStationDefinition> AllStations { get; } = new[]
        {
            new PortableStationDefinition(
                "Portable Brazier",
                "Portable brazier",
                StationTarget.Npc,
                "Feeds logs or bones for Firemaking/Prayer training.",
                SkillName.Firemaking,
                new[]
                {
                    new StationMode("Add Logs", "Burn logs for Firemaking XP.", TimeSpan.FromSeconds(14)),
                    new StationMode("Add Bones", "Offer bones for Prayer XP.", TimeSpan.FromSeconds(16))
                }),
            new PortableStationDefinition(
                "Portable Crafter",
                "Portable crafter",
                StationTarget.Npc,
                "Mobile crafting station covering jewelry, leatherwork, and clay.",
                SkillName.Crafting,
                new[]
                {
                    new StationMode("Craft", "Standard crafting actions.", TimeSpan.FromSeconds(18)),
                    new StationMode("Cut Gems", "Loop cutting gems.", TimeSpan.FromSeconds(20)),
                    new StationMode("Clay Crafting", "Soft clay processing.", TimeSpan.FromSeconds(22)),
                    new StationMode("Tan Leather", "Automatically tan hides.", TimeSpan.FromSeconds(24))
                }),
            new PortableStationDefinition(
                "Portable Fletcher",
                "Portable fletcher",
                StationTarget.Npc,
                "Fletching helper for bows, ammo, and stringing.",
                SkillName.Fletching,
                new[]
                {
                    new StationMode("Fletch", "Cut logs into bows.", TimeSpan.FromSeconds(15)),
                    new StationMode("Ammo", "Create ammo batches.", TimeSpan.FromSeconds(15)),
                    new StationMode("String", "Attach strings to bows.", TimeSpan.FromSeconds(17))
                }),
            new PortableStationDefinition(
                "Portable Range",
                "Portable range",
                StationTarget.Object,
                "Portable cooker that boosts success chance.",
                SkillName.Cooking,
                new[]
                {
                    new StationMode("Cook", "Cook food batches.", TimeSpan.FromSeconds(18))
                }),
            new PortableStationDefinition(
                "Portable Sawmill",
                "Portable sawmill",
                StationTarget.Npc,
                "Train Construction by planking logs anywhere.",
                SkillName.Construction,
                new[]
                {
                    new StationMode("Make Planks", "Convert logs into planks.", TimeSpan.FromSeconds(20))
                }),
            new PortableStationDefinition(
                "Portable Well",
                "Portable well",
                StationTarget.Npc,
                "Mix unf/final potions with a small XP boost.",
                SkillName.Herblore,
                new[]
                {
                    new StationMode("(Unf) Potions", "Create unfinished potions.", TimeSpan.FromSeconds(12)),
                    new StationMode("Finished Potions", "Finalize potions in batches.", TimeSpan.FromSeconds(16))
                }),
            new PortableStationDefinition(
                "Portable Workbench",
                "Portable workbench",
                StationTarget.Object,
                "Smithing-style utility station.",
                SkillName.Smithing,
                new[]
                {
                    new StationMode("Assemble", "Assemble components.", TimeSpan.FromSeconds(18))
                })
        };
    }

    internal enum StationTarget
    {
        Npc,
        Object
    }

    internal sealed record PortableStationDefinition(
        string Name,
        string SearchName,
        StationTarget Target,
        string Description,
        SkillName? PrimarySkill,
        IReadOnlyList<StationMode> Modes);

    internal sealed record StationMode(string Name, string Description, TimeSpan CycleDuration);

    internal sealed record PortableRunOptions(PortableStationDefinition Station, StationMode Mode, int BankPreset);

    internal sealed record SkillSnapshot(string SkillName, int Level, int LevelsGained, int XpGained, double XpPerHour)
    {
        public static SkillSnapshot Empty { get; } = new("—", 0, 0, 0, 0);
    }

    internal sealed class SkillTracker
    {
        private readonly SkillName? _skill;
        private readonly SkillSession _session;

        public SkillTracker(SkillName? skill)
        {
            _skill = skill;
            _session = new SkillSession();
        }

        public SkillSnapshot Capture()
        {
            if (_skill is null)
            {
                return SkillSnapshot.Empty;
            }

            var live = Skills.Get(_skill.Value);
            var xpGained = _session.GetXpGained(_skill.Value);
            var levels = _session.GetLevelsGained(_skill.Value);
            var xpHour = Math.Round(_session.GetXpPerHour(_skill.Value));
            return new SkillSnapshot(live.Name, live.CurrentLevel, levels, xpGained, xpHour);
        }
    }

    internal sealed class PortablesRunner : IDisposable
    {
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private int _cycleCount;
        private readonly object _gate = new();

        public bool IsRunning => _loopTask is { IsCompleted: false };

        public event Action<string>? StatusChanged;
        public event Action<string>? Log;
        public event Action<SkillSnapshot>? SkillSnapshotAvailable;
        public event Action<int>? CycleCountChanged;

        public bool Start(PortableRunOptions options)
        {
            lock (_gate)
            {
                if (IsRunning)
                {
                    return false;
                }

                _cts = new CancellationTokenSource();
                var tracker = new SkillTracker(options.Station.PrimarySkill);
                _loopTask = Task.Run(() => RunLoopAsync(options, tracker, _cts.Token));
                return true;
            }
        }

        public async Task StopAsync()
        {
            Task? loop;
            CancellationTokenSource? cts;
            lock (_gate)
            {
                loop = _loopTask;
                cts = _cts;
                _loopTask = null;
                _cts = null;
            }

            if (loop is null || cts is null)
            {
                return;
            }

            try
            {
                cts.Cancel();
                await loop.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
                cts.Dispose();
                Interlocked.Exchange(ref _cycleCount, 0);
                CycleCountChanged?.Invoke(0);
                StatusChanged?.Invoke("Idle");
            }
        }

        public async Task ExecuteTestCycleAsync(PortableRunOptions options, CancellationToken cancellationToken)
        {
            var tracker = new SkillTracker(options.Station.PrimarySkill);
            await PerformCycleAsync(options, tracker, cancellationToken, countCycle: false).ConfigureAwait(false);
        }

        private async Task RunLoopAsync(PortableRunOptions options, SkillTracker tracker, CancellationToken token)
        {
            StatusChanged?.Invoke($"Starting {options.Station.Name} ({options.Mode.Name})");
            while (!token.IsCancellationRequested)
            {
                await PerformCycleAsync(options, tracker, token, countCycle: true).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
            }
        }

        private async Task PerformCycleAsync(PortableRunOptions options, SkillTracker tracker, CancellationToken token, bool countCycle)
        {
            token.ThrowIfCancellationRequested();

            if (!Game.IsInjected || !Game.HasClientPointers)
            {
                StatusChanged?.Invoke("Waiting for MemoryError injection…");
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                return;
            }

            if (!LocalPlayer.IsLoggedIn())
            {
                StatusChanged?.Invoke("Player not logged in.");
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                return;
            }

            await EnsurePresetAsync(options.BankPreset, token).ConfigureAwait(false);
            await InteractWithStationAsync(options, token).ConfigureAwait(false);

            var snapshot = tracker.Capture();
            SkillSnapshotAvailable?.Invoke(snapshot);

            if (countCycle)
            {
                var cycle = Interlocked.Increment(ref _cycleCount);
                CycleCountChanged?.Invoke(cycle);
            }
        }

        private async Task EnsurePresetAsync(int bankPreset, CancellationToken token)
        {
            if (Inventory.FreeSlots >= 4)
            {
                return;
            }

            StatusChanged?.Invoke($"Re-stocking (preset {bankPreset})…");
            Log?.Invoke($"[{DateTime.Now:T}] Inventory low · simulating preset {bankPreset}.");
            await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
        }

        private async Task InteractWithStationAsync(PortableRunOptions options, CancellationToken token)
        {
            var (location, distance) = FindStation(options.Station);
            if (location is null)
            {
                Log?.Invoke($"[{DateTime.Now:T}] Could not find {options.Station.SearchName} nearby.");
            }
            else
            {
                Log?.Invoke($"[{DateTime.Now:T}] Found {location} at {distance:N1}u.");
            }

            StatusChanged?.Invoke($"{options.Station.Name}: {options.Mode.Name}");
            Log?.Invoke($"[{DateTime.Now:T}] Executing {options.Mode.Name} cycle ({options.Mode.CycleDuration.TotalSeconds:N0}s).");
            await Task.Delay(options.Mode.CycleDuration, token).ConfigureAwait(false);
        }

        private static (string? name, float distance) FindStation(PortableStationDefinition station)
        {
            if (station.Target == StationTarget.Npc)
            {
                var npc = Npcs.ByName(station.SearchName).OrderBy(n => n.Distance).FirstOrDefault();
                return npc is null ? (null, 0f) : ($"NPC '{npc.Name}'", npc.Distance);
            }

            var obj = Objects.ByName(station.SearchName).OrderBy(o => o.Distance).FirstOrDefault();
            return obj is null ? (null, 0f) : ($"Object '{obj.Name}'", obj.Distance);
        }

        public void Dispose() => StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
