namespace MESharpExamples.WinForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelStateCaption = new System.Windows.Forms.Label();
            this.lblStateValue = new System.Windows.Forms.Label();
            this.labelPlayerCaption = new System.Windows.Forms.Label();
            this.lblPlayerValue = new System.Windows.Forms.Label();
            this.labelInjectionCaption = new System.Windows.Forms.Label();
            this.lblInjectionValue = new System.Windows.Forms.Label();
            this.btnRefreshInventory = new System.Windows.Forms.Button();
            this.btnSampleAction = new System.Windows.Forms.Button();
            this.inventoryListView = new System.Windows.Forms.ListView();
            this.columnSlot = new System.Windows.Forms.ColumnHeader();
            this.columnName = new System.Windows.Forms.ColumnHeader();
            this.columnAmount = new System.Windows.Forms.ColumnHeader();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(12, 9);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(238, 21);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "MESharp WinForms Dashboard";
            // 
            // labelStateCaption
            // 
            this.labelStateCaption.AutoSize = true;
            this.labelStateCaption.Location = new System.Drawing.Point(14, 46);
            this.labelStateCaption.Name = "labelStateCaption";
            this.labelStateCaption.Size = new System.Drawing.Size(68, 15);
            this.labelStateCaption.TabIndex = 1;
            this.labelStateCaption.Text = "Game State:";
            // 
            // lblStateValue
            // 
            this.lblStateValue.AutoSize = true;
            this.lblStateValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStateValue.Location = new System.Drawing.Point(120, 46);
            this.lblStateValue.Name = "lblStateValue";
            this.lblStateValue.Size = new System.Drawing.Size(15, 15);
            this.lblStateValue.TabIndex = 2;
            this.lblStateValue.Text = "—";
            // 
            // labelPlayerCaption
            // 
            this.labelPlayerCaption.AutoSize = true;
            this.labelPlayerCaption.Location = new System.Drawing.Point(14, 71);
            this.labelPlayerCaption.Name = "labelPlayerCaption";
            this.labelPlayerCaption.Size = new System.Drawing.Size(84, 15);
            this.labelPlayerCaption.TabIndex = 3;
            this.labelPlayerCaption.Text = "Player / Tile Pos:";
            // 
            // lblPlayerValue
            // 
            this.lblPlayerValue.AutoSize = true;
            this.lblPlayerValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPlayerValue.Location = new System.Drawing.Point(120, 71);
            this.lblPlayerValue.Name = "lblPlayerValue";
            this.lblPlayerValue.Size = new System.Drawing.Size(15, 15);
            this.lblPlayerValue.TabIndex = 4;
            this.lblPlayerValue.Text = "—";
            // 
            // labelInjectionCaption
            // 
            this.labelInjectionCaption.AutoSize = true;
            this.labelInjectionCaption.Location = new System.Drawing.Point(14, 96);
            this.labelInjectionCaption.Name = "labelInjectionCaption";
            this.labelInjectionCaption.Size = new System.Drawing.Size(88, 15);
            this.labelInjectionCaption.TabIndex = 5;
            this.labelInjectionCaption.Text = "Injection Flags:";
            // 
            // lblInjectionValue
            // 
            this.lblInjectionValue.AutoSize = true;
            this.lblInjectionValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInjectionValue.Location = new System.Drawing.Point(120, 96);
            this.lblInjectionValue.Name = "lblInjectionValue";
            this.lblInjectionValue.Size = new System.Drawing.Size(15, 15);
            this.lblInjectionValue.TabIndex = 6;
            this.lblInjectionValue.Text = "—";
            // 
            // btnRefreshInventory
            // 
            this.btnRefreshInventory.Location = new System.Drawing.Point(12, 124);
            this.btnRefreshInventory.Name = "btnRefreshInventory";
            this.btnRefreshInventory.Size = new System.Drawing.Size(156, 27);
            this.btnRefreshInventory.TabIndex = 7;
            this.btnRefreshInventory.Text = "Refresh Inventory";
            this.btnRefreshInventory.UseVisualStyleBackColor = true;
            this.btnRefreshInventory.Click += new System.EventHandler(this.BtnRefreshInventory_Click);
            // 
            // btnSampleAction
            // 
            this.btnSampleAction.Location = new System.Drawing.Point(174, 124);
            this.btnSampleAction.Name = "btnSampleAction";
            this.btnSampleAction.Size = new System.Drawing.Size(156, 27);
            this.btnSampleAction.TabIndex = 8;
            this.btnSampleAction.Text = "Describe Interaction";
            this.btnSampleAction.UseVisualStyleBackColor = true;
            this.btnSampleAction.Click += new System.EventHandler(this.BtnSampleAction_Click);
            // 
            // inventoryListView
            // 
            this.inventoryListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnSlot,
            this.columnName,
            this.columnAmount});
            this.inventoryListView.FullRowSelect = true;
            this.inventoryListView.HideSelection = false;
            this.inventoryListView.Location = new System.Drawing.Point(12, 157);
            this.inventoryListView.MultiSelect = false;
            this.inventoryListView.Name = "inventoryListView";
            this.inventoryListView.Size = new System.Drawing.Size(480, 271);
            this.inventoryListView.TabIndex = 9;
            this.inventoryListView.UseCompatibleStateImageBehavior = false;
            this.inventoryListView.View = System.Windows.Forms.View.Details;
            // 
            // columnSlot
            // 
            this.columnSlot.Text = "Slot";
            this.columnSlot.Width = 50;
            // 
            // columnName
            // 
            this.columnName.Text = "Item Name";
            this.columnName.Width = 260;
            // 
            // columnAmount
            // 
            this.columnAmount.Text = "Amount";
            this.columnAmount.Width = 150;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(498, 157);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(274, 271);
            this.txtLog.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 441);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.inventoryListView);
            this.Controls.Add(this.btnSampleAction);
            this.Controls.Add(this.btnRefreshInventory);
            this.Controls.Add(this.lblInjectionValue);
            this.Controls.Add(this.labelInjectionCaption);
            this.Controls.Add(this.lblPlayerValue);
            this.Controls.Add(this.labelPlayerCaption);
            this.Controls.Add(this.lblStateValue);
            this.Controls.Add(this.labelStateCaption);
            this.Controls.Add(this.labelTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MESharp WinForms Example";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelStateCaption;
        private System.Windows.Forms.Label lblStateValue;
        private System.Windows.Forms.Label labelPlayerCaption;
        private System.Windows.Forms.Label lblPlayerValue;
        private System.Windows.Forms.Label labelInjectionCaption;
        private System.Windows.Forms.Label lblInjectionValue;
        private System.Windows.Forms.Button btnRefreshInventory;
        private System.Windows.Forms.Button btnSampleAction;
        private System.Windows.Forms.ListView inventoryListView;
        private System.Windows.Forms.ColumnHeader columnSlot;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnAmount;
        private System.Windows.Forms.TextBox txtLog;
    }
}
