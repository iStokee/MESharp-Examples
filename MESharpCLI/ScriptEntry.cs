using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MESharp.API;
using MESharp.Services;

namespace MESharpExamples.CLI
{
    /// <summary>
    /// Minimal CLI sample that relies on <see cref="ScriptRuntimeHost"/> to manage MESharp entry points.
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly ScriptRuntimeHostOptions HostOptions = new()
        {
            ScriptName = "MESharp CLI Example"
        };

        public static void Initialize() => ScriptRuntimeHost.Run(RunAsync, HostOptions);

        [UnmanagedCallersOnly]
        public static void Initialize_Native() => ScriptRuntimeHost.Run(RunAsync, HostOptions);

        public static void Shutdown() => ScriptRuntimeHost.Stop();

        [UnmanagedCallersOnly]
        public static void Shutdown_Native() => ScriptRuntimeHost.Stop();

        [UnmanagedCallersOnly]
        public static void SetLogger(IntPtr loggerCallbackPtr) => ScriptRuntimeHost.SetLogger(loggerCallbackPtr);

        private static async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!Game.IsInjected || !Game.HasClientPointers)
                    {
                        Console.WriteLine("[CLI Example] Waiting for MemoryError injection…");
                        await Task.Delay(1000, token);
                        continue;
                    }

                    if (!LocalPlayer.IsLoggedIn())
                    {
                        Console.WriteLine("[CLI Example] Player is not logged in yet.");
                        await Task.Delay(2000, token);
                        continue;
                    }

                    var (x, y, z) = LocalPlayer.GetTilePosition();
                    var playerName = Game.LocalPlayerName;
                    var coins = Inventory.FindById(995);
                    ulong totalCoins = 0;
                    foreach (var stack in coins)
                    {
                        totalCoins += stack.Amount;
                    }

                    Console.WriteLine($"[CLI Example] {playerName} @ ({x}, {y}, {z}) | Coins: {totalCoins:N0} ({coins.Count} stacks) | Free slots: {Inventory.FreeSlots}");
                    Console.WriteLine($"[CLI Example] Hover progress: {LocalPlayer.GetHoverProgress()}");

                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLI Example] Loop error: {ex.Message}");
                    await Task.Delay(1000, token);
                }
            }

            Console.WriteLine("[CLI Example] Main loop exited gracefully.");
        }
    }
}
