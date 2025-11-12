using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MESharp.API;
using MESharpExamples.Portables.Internal;

namespace MESharpExamples.Portables
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> _logEntries = new();
        private readonly PortablesRunner _runner = new();
        private readonly DispatcherTimer _uiTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        private DateTime? _runStartedUtc;
        private int _cycleCount;

        public MainWindow()
        {
            InitializeComponent();
            LogList.ItemsSource = _logEntries;

            StationCombo.ItemsSource = StationCatalog.AllStations;
            StationCombo.SelectedIndex = 1; // Portable Crafter by default

            PresetCombo.ItemsSource = Enumerable.Range(1, 9).Select(i => $"Preset {i}");
            PresetCombo.SelectedIndex = 0;

            _runner.Log += OnRunnerLog;
            _runner.StatusChanged += OnRunnerStatusChanged;
            _runner.SkillSnapshotAvailable += OnSkillSnapshotAvailable;
            _runner.CycleCountChanged += OnCycleCountChanged;

            _uiTimer.Tick += (_, _) => RefreshGameSnapshot();
            _uiTimer.Start();
            RefreshGameSnapshot();
            StatusText.Text = "Idle";
            UpdateButtonStates();
        }

        private PortableStationDefinition? SelectedStation => StationCombo.SelectedItem as PortableStationDefinition;
        private StationMode? SelectedMode => ModeCombo.SelectedItem as StationMode;
        private int SelectedPreset => PresetCombo.SelectedIndex + 1;

        private void StationCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedStation is null)
            {
                ModeCombo.ItemsSource = null;
                StationNotesText.Text = string.Empty;
                SkillText.Text = "—";
                return;
            }

            ModeCombo.ItemsSource = SelectedStation.Modes;
            ModeCombo.SelectedIndex = 0;
            StationNotesText.Text = SelectedStation.Description;
            SkillText.Text = SelectedStation.PrimarySkill?.ToString() ?? "—";
        }

        private PortableRunOptions? BuildOptions()
        {
            if (SelectedStation is null)
            {
                AppendLog("Select a portable station to continue.");
                return null;
            }

            if (SelectedMode is null)
            {
                AppendLog("Select a configuration for the station.");
                return null;
            }

            return new PortableRunOptions(SelectedStation, SelectedMode, SelectedPreset);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_runner.IsRunning)
            {
                AppendLog("Runner already active.");
                return;
            }

            var options = BuildOptions();
            if (options is null)
            {
                return;
            }

            if (_runner.Start(options))
            {
                _runStartedUtc = DateTime.UtcNow;
                _cycleCount = 0;
                CycleText.Text = "0";
                AppendLog($"Started {options.Station.Name} · {options.Mode.Name} using preset {options.BankPreset}.");
                UpdateButtonStates();
            }
            else
            {
                AppendLog("Runner failed to start.");
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await StopRunnerAsync();
        }

        private async Task StopRunnerAsync()
        {
            if (!_runner.IsRunning)
            {
                return;
            }

            await _runner.StopAsync();
            _runStartedUtc = null;
            StatusText.Text = "Idle";
            AppendLog("Runner stopped.");
            UpdateButtonStates();
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            var options = BuildOptions();
            if (options is null)
            {
                return;
            }

            AppendLog($"Testing single cycle for {options.Station.Name} / {options.Mode.Name}.");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try
            {
                await _runner.ExecuteTestCycleAsync(options, cts.Token);
            }
            catch (OperationCanceledException)
            {
                AppendLog("Test cycle timed out.");
            }
        }

        private void OnRunnerLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLog(message);
            });
        }

        private void OnRunnerStatusChanged(string status)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = status;
            });
        }

        private void OnSkillSnapshotAvailable(SkillSnapshot snapshot)
        {
            Dispatcher.Invoke(() =>
            {
                if (snapshot.SkillName == "—")
                {
                    SkillText.Text = SelectedStation?.PrimarySkill?.ToString() ?? "—";
                    return;
                }

                SkillText.Text = $"{snapshot.SkillName} lvl {snapshot.Level} (+{snapshot.LevelsGained}) · {snapshot.XpGained:N0} xp · {snapshot.XpPerHour:N0} xp/hr";
            });
        }

        private void OnCycleCountChanged(int count)
        {
            _cycleCount = count;
            Dispatcher.Invoke(() => CycleText.Text = count.ToString());
        }

        private void RefreshGameSnapshot()
        {
            RuntimeText.Text = _runStartedUtc.HasValue
                ? (DateTime.UtcNow - _runStartedUtc.Value).ToString("hh\\:mm\\:ss")
                : "00:00:00";

            if (!Game.IsInjected || !Game.HasClientPointers)
            {
                PlayerText.Text = "Waiting for MemoryError injection…";
                InventoryText.Text = "—";
                return;
            }

            var state = LocalPlayer.IsLoggedIn() ? "Logged In" : "Menu";
            var (x, y, z) = LocalPlayer.GetTilePosition();
            PlayerText.Text = $"{Game.LocalPlayerName} · {state} · Tile ({x}, {y}, {z})";
            InventoryText.Text = $"Free slots: {Inventory.FreeSlots}";
        }

        private void AppendLog(string message)
        {
            _logEntries.Add(message);
            while (_logEntries.Count > 250)
            {
                _logEntries.RemoveAt(0);
            }

            if (_logEntries.LastOrDefault() is { } last)
            {
                LogList.ScrollIntoView(last);
            }
        }

        private void UpdateButtonStates()
        {
            var running = _runner.IsRunning;
            StartButton.IsEnabled = !running;
            StopButton.IsEnabled = running;
            TestButton.IsEnabled = !running;
        }

        protected override async void OnClosed(EventArgs e)
        {
            _uiTimer.Stop();
            await StopRunnerAsync();
            _runner.Dispose();
            base.OnClosed(e);
        }
    }
}
