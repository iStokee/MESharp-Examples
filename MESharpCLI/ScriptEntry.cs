using System;
using System.Threading;
using System.Threading.Tasks;
using MESharp.API;
using MESharp.Services;

namespace MESharp
{
    /// <summary>
    /// CLI Example: Demonstrates how to use ScriptRuntimeHost for simplified CLI script setup.
    ///
    /// REQUIREMENTS FOR HOT-RELOAD:
    /// - public static class ScriptEntry in MESharp namespace
    /// - public static void Initialize()
    /// - public static void Shutdown()
    ///
    /// This example uses ScriptRuntimeHost which automatically handles:
    /// - Cancellation token management (passed to RunAsync)
    /// - Shutdown signal registration with ShutdownMonitor
    /// - Console redirection and logging
    ///
    /// For even simpler scripts, see ScriptBase in csharp_interop/Scripting/
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly ScriptRuntimeHostOptions HostOptions = new()
        {
            ScriptName = "MESharp CLI Example"
        };

        /// <summary>
        /// Initialize entry point - called by ME's hot-reload system via reflection.
        /// </summary>
        public static void Initialize() => ScriptRuntimeHost.Run(RunAsync, HostOptions);

        /// <summary>
        /// Shutdown entry point - called by ME's hot-reload system via reflection.
        /// </summary>
        public static void Shutdown() => ScriptRuntimeHost.Stop();

        /// <summary>
        /// Main script logic - runs asynchronously with cancellation token support.
        ///
        /// The cancellation token is provided by ScriptRuntimeHost and signals when the script
        /// should stop (e.g., during hot-reload or shutdown).
        ///
        /// IMPORTANT: Always check token.IsCancellationRequested in loops and pass the token
        /// to async operations like Task.Delay. This allows graceful shutdown and prevents
        /// the script from continuing to run after unload.
        /// </summary>
        private static async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Wait for ME to inject into the game client
                    if (!Game.IsInjected || !Game.HasClientPointers)
                    {
                        Console.WriteLine("[CLI Example] Waiting for MemoryError injection…");
                        await Task.Delay(1000, token);
                        continue;
                    }

                    // Wait for player to log in
                    if (!LocalPlayer.IsLoggedIn())
                    {
                        Console.WriteLine("[CLI Example] Player is not logged in yet.");
                        await Task.Delay(2000, token);
                        continue;
                    }

                    // Example: Read player data and inventory
                    var (x, y, z) = LocalPlayer.GetTilePosition();
                    var playerName = Game.LocalPlayerName;
                    var coins = Inventory.FindById(995); // Item ID 995 = coins
                    ulong totalCoins = 0;
                    foreach (var stack in coins)
                    {
                        totalCoins += stack.Amount;
                    }

                    Console.WriteLine($"[CLI Example] {playerName} @ ({x}, {y}, {z}) | Coins: {totalCoins:N0} ({coins.Count} stacks) | Free slots: {Inventory.FreeSlots}");
                    Console.WriteLine($"[CLI Example] Hover progress: {LocalPlayer.GetHoverProgress()}");

                    // Always pass the cancellation token to delays
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // Expected when shutdown is requested - exit gracefully
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
