using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MESharp.API;
using MESharp.Services;
using MESharp.ScriptUI;

namespace MESharp
{
    /// <summary>
    /// CLI Example: Demonstrates how to use ScriptRuntimeHost for simplified CLI script setup
    /// with the built-in ScriptUI framework.
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
    /// ScriptUI provides:
    /// - Automatic XP tracking (updates every 2 seconds)
    /// - Thread-safe logging with 4 levels (Info, Success, Warn, Error)
    /// - Settings builder for dynamic UI controls (Toggle, Slider, Button, TextBox, ComboBox)
    /// - Session timer and auto-scroll
    ///
    /// For even simpler scripts, see ScriptBase in csharp_interop/Scripting/
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly ScriptRuntimeHostOptions HostOptions = new()
        {
            ScriptName = "MESharp CLI Example"
        };

        private static bool _uiInitialized;
        private static DateTime _lastLog = DateTime.MinValue;
        private static int _actionCount = 0;

        /// <summary>
        /// Initialize entry point - called by ME's hot-reload system via reflection.
        /// </summary>
        public static void Initialize() => ScriptRuntimeHost.Run(RunAsync, HostOptions);

        /// <summary>
        /// Shutdown entry point - called by ME's hot-reload system via reflection.
        /// </summary>
        public static void Shutdown()
        {
            ScriptUi.Shutdown();
            ScriptRuntimeHost.Stop();
        }

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

                    // First-time setup: start Script UI; skills auto-track internally
                    if (!_uiInitialized)
                    {
                        InitializeScriptUi();
                        ScriptUi.AddLog("Script UI initialized successfully!", ScriptUiLogLevel.Success);
                        ScriptUi.AddLog("Skills are auto-tracking every 2 seconds.", ScriptUiLogLevel.Info);
                    }

                    // Read player data and inventory
                    var (x, y, z) = LocalPlayer.GetTilePosition();
                    var playerName = Game.LocalPlayerName;
                    var coins = Inventory.FindById(995); // Item ID 995 = coins
                    ulong totalCoins = 0;
                    foreach (var stack in coins)
                    {
                        totalCoins += stack.Amount;
                    }

                    // Get settings values
                    var enableXp = (bool?)ScriptUi.SettingsStore["xpEnabled"] ?? true;
                    var delaySeconds = (double?)ScriptUi.SettingsStore["updateDelay"] ?? 5.0;

                    Console.WriteLine($"[CLI Example] {playerName} @ ({x}, {y}, {z}) | Coins: {totalCoins:N0} | Free slots: {Inventory.FreeSlots}");
                    Console.WriteLine($"[CLI Example] XP tracking: {(enableXp ? "ON" : "OFF")} | Update delay: {delaySeconds}s");

                    // Demonstrate all log levels periodically
                    if (DateTime.UtcNow - _lastLog > TimeSpan.FromSeconds(30))
                    {
                        _lastLog = DateTime.UtcNow;
                        _actionCount++;

                        ScriptUi.AddLog($"Status update #{_actionCount}: Coins: {totalCoins:N0} | Free slots: {Inventory.FreeSlots}", ScriptUiLogLevel.Info);

                        // Demonstrate different log levels based on conditions
                        if (totalCoins > 1000000)
                        {
                            ScriptUi.AddLog($"Great wealth! You have {totalCoins:N0} coins!", ScriptUiLogLevel.Success);
                        }

                        if (Inventory.FreeSlots < 5)
                        {
                            ScriptUi.AddLog($"Low inventory space: only {Inventory.FreeSlots} slots remaining", ScriptUiLogLevel.Warn);
                        }

                        if (Inventory.FreeSlots == 0)
                        {
                            ScriptUi.AddLog("Inventory is full! Cannot pick up items.", ScriptUiLogLevel.Error);
                        }
                    }

                    // Always pass the cancellation token to delays
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
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

        private static void InitializeScriptUi()
        {
            if (_uiInitialized)
            {
                return;
            }

            // Build a comprehensive settings layout demonstrating all available controls
            var layout = new ScriptUiLayoutBuilder()
                // Header with description
                .AddHeader("Script Controls", "Configure your script settings using the built-in UI framework.")

                // Toggle controls
                .AddToggle("xpEnabled", "Enable XP tracking", true, "If disabled, XP tab stops updating (still visible).")

                // Slider control (NEW!)
                .AddSlider("updateDelay", "Update delay (seconds)", 1, 30, 5, "How often the script checks game state.")

                // Text input
                .AddText("profile", "Profile label", string.Empty, "Optional annotation for this session.", "e.g. AFK-mining")

                // ComboBox dropdown
                .AddChoice("mode", "Script mode", new[] { "Safe", "Normal", "Aggressive" }, "Normal", "Behavior profile for the script.")

                // Visual separator (NEW!)
                .AddSeparator()

                // Section header for advanced settings
                .AddHeader("Advanced Settings", "Fine-tune script behavior")

                // More toggles
                .AddToggle("logVerbose", "Verbose logging", false, "Show detailed debug information in the log tab.")
                .AddToggle("autoPause", "Auto-pause on low health", true, "Pause script when health drops below 50%.")

                // Another slider
                .AddSlider("healthThreshold", "Health pause threshold (%)", 10, 90, 50, "Health percentage to trigger auto-pause.")

                // Visual separator
                .AddSeparator()

                // Action buttons (NEW!)
                .AddHeader("Actions", "Quick actions for testing")
                .AddButton("Test Success Log", () =>
                {
                    ScriptUi.AddLog("Button clicked! This is a success message.", ScriptUiLogLevel.Success);
                })
                .AddButton("Test Warning Log", () =>
                {
                    ScriptUi.AddLog("This is a warning message from a button.", ScriptUiLogLevel.Warn);
                })
                .AddButton("Clear Logs", () =>
                {
                    // Note: There's no Clear method exposed, but we can add a lot of messages
                    ScriptUi.AddLog("Logs cleared (simulated).", ScriptUiLogLevel.Info);
                })

                .Build();

            ScriptUi.ConfigureLayout(layout);
            ScriptUi.Show();
            _uiInitialized = true;
        }

        // Skills tab auto-tracks all skills every 2 seconds - no manual intervention needed!
    }
}
