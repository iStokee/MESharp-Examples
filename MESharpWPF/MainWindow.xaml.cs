using MESharp.API;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace MESharpExamples.WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromSeconds(1) };
        private string _gameStateText = "Waiting for MemoryError...";
        private string _playerTileText = "—";
        private string _injectionFlagsText = "—";

        public ObservableCollection<InventoryItemViewModel> InventoryItems { get; } = new();

        public string GameStateText
        {
            get => _gameStateText;
            set { _gameStateText = value; OnPropertyChanged(); }
        }

        public string PlayerTileText
        {
            get => _playerTileText;
            set { _playerTileText = value; OnPropertyChanged(); }
        }

        public string InjectionFlagsText
        {
            get => _injectionFlagsText;
            set { _injectionFlagsText = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _statusTimer.Tick += OnStatusTick;
            _statusTimer.Start();
            RefreshStatus();
        }

        private void OnStatusTick(object? sender, EventArgs e) => RefreshStatus();


        private void RefreshStatus()
        {
            if (!Game.IsInjected || !Game.HasClientPointers)
            {
                GameStateText = "Waiting for MemoryError injection";
                PlayerTileText = "—";
                InjectionFlagsText = Game.IsInjected ? "Pointers not ready" : "Not injected";
                return;
            }

            GameStateText = $"{Game.State} ({(LocalPlayer.IsLoggedIn() ? "Logged In" : "Menu")})";
            var (x, y, z) = LocalPlayer.GetTilePosition();
            PlayerTileText = $"{Game.LocalPlayerName} @ {x}, {y}, {z}";
            InjectionFlagsText = Game.InjectionFlags.ToString();
        }

        private void RefreshInventory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = Inventory.GetAll();
                InventoryItems.Clear();
                foreach (var item in items)
                {
                    InventoryItems.Add(new InventoryItemViewModel(item.Slot, item.Name, item.Amount));
                }
                AppendLog($"Inventory refreshed ({items.Count} items, {Inventory.FreeSlots} free slots).");
            }
            catch (Exception ex)
            {
                AppendLog($"Inventory refresh failed: {ex.Message}");
            }
        }

        private void DescribeInteraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Game.HasClientPointers)
                {
                    AppendLog("Client is not ready yet.");
                    return;
                }

                if (!LocalPlayer.IsLoggedIn())
                {
                    AppendLog("Player is not logged in.");
                    return;
                }

                var interactingName = LocalPlayer.GetInteractingWith();
                var interactionId = LocalPlayer.GetInteractingWithId();

                if (string.IsNullOrWhiteSpace(interactingName))
                {
                    AppendLog("You are not currently interacting with an NPC or object.");
                }
                else
                {
                    AppendLog($"Interacting with '{interactingName}' (id {interactionId}).");
                }

                var totalCoins = Inventory.FindById(995).Aggregate(0UL, (acc, item) => acc + item.Amount);
                AppendLog($"Detected approximately {totalCoins:N0} coins in inventory.");
            }
            catch (Exception ex)
            {
                AppendLog($"Describe interaction failed: {ex.Message}");
            }
        }

        private void AppendLog(string message)
        {
            LogTextBox.AppendText($"[{DateTime.Now:T}] {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        }

        protected override void OnClosed(EventArgs e)
        {
            _statusTimer.Stop();
            _statusTimer.Tick -= OnStatusTick;
            base.OnClosed(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public record InventoryItemViewModel(int Slot, string Name, ulong Amount);
}
