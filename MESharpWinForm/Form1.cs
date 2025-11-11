using System;
using System.Linq;
using System.Windows.Forms;
using MESharp.API;
using Timer = System.Windows.Forms.Timer;

namespace MESharpExamples.WinForms
{
    public partial class Form1 : Form
    {
        private readonly Timer _statusTimer = new() { Interval = 1000 };

        public Form1()
        {
            InitializeComponent();
            _statusTimer.Tick += (_, _) => RefreshStatus();
            _statusTimer.Start();
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (!Game.IsInjected || !Game.HasClientPointers)
            {
                lblStateValue.Text = "Waiting for MemoryError...";
                lblPlayerValue.Text = "—";
                lblInjectionValue.Text = Game.IsInjected ? "Pointers not ready" : "Not injected";
                return;
            }

            lblStateValue.Text = $"{Game.State} ({(LocalPlayer.IsLoggedIn() ? "Logged In" : "Menu")})";

            var (x, y, z) = LocalPlayer.GetTilePosition();
            lblPlayerValue.Text = $"{Game.LocalPlayerName} @ {x}, {y}, {z}";
            lblInjectionValue.Text = Game.InjectionFlags.ToString();
        }

        private void BtnRefreshInventory_Click(object? sender, EventArgs e)
        {
            try
            {
                var items = Inventory.GetAll();
                inventoryListView.BeginUpdate();
                inventoryListView.Items.Clear();
                foreach (var item in items)
                {
                    var row = new ListViewItem(item.Slot.ToString());
                    row.SubItems.Add(item.Name);
                    row.SubItems.Add(item.Amount.ToString());
                    inventoryListView.Items.Add(row);
                }
                inventoryListView.EndUpdate();
                AppendLog($"Inventory refreshed ({items.Count} items, {Inventory.FreeSlots} free slots).");
            }
            catch (Exception ex)
            {
                AppendLog($"Inventory refresh failed: {ex.Message}");
            }
        }

        private void BtnSampleAction_Click(object? sender, EventArgs e)
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
                AppendLog($"Sample action failed: {ex.Message}");
            }
        }

        private void AppendLog(string message)
        {
            var line = $"[{DateTime.Now:T}] {message}{Environment.NewLine}";
            txtLog.AppendText(line);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _statusTimer.Stop();
            _statusTimer.Dispose();
            base.OnFormClosing(e);
        }
    }
}
