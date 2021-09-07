
namespace PersonalViewMigrationTool
{
    partial class PersonalViewMigrationToolControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnConnectTargetOrg = new System.Windows.Forms.Button();
            this.lblConnectedSourceOrg = new System.Windows.Forms.Label();
            this.lblConnectedTargetOrg = new System.Windows.Forms.Label();
            this.lblSourceOrgHint = new System.Windows.Forms.Label();
            this.btnLoadUsers = new System.Windows.Forms.Button();
            this.lbDebugOutput = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLoadPersonalViews = new System.Windows.Forms.Button();
            this.lblUsersLoadStatus = new System.Windows.Forms.Label();
            this.lblPersonalViewsLoadedStatus = new System.Windows.Forms.Label();
            this.btnLoadSharing = new System.Windows.Forms.Button();
            this.lblSharingRetrievedStatus = new System.Windows.Forms.Label();
            this.btnStartMigration = new System.Windows.Forms.Button();
            this.toolStripMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1135, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(86, 22);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnConnectTargetOrg
            // 
            this.btnConnectTargetOrg.Location = new System.Drawing.Point(23, 54);
            this.btnConnectTargetOrg.Name = "btnConnectTargetOrg";
            this.btnConnectTargetOrg.Size = new System.Drawing.Size(121, 23);
            this.btnConnectTargetOrg.TabIndex = 5;
            this.btnConnectTargetOrg.Text = "Connect Target";
            this.btnConnectTargetOrg.UseVisualStyleBackColor = true;
            this.btnConnectTargetOrg.Click += new System.EventHandler(this.btnConnectTargetOrg_Click);
            // 
            // lblConnectedSourceOrg
            // 
            this.lblConnectedSourceOrg.AutoSize = true;
            this.lblConnectedSourceOrg.Location = new System.Drawing.Point(153, 20);
            this.lblConnectedSourceOrg.Name = "lblConnectedSourceOrg";
            this.lblConnectedSourceOrg.Size = new System.Drawing.Size(113, 13);
            this.lblConnectedSourceOrg.TabIndex = 7;
            this.lblConnectedSourceOrg.Text = "Source not connected";
            // 
            // lblConnectedTargetOrg
            // 
            this.lblConnectedTargetOrg.AutoSize = true;
            this.lblConnectedTargetOrg.Location = new System.Drawing.Point(153, 59);
            this.lblConnectedTargetOrg.Name = "lblConnectedTargetOrg";
            this.lblConnectedTargetOrg.Size = new System.Drawing.Size(110, 13);
            this.lblConnectedTargetOrg.TabIndex = 8;
            this.lblConnectedTargetOrg.Text = "Target not connected";
            // 
            // lblSourceOrgHint
            // 
            this.lblSourceOrgHint.AutoSize = true;
            this.lblSourceOrgHint.Location = new System.Drawing.Point(30, 20);
            this.lblSourceOrgHint.Name = "lblSourceOrgHint";
            this.lblSourceOrgHint.Size = new System.Drawing.Size(44, 13);
            this.lblSourceOrgHint.TabIndex = 9;
            this.lblSourceOrgHint.Text = "Source:";
            // 
            // btnLoadUsers
            // 
            this.btnLoadUsers.Enabled = false;
            this.btnLoadUsers.Location = new System.Drawing.Point(23, 100);
            this.btnLoadUsers.Name = "btnLoadUsers";
            this.btnLoadUsers.Size = new System.Drawing.Size(127, 23);
            this.btnLoadUsers.TabIndex = 10;
            this.btnLoadUsers.Text = "Load Users";
            this.btnLoadUsers.UseVisualStyleBackColor = true;
            this.btnLoadUsers.Click += new System.EventHandler(this.btnLoadUsers_Click);
            // 
            // lbDebugOutput
            // 
            this.lbDebugOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbDebugOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbDebugOutput.CausesValidation = false;
            this.lbDebugOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbDebugOutput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDebugOutput.ForeColor = System.Drawing.Color.White;
            this.lbDebugOutput.FormattingEnabled = true;
            this.lbDebugOutput.HorizontalScrollbar = true;
            this.lbDebugOutput.Items.AddRange(new object[] {
            "Debug Output"});
            this.lbDebugOutput.Location = new System.Drawing.Point(383, 6);
            this.lbDebugOutput.MinimumSize = new System.Drawing.Size(500, 2);
            this.lbDebugOutput.Name = "lbDebugOutput";
            this.tableLayoutPanel1.SetRowSpan(this.lbDebugOutput, 2);
            this.lbDebugOutput.ScrollAlwaysVisible = true;
            this.lbDebugOutput.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbDebugOutput.Size = new System.Drawing.Size(746, 741);
            this.lbDebugOutput.TabIndex = 11;
            this.lbDebugOutput.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.39238F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66.60762F));
            this.tableLayoutPanel1.Controls.Add(this.lbDebugOutput, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1135, 753);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnStartMigration);
            this.panel1.Controls.Add(this.lblSharingRetrievedStatus);
            this.panel1.Controls.Add(this.btnLoadSharing);
            this.panel1.Controls.Add(this.lblPersonalViewsLoadedStatus);
            this.panel1.Controls.Add(this.lblUsersLoadStatus);
            this.panel1.Controls.Add(this.btnLoadPersonalViews);
            this.panel1.Controls.Add(this.lblConnectedSourceOrg);
            this.panel1.Controls.Add(this.btnLoadUsers);
            this.panel1.Controls.Add(this.lblSourceOrgHint);
            this.panel1.Controls.Add(this.lblConnectedTargetOrg);
            this.panel1.Controls.Add(this.btnConnectTargetOrg);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(371, 367);
            this.panel1.TabIndex = 12;
            // 
            // btnLoadPersonalViews
            // 
            this.btnLoadPersonalViews.Enabled = false;
            this.btnLoadPersonalViews.Location = new System.Drawing.Point(23, 146);
            this.btnLoadPersonalViews.Name = "btnLoadPersonalViews";
            this.btnLoadPersonalViews.Size = new System.Drawing.Size(127, 23);
            this.btnLoadPersonalViews.TabIndex = 11;
            this.btnLoadPersonalViews.Text = "Load Personal Views";
            this.btnLoadPersonalViews.UseVisualStyleBackColor = true;
            this.btnLoadPersonalViews.Click += new System.EventHandler(this.btnLoadPersonalViews_Click);
            // 
            // lblUsersLoadStatus
            // 
            this.lblUsersLoadStatus.AutoSize = true;
            this.lblUsersLoadStatus.Location = new System.Drawing.Point(153, 105);
            this.lblUsersLoadStatus.Name = "lblUsersLoadStatus";
            this.lblUsersLoadStatus.Size = new System.Drawing.Size(87, 13);
            this.lblUsersLoadStatus.TabIndex = 12;
            this.lblUsersLoadStatus.Text = "Users not loaded";
            // 
            // lblPersonalViewsLoadedStatus
            // 
            this.lblPersonalViewsLoadedStatus.AutoSize = true;
            this.lblPersonalViewsLoadedStatus.Location = new System.Drawing.Point(153, 151);
            this.lblPersonalViewsLoadedStatus.Name = "lblPersonalViewsLoadedStatus";
            this.lblPersonalViewsLoadedStatus.Size = new System.Drawing.Size(132, 13);
            this.lblPersonalViewsLoadedStatus.TabIndex = 13;
            this.lblPersonalViewsLoadedStatus.Text = "Personal Views not loaded";
            // 
            // btnLoadSharing
            // 
            this.btnLoadSharing.Enabled = false;
            this.btnLoadSharing.Location = new System.Drawing.Point(23, 192);
            this.btnLoadSharing.Name = "btnLoadSharing";
            this.btnLoadSharing.Size = new System.Drawing.Size(127, 23);
            this.btnLoadSharing.TabIndex = 14;
            this.btnLoadSharing.Text = "Load Sharing";
            this.btnLoadSharing.UseVisualStyleBackColor = true;
            this.btnLoadSharing.Click += new System.EventHandler(this.btnLoadSharing_Click);
            // 
            // lblSharingRetrievedStatus
            // 
            this.lblSharingRetrievedStatus.AutoSize = true;
            this.lblSharingRetrievedStatus.Location = new System.Drawing.Point(153, 197);
            this.lblSharingRetrievedStatus.Name = "lblSharingRetrievedStatus";
            this.lblSharingRetrievedStatus.Size = new System.Drawing.Size(152, 13);
            this.lblSharingRetrievedStatus.TabIndex = 15;
            this.lblSharingRetrievedStatus.Text = "Shared Permissions not loaded";
            // 
            // btnStartMigration
            // 
            this.btnStartMigration.Enabled = false;
            this.btnStartMigration.Location = new System.Drawing.Point(23, 239);
            this.btnStartMigration.Name = "btnStartMigration";
            this.btnStartMigration.Size = new System.Drawing.Size(127, 23);
            this.btnStartMigration.TabIndex = 16;
            this.btnStartMigration.Text = "Migrate";
            this.btnStartMigration.UseVisualStyleBackColor = true;
            this.btnStartMigration.Click += new System.EventHandler(this.btnStartMigration_Click);
            // 
            // PersonalViewMigrationToolControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "PersonalViewMigrationToolControl";
            this.Size = new System.Drawing.Size(1135, 778);
            this.Load += new System.EventHandler(this.PersonalViewMigrationToolControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.Button btnConnectTargetOrg;
        private System.Windows.Forms.Label lblConnectedSourceOrg;
        private System.Windows.Forms.Label lblConnectedTargetOrg;
        private System.Windows.Forms.Label lblSourceOrgHint;
        private System.Windows.Forms.Button btnLoadUsers;
        private System.Windows.Forms.ListBox lbDebugOutput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblPersonalViewsLoadedStatus;
        private System.Windows.Forms.Label lblUsersLoadStatus;
        private System.Windows.Forms.Button btnLoadPersonalViews;
        private System.Windows.Forms.Label lblSharingRetrievedStatus;
        private System.Windows.Forms.Button btnLoadSharing;
        private System.Windows.Forms.Button btnStartMigration;
    }
}
