
using System.Reflection;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PersonalViewMigrationToolControl));
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Users");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Teams");
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbHelp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbFeedback = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.dropDownButtonLogs = new System.Windows.Forms.ToolStripDropDownButton();
            this.openLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLogFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnConnectTargetOrg = new System.Windows.Forms.Button();
            this.btnLoadUsers = new System.Windows.Forms.Button();
            this.lbDebugOutput = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnConnectSource = new System.Windows.Forms.Button();
            this.tbMigrationResult = new System.Windows.Forms.TextBox();
            this.tbSharingRetrievedStatus = new System.Windows.Forms.TextBox();
            this.tbPersonalViewsLoadedStatus = new System.Windows.Forms.TextBox();
            this.tbUsersLoadStatus = new System.Windows.Forms.TextBox();
            this.tbConnectedTargetOrg = new System.Windows.Forms.TextBox();
            this.tbConnectedSourceOrg = new System.Windows.Forms.TextBox();
            this.btnStartMigration = new System.Windows.Forms.Button();
            this.btnLoadSharing = new System.Windows.Forms.Button();
            this.btnLoadPersonalViews = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblWarning = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.cmsOverviewTab = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.toolStripMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.cmsOverviewTab.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbHelp,
            this.toolStripSeparator1,
            this.tsbFeedback,
            this.toolStripSeparator2,
            this.dropDownButtonLogs});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1317, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(40, 22);
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbHelp
            // 
            this.tsbHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbHelp.Image = ((System.Drawing.Image)(resources.GetObject("tsbHelp.Image")));
            this.tsbHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbHelp.Name = "tsbHelp";
            this.tsbHelp.Size = new System.Drawing.Size(36, 22);
            this.tsbHelp.Text = "Help";
            this.tsbHelp.Click += new System.EventHandler(this.tsbHelp_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbFeedback
            // 
            this.tsbFeedback.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbFeedback.Image = ((System.Drawing.Image)(resources.GetObject("tsbFeedback.Image")));
            this.tsbFeedback.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFeedback.Name = "tsbFeedback";
            this.tsbFeedback.Size = new System.Drawing.Size(79, 22);
            this.tsbFeedback.Text = "Report a Bug";
            this.tsbFeedback.Click += new System.EventHandler(this.tsbFeedback_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // dropDownButtonLogs
            // 
            this.dropDownButtonLogs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.dropDownButtonLogs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openLogToolStripMenuItem,
            this.openLogFolderToolStripMenuItem});
            this.dropDownButtonLogs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.dropDownButtonLogs.Name = "dropDownButtonLogs";
            this.dropDownButtonLogs.Size = new System.Drawing.Size(45, 22);
            this.dropDownButtonLogs.Text = "Logs";
            // 
            // openLogToolStripMenuItem
            // 
            this.openLogToolStripMenuItem.Name = "openLogToolStripMenuItem";
            this.openLogToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.openLogToolStripMenuItem.Text = "Open Log";
            this.openLogToolStripMenuItem.Click += new System.EventHandler(this.openLogToolStripMenuItem_Click);
            // 
            // openLogFolderToolStripMenuItem
            // 
            this.openLogFolderToolStripMenuItem.Name = "openLogFolderToolStripMenuItem";
            this.openLogFolderToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.openLogFolderToolStripMenuItem.Text = "Open Log Folder";
            this.openLogFolderToolStripMenuItem.Click += new System.EventHandler(this.openLogFolderToolStripMenuItem_Click);
            // 
            // btnConnectTargetOrg
            // 
            this.btnConnectTargetOrg.Location = new System.Drawing.Point(23, 60);
            this.btnConnectTargetOrg.Name = "btnConnectTargetOrg";
            this.btnConnectTargetOrg.Size = new System.Drawing.Size(121, 23);
            this.btnConnectTargetOrg.TabIndex = 5;
            this.btnConnectTargetOrg.Text = "Connect Target";
            this.btnConnectTargetOrg.UseVisualStyleBackColor = true;
            this.btnConnectTargetOrg.Click += new System.EventHandler(this.btnConnectTargetOrg_Click);
            // 
            // btnLoadUsers
            // 
            this.btnLoadUsers.Enabled = false;
            this.btnLoadUsers.Location = new System.Drawing.Point(23, 103);
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
            this.lbDebugOutput.Location = new System.Drawing.Point(3, 3);
            this.lbDebugOutput.MinimumSize = new System.Drawing.Size(500, 2);
            this.lbDebugOutput.Name = "lbDebugOutput";
            this.lbDebugOutput.ScrollAlwaysVisible = true;
            this.lbDebugOutput.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbDebugOutput.Size = new System.Drawing.Size(941, 808);
            this.lbDebugOutput.TabIndex = 11;
            this.lbDebugOutput.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1317, 858);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnConnectSource);
            this.panel1.Controls.Add(this.tbMigrationResult);
            this.panel1.Controls.Add(this.tbSharingRetrievedStatus);
            this.panel1.Controls.Add(this.tbPersonalViewsLoadedStatus);
            this.panel1.Controls.Add(this.tbUsersLoadStatus);
            this.panel1.Controls.Add(this.tbConnectedTargetOrg);
            this.panel1.Controls.Add(this.tbConnectedSourceOrg);
            this.panel1.Controls.Add(this.btnStartMigration);
            this.panel1.Controls.Add(this.btnLoadSharing);
            this.panel1.Controls.Add(this.btnLoadPersonalViews);
            this.panel1.Controls.Add(this.btnLoadUsers);
            this.panel1.Controls.Add(this.btnConnectTargetOrg);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(344, 420);
            this.panel1.TabIndex = 12;
            // 
            // btnConnectSource
            // 
            this.btnConnectSource.Location = new System.Drawing.Point(23, 17);
            this.btnConnectSource.Name = "btnConnectSource";
            this.btnConnectSource.Size = new System.Drawing.Size(121, 23);
            this.btnConnectSource.TabIndex = 23;
            this.btnConnectSource.Text = "Connect Source";
            this.btnConnectSource.UseVisualStyleBackColor = true;
            this.btnConnectSource.Click += new System.EventHandler(this.btnConnectSource_Click);
            // 
            // tbMigrationResult
            // 
            this.tbMigrationResult.Enabled = false;
            this.tbMigrationResult.Location = new System.Drawing.Point(3, 334);
            this.tbMigrationResult.Name = "tbMigrationResult";
            this.tbMigrationResult.Size = new System.Drawing.Size(338, 20);
            this.tbMigrationResult.TabIndex = 22;
            this.tbMigrationResult.Text = "Migration not completed";
            // 
            // tbSharingRetrievedStatus
            // 
            this.tbSharingRetrievedStatus.Enabled = false;
            this.tbSharingRetrievedStatus.Location = new System.Drawing.Point(156, 192);
            this.tbSharingRetrievedStatus.Name = "tbSharingRetrievedStatus";
            this.tbSharingRetrievedStatus.Size = new System.Drawing.Size(166, 20);
            this.tbSharingRetrievedStatus.TabIndex = 21;
            this.tbSharingRetrievedStatus.Text = "Shared Permissions not loaded";
            // 
            // tbPersonalViewsLoadedStatus
            // 
            this.tbPersonalViewsLoadedStatus.Enabled = false;
            this.tbPersonalViewsLoadedStatus.Location = new System.Drawing.Point(156, 149);
            this.tbPersonalViewsLoadedStatus.Name = "tbPersonalViewsLoadedStatus";
            this.tbPersonalViewsLoadedStatus.Size = new System.Drawing.Size(166, 20);
            this.tbPersonalViewsLoadedStatus.TabIndex = 20;
            this.tbPersonalViewsLoadedStatus.Text = "Personal Views not loaded";
            // 
            // tbUsersLoadStatus
            // 
            this.tbUsersLoadStatus.Enabled = false;
            this.tbUsersLoadStatus.Location = new System.Drawing.Point(156, 106);
            this.tbUsersLoadStatus.Name = "tbUsersLoadStatus";
            this.tbUsersLoadStatus.Size = new System.Drawing.Size(166, 20);
            this.tbUsersLoadStatus.TabIndex = 19;
            this.tbUsersLoadStatus.Text = "Users not loaded";
            // 
            // tbConnectedTargetOrg
            // 
            this.tbConnectedTargetOrg.Enabled = false;
            this.tbConnectedTargetOrg.Location = new System.Drawing.Point(156, 63);
            this.tbConnectedTargetOrg.Name = "tbConnectedTargetOrg";
            this.tbConnectedTargetOrg.Size = new System.Drawing.Size(166, 20);
            this.tbConnectedTargetOrg.TabIndex = 18;
            this.tbConnectedTargetOrg.Text = "Target not connected";
            // 
            // tbConnectedSourceOrg
            // 
            this.tbConnectedSourceOrg.Enabled = false;
            this.tbConnectedSourceOrg.Location = new System.Drawing.Point(156, 20);
            this.tbConnectedSourceOrg.Name = "tbConnectedSourceOrg";
            this.tbConnectedSourceOrg.Size = new System.Drawing.Size(166, 20);
            this.tbConnectedSourceOrg.TabIndex = 17;
            this.tbConnectedSourceOrg.Text = "Source not connected";
            // 
            // btnStartMigration
            // 
            this.btnStartMigration.Enabled = false;
            this.btnStartMigration.Location = new System.Drawing.Point(90, 240);
            this.btnStartMigration.Name = "btnStartMigration";
            this.btnStartMigration.Size = new System.Drawing.Size(127, 23);
            this.btnStartMigration.TabIndex = 16;
            this.btnStartMigration.Text = "Migrate";
            this.btnStartMigration.UseVisualStyleBackColor = true;
            this.btnStartMigration.Click += new System.EventHandler(this.btnStartMigration_Click);
            // 
            // btnLoadSharing
            // 
            this.btnLoadSharing.Enabled = false;
            this.btnLoadSharing.Location = new System.Drawing.Point(23, 189);
            this.btnLoadSharing.Name = "btnLoadSharing";
            this.btnLoadSharing.Size = new System.Drawing.Size(127, 23);
            this.btnLoadSharing.TabIndex = 14;
            this.btnLoadSharing.Text = "Load Sharing";
            this.btnLoadSharing.UseVisualStyleBackColor = true;
            this.btnLoadSharing.Click += new System.EventHandler(this.btnLoadSharing_Click);
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
            // panel2
            // 
            this.panel2.Controls.Add(this.lblWarning);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(6, 432);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(344, 420);
            this.panel2.TabIndex = 13;
            // 
            // lblWarning
            // 
            this.lblWarning.AutoSize = true;
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(3, 112);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(334, 91);
            this.lblWarning.TabIndex = 0;
            this.lblWarning.Text = resources.GetString("lblWarning.Text");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(356, 6);
            this.tabControl1.Name = "tabControl1";
            this.tableLayoutPanel1.SetRowSpan(this.tabControl1, 2);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(955, 846);
            this.tabControl1.TabIndex = 14;
            this.tabControl1.TabStop = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.treeView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(947, 820);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Overview";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.CheckBoxes = true;
            this.treeView1.ContextMenuStrip = this.cmsOverviewTab;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(3, 3);
            this.treeView1.Name = "treeView1";
            treeNode1.Checked = true;
            treeNode1.Name = "nUsers";
            treeNode1.Text = "Users";
            treeNode2.Checked = true;
            treeNode2.Name = "nTeams";
            treeNode2.Text = "Teams";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.Size = new System.Drawing.Size(941, 814);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            // 
            // cmsOverviewTab
            // 
            this.cmsOverviewTab.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsOverviewTab.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem});
            this.cmsOverviewTab.Name = "cmsOverviewTab";
            this.cmsOverviewTab.Size = new System.Drawing.Size(137, 48);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.expandAllToolStripMenuItem.Text = "Expand All";
            this.expandAllToolStripMenuItem.Click += new System.EventHandler(this.expandAllToolStripMenuItem_Click);
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            this.collapseAllToolStripMenuItem.Click += new System.EventHandler(this.collapseAllToolStripMenuItem_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lbDebugOutput);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(947, 814);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Log Output";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // PersonalViewMigrationToolControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "PersonalViewMigrationToolControl";
            this.Size = new System.Drawing.Size(1317, 883);
            this.Load += new System.EventHandler(this.PersonalViewMigrationToolControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.cmsOverviewTab.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.Button btnConnectTargetOrg;
        private System.Windows.Forms.Button btnLoadUsers;
        private System.Windows.Forms.ListBox lbDebugOutput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnLoadPersonalViews;
        private System.Windows.Forms.Button btnLoadSharing;
        private System.Windows.Forms.Button btnStartMigration;
        private System.Windows.Forms.ToolStripDropDownButton dropDownButtonLogs;
        private System.Windows.Forms.ToolStripMenuItem openLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLogFolderToolStripMenuItem;
        private System.Windows.Forms.TextBox tbPersonalViewsLoadedStatus;
        private System.Windows.Forms.TextBox tbUsersLoadStatus;
        private System.Windows.Forms.TextBox tbConnectedTargetOrg;
        private System.Windows.Forms.TextBox tbConnectedSourceOrg;
        private System.Windows.Forms.TextBox tbMigrationResult;
        private System.Windows.Forms.TextBox tbSharingRetrievedStatus;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Button btnConnectSource;
        private System.Windows.Forms.ToolStripButton tsbHelp;
        private System.Windows.Forms.ToolStripButton tsbFeedback;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ContextMenuStrip cmsOverviewTab;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
    }
}
