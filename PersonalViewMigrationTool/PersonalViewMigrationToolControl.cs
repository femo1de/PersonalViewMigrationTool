using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PersonalViewMigrationTool.Dto;
using PersonalViewMigrationTool.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace PersonalViewMigrationTool
{
    public partial class PersonalViewMigrationToolControl : MultipleConnectionsPluginControlBase, IGitHubPlugin, IAboutPlugin, IHelpPlugin
    {
        #region Fields

        private Settings mySettings;
        private List<Entity> sourceUserAndTeamRecords = new List<Entity>();
        private List<Entity> targetUserAndTeamRecords = new List<Entity>();
        private ConnectionDetail sourceConnection;
        private ConnectionDetail targetConnection;
        private ObservableCollection<MigrationObject> migrationObjects = new ObservableCollection<MigrationObject>();
        private delegate void _updateTreeNodeDelegate(NodeUpdateObject nodeUpdateObject);
        private delegate void _updateLogWindowDelegate(string msg);

        // TODO: Limit columns
        const string fetch_PersonalViewsOwnedByUserOrTeam = @"
            <fetch>
              <entity name='userquery'>
                <filter>
                  <condition attribute='ownerid' operator='eq' value='{0}'/>
                </filter>
              </entity>
            </fetch>";

        #endregion

        #region ctor
        public PersonalViewMigrationToolControl()
        {
            InitializeComponent();
            LogInfo("Plugin initialized.");
            CustomLog("Executing Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());

            // register eventhandler to handle UI refreshes
            migrationObjects.CollectionChanged += MigrationObjects_CollectionChanged;
        }
        #endregion

        #region Event Handlers

        private void MigrationObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (treeView1.InvokeRequired)
            {
                treeView1.Invoke(new Action(() => MigrationObjects_CollectionChanged(sender, e)));
            }
            else
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        treeView1.Nodes.Clear();
                        // always add the nUsers and nTeams node
                        treeView1.Nodes.Add(new TreeNode("Users") { Name = "nUsers" });
                        treeView1.Nodes.Add(new TreeNode("Teams") { Name = "nTeams" });
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region IHelpPluginImplementation

        public string HelpUrl => "https://github.com/femo1de/PersonalViewMigrationTool/wiki";

        #endregion

        #region GitHub implementation

        public string RepositoryName => "PersonalViewMigrationTool";

        public string UserName => "femo1de";

        #endregion

        #region IAboutPlugin implementation
        public void ShowAboutDialog()
        {
            MessageBox.Show("This is an early alpha. Expect Bugs and always test on a non production system first. Inspect the Code on GitHub if you are not sure");
        }

        #endregion

        #region Control Events

        private void PersonalViewMigrationToolControl_Load(object sender, EventArgs e)
        {
            ShowWarningNotification("This Plugin is pre-release, always test on a non-production system first!", null);

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PersonalViewMigrationToolControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void btnLoadUsers_Click(object sender, EventArgs e)
        {
            HideNotification();

            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Loading Users and Teams",
                Work = RetrieveUsers,
                PostWorkCallBack = OnUsersRetrieved
            });
        }

        private void btnConnectTargetOrg_Click(object sender, EventArgs e)
        {
            // remove the currently connected target if it was connected before - we allow only one target connection
            if (targetConnection != null)
            {
                RemoveAdditionalOrganization(targetConnection);
            }
            AddAdditionalOrganization();
        }
   
        private void btnConnectSource_Click(object sender, EventArgs e)
        {
            RaiseRequestConnectionEvent(new RequestConnectionEventArgs()
            {
                ActionName = string.Empty,
                Control = this
            });
        }

        private void btnLoadPersonalViews_Click(object sender, EventArgs e)
        {
            HideNotification();

            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Loading Personal Views",
                Work = RetrievePersonalViews,
                PostWorkCallBack = OnPersonalViewsRetrieved
            });
        }

        private void btnLoadSharing_Click(object sender, EventArgs e)
        {
            HideNotification();

            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Loading Shared Permissions",
                Work = RetrieveSharings,
                PostWorkCallBack = OnSharingRetrieved
            });
        }

        private void btnStartMigration_Click(object sender, EventArgs e)
        {
            HideNotification();

            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Migrating",
                Work = Migrate,
                PostWorkCallBack = OnMigrateCompleted
            });
        }

        private void openLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLogFile();
        }

        private void openLogFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(LogFilePath));
        }

        private void tsbHelp_Click(object sender, EventArgs e)
        {
            Process.Start(HelpUrl);
        }

        private void tsbFeedback_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/femo1de/PersonalViewMigrationTool/issues/new");
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (actionName != "AdditionalOrganization")
            {
                // this is the source connection
                sourceConnection = detail;

                tbConnectedSourceOrg.Text = detail.ConnectionName;
                CustomLog($"Connected Source to: {detail.ConnectionName}");

                var whoAmIResponse = detail.GetCrmServiceClient().Execute(new WhoAmIRequest()) as WhoAmIResponse;
                CustomLog($"BusinessUnitId: {whoAmIResponse.BusinessUnitId.ToString("b")}, OrganizationId: {whoAmIResponse.OrganizationId.ToString("b")},  UserId: {whoAmIResponse.UserId.ToString("b")}");

                if (mySettings != null && detail != null)
                {
                    mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                    LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
                }

                // check whether this user can impersonate other users
                if (!detail.CanImpersonate)
                {
                    MessageBox.Show("Source user does not have the impersonate privileges! You will only be able to migrate this user's own personal views","Unable to impersonate other users", MessageBoxButtons.OK, MessageBoxIcon.Hand );
                    CustomLog("Source user does not have the impersonate privileges! You will only be able to migrate this user's own personal views");
                }
                else
                {
                    CustomLog($"Source User impersonation check successfull.");
                }
            }
        }

        /// <summary>
        /// This event occurs when the target connection has been updated in XrmToolBox
        /// </summary>
        protected override void ConnectionDetailsUpdated(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                targetConnection = AdditionalConnectionDetails[0];
                tbConnectedTargetOrg.Text = targetConnection.ConnectionName;

                LogInfo("Connection has changed to: {0}", targetConnection.WebApplicationUrl);

                CustomLog($"Connected Target to: {targetConnection.ConnectionName}");

                var whoAmIResponse = targetConnection.GetCrmServiceClient().Execute(new WhoAmIRequest()) as WhoAmIResponse;
                CustomLog($"BusinessUnitId: {whoAmIResponse.BusinessUnitId.ToString("b")}, OrganizationId: {whoAmIResponse.OrganizationId.ToString("b")},  UserId: {whoAmIResponse.UserId.ToString("b")}");
                btnLoadUsers.Enabled = true;

                // check whether this user can impersonate other users
                if (!targetConnection.CanImpersonate)
                {
                    MessageBox.Show("Target user does not have the impersonate privileges! You will only be able to migrate this user's own personal views", 
                        "Unable to impersonate other users", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CustomLog("Target user does not have the impersonate privileges! You will only be able to migrate this user's own personal views");
                }
                else
                {
                    CustomLog($"Target User impersonation check successfull.");
                }
            }
        }
        private void RetrieveUsers(BackgroundWorker worked, DoWorkEventArgs args)
        {
            if (Service == null)
            {
                CustomLog("Source connection has not been set. Please connect a source environment.");
                MessageBox.Show("Source connection has not been set. Please connect a source environment.", "Please connect source system", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                args.Cancel = true;
                return;
            }

            var sourceUsers = RetrieveUsersAndTeams(Service);
            var destinationUsers = RetrieveUsersAndTeams(AdditionalConnectionDetails[0].GetCrmServiceClient());

            sourceUserAndTeamRecords = sourceUsers;
            targetUserAndTeamRecords = destinationUsers;

            CustomLog("Mapping users / teams from source to target...");

            // source might still be impersonating someone. Force CallerId to be empty
            sourceConnection.RemoveImpersonation();
            migrationObjects.Clear();

            foreach (var sourceUserOrTeam in sourceUserAndTeamRecords)
            {
                if (sourceUserOrTeam.LogicalName == "systemuser")
                {
                    var mo = new MigrationObject(UpdateNode, sourceUserOrTeam.LogicalName, sourceUserOrTeam.Id, sourceUserOrTeam.Attributes["fullname"].ToString());

                    // skip this user if it is disabled
                    if ((bool)sourceUserOrTeam.Attributes["isdisabled"])
                    {
                        mo.WillBeMigrated = false;
                        mo.NotMigrateReason = "This user is disabled on the source system.";
                        migrationObjects.Add(mo);
                        continue;
                    }

                    migrationObjects.Add(mo);

                    // try to map user via id
                    if (targetUserAndTeamRecords.Any(x => x.Id == sourceUserOrTeam.Id))
                    {
                        mo.TargetOwnerId = sourceUserOrTeam.Id;
                        mo.WillBeMigrated = true;
                    }

                    // needs to be mapped via domainname
                    else if (sourceUserOrTeam.Attributes.TryGetValue("domainname", out object sourceUserDomainName))
                    {
                        CustomLog($"Unable to map source user {sourceUserOrTeam.Id} {sourceUserOrTeam.Attributes["fullname"]} via ID, will try to find a user with the same domainname on the target system");
                        var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("domainname") && t.Attributes["domainname"].ToString() == sourceUserDomainName.ToString());

                        if (mappingCandidate != null)
                        {
                            mo.TargetOwnerId = mappingCandidate.Id;
                            mo.WillBeMigrated = true;
                            CustomLog($"   Mapped user via Domain Name: {sourceUserDomainName}.");
                        }
                        else
                        {
                            CustomLog($"   Mapping failed, this user's views will not be migrated: {sourceUserOrTeam.Attributes["fullname"]}");
                            mo.WillBeMigrated = false;
                            mo.NotMigrateReason = "This user could not be mapped to the target system. The user's views will not be migrated.";
                        }
                    }
                }
                else if (sourceUserOrTeam.LogicalName == "team")
                {
                    var mo = new MigrationObject(UpdateNode, sourceUserOrTeam.LogicalName, sourceUserOrTeam.Id, sourceUserOrTeam.Attributes["name"].ToString());
                    migrationObjects.Add(mo);

                    // try to map team via id
                    if (targetUserAndTeamRecords.Any(u => u.Id == sourceUserOrTeam.Id))
                    {
                        // could be mapped via id
                        mo.TargetOwnerId = sourceUserOrTeam.Id;
                        mo.WillBeMigrated = true;
                    }

                    // needs to be mapped via team name
                    // get the name from the source team with that id
                    else if (sourceUserOrTeam.Attributes.TryGetValue("name", out object sourceTeamname))
                    {
                        CustomLog($"Unable to map source team {sourceUserOrTeam.Id} {sourceUserOrTeam.Attributes["name"]} via ID");
                        var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("name") && t.Attributes["name"].ToString() == sourceTeamname.ToString());
                        if (mappingCandidate != null)
                        {
                            // found a team that can be mapped 
                            mo.TargetOwnerId = mappingCandidate.Id;
                            mo.WillBeMigrated = true;
                            CustomLog($"   Mapped team via Domain Name: {sourceTeamname}.");
                        }
                        else
                        {
                            CustomLog($"   Unable to map source team {sourceUserOrTeam.Id} - {sourceTeamname} via ID or name. This Team's views will not be migrated.");
                            mo.WillBeMigrated = false;
                            mo.NotMigrateReason = "This team could not be mapped to the target system. The team's views will not be migrated.";
                        }
                    }
                    else
                    {
                        // the source team name is empty
                        CustomLog($"   This team does not seem to have a name in the source system, mapping is not possible.");
                        mo.WillBeMigrated = false;
                        mo.NotMigrateReason = "This team could not be mapped to the target system. The team's views will not be migrated.";
                    }
                }
            }

            CustomLog("Completed mapping users and teams.");
        }

        private void OnUsersRetrieved(RunWorkerCompletedEventArgs obj)
        {
            if (obj.Cancelled)
            {
                return;
            }

            CustomLog($"Retrieved {sourceUserAndTeamRecords.Count} users/teams from the source system.");
            CustomLog($"Retrieved {targetUserAndTeamRecords.Count} users/teams from the target system.");

            tbUsersLoadStatus.Text = $"{sourceUserAndTeamRecords.Count} users / teams retrieved";

            var usersNotInTarget = sourceUserAndTeamRecords.Where(u => !targetUserAndTeamRecords.Any(x => x.Id == u.Id)).Select(x => x).ToList();

            if (usersNotInTarget.Count > 0)
            {
                CustomLog($"There are {usersNotInTarget.Count} users or teams in the source system, which do not exist in the target system.");
            }
            btnLoadPersonalViews.Enabled = true;
            CustomLog("----------------------");
        }

        private void RetrievePersonalViews(BackgroundWorker worker, DoWorkEventArgs args)
        {
            CustomLog("Retrieving personal views...");

            sourceConnection.RemoveImpersonation();
            var impersonatedSourceConnection = sourceConnection.GetCrmServiceClient();

            foreach (var mo in migrationObjects.Where(m => m.WillBeMigrated)) // only work on users or teams that are marked for migration
            {
                // try to find source user or team that corresponds to this migration object
                var sourceUserOrTeam = sourceUserAndTeamRecords.FirstOrDefault(x => x.Id == mo.SourceOwnerId);

                if (sourceUserOrTeam.LogicalName == "systemuser")
                {
                    try
                    {
                        impersonatedSourceConnection.CallerId = sourceUserOrTeam.Id;
                        var userPersonalViews = impersonatedSourceConnection.RetrieveAll(new FetchExpression(string.Format(fetch_PersonalViewsOwnedByUserOrTeam, sourceUserOrTeam.Id)));

                        if (!userPersonalViews.Any())
                        {
                            mo.WillBeMigrated = false;
                            mo.NotMigrateReason = "This user has no personal views that could be migrated.";
                        }
                        else
                        {
                            userPersonalViews.ForEach(
                                x =>
                                {
                                    mo.PersonalViewsMigrationObjects.Add(new PersonalViewMigrationObject(UpdateNode, mo, x, x.Attributes["name"].ToString(), mo.WillBeMigrated));
                                }
                            );
                        }
                        CustomLog($"{sourceUserOrTeam.Attributes["fullname"]} has {userPersonalViews.Count} personal views.");

                    }
                    catch (Exception ex)
                    {
                        CustomLog($"Error ({ex.GetType().Name}) while retrieving personal views owned by: {sourceUserOrTeam.Attributes["fullname"]}: {ex.Message}");
                        mo.MigrationResult = MigrationResult.SucessfulWithSomeErrors;
                    }
                }
                else if (sourceUserOrTeam.LogicalName == "team")
                {
                    try
                    {
                        // impersonate the team admin
                        var teamAdminId = Guid.Parse(sourceUserOrTeam.Attributes["administratorid"].ToString());
                        impersonatedSourceConnection.CallerId = teamAdminId;
                        var teamPersonalViews = impersonatedSourceConnection.RetrieveAll(new FetchExpression(string.Format(fetch_PersonalViewsOwnedByUserOrTeam, sourceUserOrTeam.Id)));

                        if (!teamPersonalViews.Any())
                        {
                            mo.WillBeMigrated = false;
                            mo.NotMigrateReason = "This team owns no personal views that could be migrated.";
                        }
                        else
                        {
                            teamPersonalViews.ForEach(
                                x =>
                                {
                                    mo.PersonalViewsMigrationObjects.Add(new PersonalViewMigrationObject(UpdateNode, mo, x, x.Attributes["name"].ToString(), mo.WillBeMigrated));
                                }
                            );
                        }
                        CustomLog($"{sourceUserOrTeam.Attributes["name"]} owns {teamPersonalViews.Count} personal views.");

                    }
                    catch (Exception ex)
                    {
                        CustomLog($"Error ({ex.GetType().Name}) while retrieving personal views owned by: {sourceUserOrTeam.Attributes["name"]}: {ex.Message}");
                        mo.MigrationResult = MigrationResult.SucessfulWithSomeErrors;
                    }
                }
            }

            CustomLog("Done.");
            CustomLog($"Retrieved a total of {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} personal views, owned by {migrationObjects.Count} users or teams.");
        }

        private void OnPersonalViewsRetrieved(RunWorkerCompletedEventArgs obj)
        {
            btnLoadSharing.Enabled = true;
            tbPersonalViewsLoadedStatus.Text = $"Retrieved {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} views.";
            CustomLog("----------------------");
        }

        private void RetrieveSharings(BackgroundWorker worker, DoWorkEventArgs args)
        {
            CustomLog("Retrieving the Sharings for the loaded personal views..");

            // sourceConnection might still be impersonating someone. Force CallerId to be empty
            sourceConnection.RemoveImpersonation();

            try
            {

                foreach (var migrationObject in migrationObjects.Where(x => x.PersonalViewsMigrationObjects.Any()))
                {
                    // user level
                    int sharesByCurrentUser = 0;
                    int sharesThatCouldNotbeMapped = 0;
                    int sharesThatCouldBeMapped = 0;

                    foreach (var personalViewMigrationObject in migrationObject.PersonalViewsMigrationObjects)
                    {

                        // clear PoA Collections 
                        personalViewMigrationObject.MappedSharings.Clear();
                        personalViewMigrationObject.Sharings.Clear();

                        // personal view level
                        var accessRequest = new RetrieveSharedPrincipalsAndAccessRequest
                        {
                            Target = personalViewMigrationObject.PersonalView.ToEntityReference()
                        };

                        var accessResponse = (RetrieveSharedPrincipalsAndAccessResponse)sourceConnection.GetCrmServiceClient().ExecuteCrmOrganizationRequest(accessRequest);

                        if (accessResponse == null)
                        {
                            // this view was not shared
                            continue;
                        }

                        // perform mapping if necessary
                        foreach (var poa in accessResponse.PrincipalAccesses)
                        {
                            sharesByCurrentUser++;

                            // add the non-mapped poa object
                            personalViewMigrationObject.Sharings.Add(poa);

                            // try mapping
                            if (poa.Principal.LogicalName == "systemuser")
                            {
                                var targetUser = targetUserAndTeamRecords.FirstOrDefault(x => x.Id == poa.Principal.Id);
                                if (targetUser != null)
                                {
                                    // could be mapped via id

                                    SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poa, $"Shared with User {targetUser.Attributes["fullname"]}", true);
                                    personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);
                                    sharesThatCouldBeMapped++;
                                }
                                else
                                {
                                    object sourceDomainName = null;

                                    // get the domainname from the source user with that id
                                    if (sourceUserAndTeamRecords.Where(x => x.LogicalName == "systemuser").FirstOrDefault(x => x.Id == poa.Principal.Id)?.Attributes.TryGetValue("domainname", out sourceDomainName) == true)
                                    {
                                        var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("domainname") && t.Attributes["domainname"].ToString() == sourceDomainName.ToString());
                                        if (mappingCandidate != null)
                                        {
                                            var poaCopy = poa;
                                            poaCopy.Principal = mappingCandidate.ToEntityReference();

                                            SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poaCopy, $"Shared with User {mappingCandidate.Attributes["fullname"]}" , true);
                                            personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);
                                            sharesThatCouldBeMapped++;
                                        }
                                        else
                                        {
                                            sharesThatCouldNotbeMapped++;
                                            SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poa, $"Shared with User {poa.Principal.Id}", false);
                                            sharingMigrationObject.NotMigrateReason = "Shared with a user that could not be mapped to the target system.";
                                            personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);

                                            CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a user with the id {poa.Principal.Id}. " +
                                                $"This Id does not exist in the target system and the user's domainname adress was not found in the target system for mapping. This sharing will be skipped.");
                                        }
                                    }
                                    else
                                    {
                                        // this source user has no domainname address and cant be mapped by id either
                                        sharesThatCouldNotbeMapped++;
                                        SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poa, $"Shared with User {poa.Principal.Id}", false);
                                        sharingMigrationObject.NotMigrateReason = "Shared with a user that could not be mapped to the target system.";
                                        personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);

                                        CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a user with the id {poa.Principal.Id}. This Id does not exist in the target system and the user didnt contain an domainname adress that could be used for mapping. This sharing will be skipped.");
                                    }
                                }
                            }
                            else if (poa.Principal.LogicalName == "team")
                            {
                                var targetTeam = targetUserAndTeamRecords.FirstOrDefault(x => x.Id == poa.Principal.Id);

                                if (targetTeam != null)
                                {
                                    // could be mapped via id
                                    SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poa, $"Shared with User {targetTeam.Attributes["name"]}", true);
                                    personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);
                                    sharesThatCouldBeMapped++;
                                }
                                else
                                {
                                    object sourceTeamname = null;
                                    // get the name from the source team with that id
                                    if (sourceUserAndTeamRecords.Where(x => x.LogicalName == "team").FirstOrDefault(x => x.Id == poa.Principal.Id)?.Attributes.TryGetValue("name", out sourceTeamname) == true)
                                    {
                                        var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("name") && t.Attributes["name"].ToString() == sourceTeamname.ToString());
                                        if (mappingCandidate != null)
                                        {
                                            var poaCopy = poa;
                                            poaCopy.Principal = mappingCandidate.ToEntityReference();
                                            SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poaCopy, $"Shared with User {mappingCandidate.Attributes["name"]}", true);
                                            personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);
                                            sharesThatCouldBeMapped++;
                                        }
                                        else
                                        {
                                            sharesThatCouldNotbeMapped++;
                                            SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poa, $"Shared with team {poa.Principal.Id}", false);
                                            sharingMigrationObject.NotMigrateReason = "Shared with a team that could not be mapped to the target system.";
                                            personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);

                                            CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a team with the id {poa.Principal.Id}. " +
                                                $"This Id does not exist in the target system and the teams's name was not found in the target system for mapping. This sharing will be skipped.");
                                        }
                                    }
                                    else
                                    {
                                        // this source team has no name  and cant be mapped by id either
                                        sharesThatCouldNotbeMapped++;
                                        SharingMigrationObject sharingMigrationObject = new SharingMigrationObject(UpdateNode, personalViewMigrationObject, poa, $"Shared with team {poa.Principal.Id}", false);
                                        sharingMigrationObject.NotMigrateReason = "Shared with a team that could not be mapped to the target system.";
                                        personalViewMigrationObject.MappedSharings.Add(sharingMigrationObject);

                                        CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a team with the id {poa.Principal.Id}. This Id does not exist in the target system and the team didnt contain a name adress that could be used for mapping. This sharing will be skipped.");
                                    }
                                }
                            }
                        }
                    }
                    CustomLog($"{migrationObject.OwnerName} has {sharesByCurrentUser} sharings of which {sharesThatCouldBeMapped} could be mapped to a target user / team and {sharesThatCouldNotbeMapped} could not be mapped.");
                }
            }
            catch (Exception ex)
            {
                LogError($"Exception thrown: {ex.Message}. {ex.StackTrace}" );
                CustomLog("Exception while retrieving the Sharings.");
                throw;
            }

            CustomLog($"Done. Retrieved {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count))} PrincipalAccessObjects in total.");
        }
 
        private void OnSharingRetrieved(RunWorkerCompletedEventArgs obj)
        {
            tbSharingRetrievedStatus.Text = $"Retrieved {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count))} PoAs";
            btnStartMigration.Enabled = true;
            CustomLog("----------------------");
        }

        private void Migrate(BackgroundWorker worker, DoWorkEventArgs args)
        {
            CustomLog("Starting migration..");

            int currentUserTeamCount = 0;
            int totalUserTeamCount = migrationObjects.Count;

            int currentViewCount = 0;
            int totalViewCount = migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count);

            int currentPoACount = 0;
            int totalPoACount = migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count));

            foreach (var migrationObject in migrationObjects.Where(m => m.WillBeMigrated)) 
            {
                currentUserTeamCount++;

                if ( !migrationObject.PersonalViewsMigrationObjects.Any())
                {
                    // this user does not own any views 
                    continue;
                }
                else if (migrationObject.TargetOwnerId == null || migrationObject.TargetOwnerId == Guid.Empty)
                {
                    CustomLog($"The user {migrationObject.SourceOwnerId} has personal views but the user could not be mapped to a corresponding record in the target system. {migrationObject.PersonalViewsMigrationObjects.Count} views owned by that user will not be migrated.");
                    continue;
                }

                try
                {
                    // impersonate the owner of this batch
                    CustomLog($"Migrating Views owned by User / Team with: {migrationObject.OwnerName}..");
                    targetConnection.RemoveImpersonation();
                    var impersonatedConnection = targetConnection.GetCrmServiceClient();
                    impersonatedConnection.CallerId = migrationObject.TargetOwnerId;

                    // --- upsert personal views ---
                    bool allPersonalViewsMigratedSucessful = true;
                    foreach (var personalViewMigrationObject in migrationObject.PersonalViewsMigrationObjects.Where(p => p.WillBeMigrated))
                    {
                        try
                        {
                            var upsertRecord = personalViewMigrationObject.PersonalView.Copy("columnsetxml", "conditionalformatting", "description", "fetchxml",
                                "layoutxml", "name", "querytype", "returnedtypecode", "statecode", "statuscode", "userqueryid");

                            upsertRecord.Attributes["ownerid"] = new EntityReference("systemuser", migrationObject.TargetOwnerId);

                            var createdViewId = impersonatedConnection.Upsert(upsertRecord);
                            currentViewCount++;

                            // --- migrate the sharings of this view ----
                            bool allSharingsMigratedSuccessful = true;
                            foreach (var poa in personalViewMigrationObject.MappedSharings.Where(p => p.WillBeMigrated))
                            {
                                try
                                {
                                    impersonatedConnection.Execute(new GrantAccessRequest()
                                    {
                                        PrincipalAccess = new PrincipalAccess()
                                        {
                                            AccessMask = poa.PoAObject.AccessMask,
                                            Principal = poa.PoAObject.Principal
                                        },
                                        Target = new EntityReference(upsertRecord.LogicalName, createdViewId)
                                    });
                                    poa.MigrationResult = MigrationResult.Sucessful;
                                    currentPoACount++;
                                }
                                catch (Exception ex)
                                {
                                    CustomLog($"Exception while migrating sharing: {ex.GetType().Name}: {ex.Message}");
                                    poa.NotMigrateReason = "Exception while migrating sharing, please see log for exception details";
                                    poa.MigrationResult = MigrationResult.Failed;
                                    allSharingsMigratedSuccessful = false;
                                    allPersonalViewsMigratedSucessful = false;
                                }
                            }

                            // update the migration status of the view
                            if (allSharingsMigratedSuccessful)
                            {
                                personalViewMigrationObject.MigrationResult = MigrationResult.Sucessful;
                            }
                            else
                            {
                                personalViewMigrationObject.MigrationResult = MigrationResult.SucessfulWithSomeErrors;
                            }
                        }
                        catch (Exception ex)
                        {
                            CustomLog($"Exception while migrating view '{personalViewMigrationObject.PersonalViewName}': {ex.GetType().Name}: {ex.Message}");
                            personalViewMigrationObject.NotMigrateReason = "Exception while migrating personal view, please see log for exception details.";
                            personalViewMigrationObject.MigrationResult = MigrationResult.Failed;
                            allPersonalViewsMigratedSucessful = false;

                            // update sharings below
                            foreach (var item in personalViewMigrationObject.MappedSharings)
                            {
                                item.WillBeMigrated = false;
                                item.NotMigrateReason = "Parent View could not be migrated so this sharing won't be migrated either.";
                                item.MigrationResult = MigrationResult.Failed;
                            }
                        }
                    }

                    // entire user/team incl. child views and poa have been  migrated at this point
                    migrationObject.MigrationResult = (allPersonalViewsMigratedSucessful) ? MigrationResult.Sucessful : MigrationResult.Failed;
                }
                catch (Exception ex)
                {
                    // only impersation errors should reach this point
                    CustomLog($"Exception while migrating user {migrationObject.OwnerName}: {ex.GetType().Name}. {ex.Message}");
                    migrationObject.MigrationResult = MigrationResult.Failed;
                }
            }

            CustomLog("----------------------");
            CustomLog($"Migration completed. Migrated {currentViewCount} views owned by {currentUserTeamCount} users or teams and shared them with {currentPoACount} users or teams.");


            CustomLog($"Migrated Users / Teams: {Environment.NewLine}" +
                $" Successfully migrated: {migrationObjects.Count(m => m.MigrationResult == MigrationResult.Sucessful)} {Environment.NewLine}" +
                $" Migrated with issues: {migrationObjects.Count(m => m.MigrationResult == MigrationResult.SucessfulWithSomeErrors)} {Environment.NewLine}" +
                $" Migration failed: {migrationObjects.Count(m => m.MigrationResult == MigrationResult.Failed)} {Environment.NewLine}" +
                $" Marked for migration: {migrationObjects.Count(m => m.WillBeMigrated)}" +
                $" Not marked for migration: {migrationObjects.Count(m => !m.WillBeMigrated)}");

            CustomLog($"Migrated Views: {Environment.NewLine}" +
                $" Successfully migrated: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count(v => v.MigrationResult == MigrationResult.Sucessful))} {Environment.NewLine}" +
                $" Migrated with issues: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count(v => v.MigrationResult == MigrationResult.SucessfulWithSomeErrors))} {Environment.NewLine}" +
                $" Migration failed: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count(v => v.MigrationResult == MigrationResult.Failed))} {Environment.NewLine}" +
                $" Marked for migration: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count(v => v.WillBeMigrated))}");

            CustomLog($"Migrated Sharings: {Environment.NewLine}" +
                $" Successfully migrated: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(v => v.MappedSharings.Count(s => s.MigrationResult == MigrationResult.Sucessful)))} {Environment.NewLine}" +
                $" Migrated with issues: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(v => v.MappedSharings.Count(s => s.MigrationResult == MigrationResult.SucessfulWithSomeErrors)))} {Environment.NewLine}" +
                $" Migration failed: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(v => v.MappedSharings.Count(s => s.MigrationResult == MigrationResult.Failed)))} {Environment.NewLine}" +
                $" Marked for migration: {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(v => v.MappedSharings.Count(s => s.WillBeMigrated)))}");
        }


        private void OnMigrateCompleted(RunWorkerCompletedEventArgs obj)
        {
            btnConnectSource.Enabled = false;
            btnConnectTargetOrg.Enabled = false;
            btnLoadPersonalViews.Enabled = false;
            btnLoadSharing.Enabled = false;
            btnLoadUsers.Enabled = false;
            btnStartMigration.Enabled = false;

            tbMigrationResult.Text = "Migration completed.";
        }

        private List<Entity> RetrieveUsersAndTeams(IOrganizationService service)
        {
            var usersQuery = new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet("fullname", "domainname", "internalemailaddress", "isdisabled"),
                PageInfo = new PagingInfo()
                {
                    Count = 5000,
                    PageNumber = 1
                }
            };

            usersQuery.AddOrder("fullname", OrderType.Ascending);

            var allRecords = service.RetrieveAll(usersQuery);

            var teamsQuery = new QueryExpression("team")
            {
                ColumnSet = new ColumnSet("name", "administratorid"),
                PageInfo = new PagingInfo()
                {
                    Count = 5000,
                    PageNumber = 1
                }
            };

            teamsQuery.Criteria.AddCondition("teamtype", ConditionOperator.Equal, 0);
            teamsQuery.AddOrder("name", OrderType.Ascending);

            allRecords.AddRange(service.RetrieveAll(teamsQuery));

            return allRecords;
        }

        private void UpdateNode(NodeUpdateObject nodeUpdateObject)
        {
            if (treeView1.InvokeRequired)
            {
                treeView1.Invoke(new _updateTreeNodeDelegate(UpdateNode), nodeUpdateObject);
            }
            else
            {
                try
                {
                    switch (nodeUpdateObject.UpdateReason)
                    {
                        case UpdateReason.AddedToList:
                            var parentNode = treeView1.Nodes.Find(nodeUpdateObject.ParentNodeId, true).FirstOrDefault();
                            if (parentNode == null) throw new Exception("Tried to add a node under a parent that does not exist.");
                            var createNode = new TreeNode()
                            {
                                Name = nodeUpdateObject.NodeId,
                                Text = nodeUpdateObject.NodeText
                            };
                            // grey out the node if we already know that it will not be migrated
                            if (nodeUpdateObject.WillMigrate)
                            {
                                createNode.ForeColor = System.Drawing.Color.Black;
                                createNode.ToolTipText = "This element will be migrated.";
                            }
                            else
                            {
                                createNode.ForeColor = System.Drawing.Color.DimGray;
                                createNode.ToolTipText = nodeUpdateObject.NotMigrateReason;
                            }

                            parentNode.Nodes.Add(createNode);
                            parentNode.Expand();
                            break;

                        case UpdateReason.DetailsAdded:
                            var updateNode = treeView1.Nodes.Find(nodeUpdateObject.NodeId, true).FirstOrDefault();
                            if (nodeUpdateObject.WillMigrate)
                            {
                                updateNode.ForeColor = System.Drawing.Color.Black;
                                updateNode.ToolTipText = "This element will be migrated.";
                            }
                            else
                            {
                                updateNode.ForeColor = System.Drawing.Color.DimGray;
                                updateNode.ToolTipText = nodeUpdateObject.NotMigrateReason;
                            }
                            break;

                        case UpdateReason.RemovedFromList:
                            // not implemented
                            break;

                        case UpdateReason.MigrationFailed:
                            var failedNode = treeView1.Nodes.Find(nodeUpdateObject.NodeId, true).FirstOrDefault();
                            failedNode.ForeColor = System.Drawing.Color.Red;
                            break;

                        case UpdateReason.MigrationSucceeded:
                            var sucessfulNode = treeView1.Nodes.Find(nodeUpdateObject.NodeId, true).FirstOrDefault();
                            sucessfulNode.ForeColor = System.Drawing.Color.Green;
                            break;


                        case UpdateReason.MigrationSucessfulWithErrors:
                            var completedWithErrorsNode = treeView1.Nodes.Find(nodeUpdateObject.NodeId, true).FirstOrDefault();
                            completedWithErrorsNode.ForeColor = System.Drawing.Color.Orange;
                            break;

                        default:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    LogError("Exception building the TreeView. This is not critical and the migration can resume.");
                    LogError(ex.Message + " " + ex.StackTrace);
                }
            }
        }

        #endregion

        #region Helpers

        private void CustomLog(string text)
        {
            if (lbDebugOutput.InvokeRequired)
            {
                _updateLogWindowDelegate update = new _updateLogWindowDelegate(CustomLog);
                lbDebugOutput.Invoke(update, text);
            }
            else
            {
                LogInfo(text);
                lbDebugOutput.Items.Add(text);
                if (lbDebugOutput.Items.Count > 1000)
                {
                    lbDebugOutput.Items.RemoveAt(0); // remove first line
                }
                // Make sure the last item is made visible
                lbDebugOutput.SelectedIndex = lbDebugOutput.Items.Count - 1;
                lbDebugOutput.ClearSelected();
            }
        }

        #endregion

    }
}