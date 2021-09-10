﻿using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PersonalViewMigrationTool.Dto;
using PersonalViewMigrationTool.Extensions;
using System;
using System.Collections.Generic;
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
        private List<MigrationObject> migrationObjects = new List<MigrationObject>();
        private delegate void _updateTreeNodeDelegate(NodeUpdateObject nodeUpdateObject);
        private delegate void _updateLogWindowDelegate(string msg);

        // TODO: Limit columns
        const string fetch_PersonalViewsCreatedByUser = @"
            <fetch>
              <entity name='userquery'>
                <filter>
                  <condition attribute='createdby' operator='eq' value='{0}'/>
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
                Message = "Mapping Users and Loading Personal Views",
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

            args.Result = new object[] { sourceUsers, destinationUsers };
        }

        private void OnUsersRetrieved(RunWorkerCompletedEventArgs obj)
        {
            if (obj.Cancelled)
            {
                return;
            }

            var results = (object[])obj.Result;

            sourceUserAndTeamRecords = (List<Entity>)results[0];
            targetUserAndTeamRecords = (List<Entity>)results[1];

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
                    var targetNode = treeView1.Nodes.Find(nodeUpdateObject.NodeId, true).FirstOrDefault();
                    if (targetNode == null)
                    {
                        // node needs to be created

                        // get parent node
                        var parentNode = treeView1.Nodes.Find(nodeUpdateObject.ParentNodeId, true).FirstOrDefault();

                        if (parentNode == null) throw new Exception("Tried to add a node under a parent that does not exist.");

                        targetNode = new TreeNode()
                        {
                            Name = nodeUpdateObject.NodeId,
                            Text = nodeUpdateObject.NodeText
                        };
                        parentNode.Nodes.Add(targetNode);
                        parentNode.Expand();
                    }

                    // node is created at that point, update it

                    if (!nodeUpdateObject.WillMigrate)
                    {
                        targetNode.ForeColor = System.Drawing.Color.DimGray;
                        targetNode.ToolTipText = nodeUpdateObject.NotMigrateReason;
                    }
                }
                catch (Exception ex)
                {
                    LogError("Exception building the TreeView. This is not critical and the migration can resume.");
                    LogError(ex.Message + " " + ex.StackTrace);
                }
            }
        }

        private void RetrievePersonalViews(BackgroundWorker worker, DoWorkEventArgs args)
        {
            CustomLog("Mapping users / teams from source to target...");

            // source might still be impersonating someone. Force CallerId to be empty
            sourceConnection.RemoveImpersonation();
            migrationObjects.Clear();

            foreach (var sourceUserOrTeam in sourceUserAndTeamRecords)
            {
                if (sourceUserOrTeam.LogicalName == "systemuser")
                {
                    var mo = new MigrationObject(UpdateNode, sourceUserOrTeam.LogicalName, sourceUserOrTeam.Id, sourceUserOrTeam.Attributes["fullname"].ToString());
                    migrationObjects.Add(mo);

                    // try to map user via id
                    if (targetUserAndTeamRecords.Any(x => x.Id == sourceUserOrTeam.Id))
                    {
                        mo.MappedOwnerId = sourceUserOrTeam.Id;
                    }

                    // needs to be mapped via domainname
                    else if (sourceUserOrTeam.Attributes.TryGetValue("domainname", out object sourceUserDomainName))
                    {
                        CustomLog($"Unable to map source user {sourceUserOrTeam.Id} {sourceUserOrTeam.Attributes["fullname"]} via ID, will try to find a user with the same domainname on the target system");
                        var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("domainname") && t.Attributes["domainname"].ToString() == sourceUserDomainName.ToString());

                        if (mappingCandidate != null)
                        {
                            mo.MappedOwnerId = mappingCandidate.Id;
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
                        mo.MappedOwnerId = sourceUserOrTeam.Id;
                    }

                    // needs to be mapped via team name
                    // get the name from the source team with that id
                    else if (sourceUserOrTeam.Attributes.TryGetValue("name", out object sourceTeamname))
                    {
                        var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("name") && t.Attributes["name"].ToString() == sourceTeamname.ToString());
                        if (mappingCandidate != null)
                        {
                            // found a team that can be mapped 
                            mo.MappedOwnerId = mappingCandidate.Id;
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
            CustomLog("Retrieving personal views...");
            // mapping completed for the current user / team .. retrieve the personal views now
            foreach (var sourceUser in sourceUserAndTeamRecords.Where(x => x.LogicalName == "systemuser"))
            {

                var mo = migrationObjects.FirstOrDefault(x => x.OwnerId == sourceUser.Id);

                var impersonatedSourceConnection = sourceConnection.GetCrmServiceClient();
                impersonatedSourceConnection.CallerId = sourceUser.Id;

                var userPersonalViews = impersonatedSourceConnection.RetrieveAll(new FetchExpression(string.Format(fetch_PersonalViewsCreatedByUser, sourceUser.Id)));

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
                            mo.PersonalViewsMigrationObjects.Add(new PersonalViewMigrationObject(UpdateNode, mo, x, x.Attributes["name"].ToString()));
                        }
                    );
                }
                CustomLog($"{sourceUser.Attributes["fullname"]} has {userPersonalViews.Count} personal views.");
            }

            CustomLog("Done.");
            CustomLog($"Retrieved a total of {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} personal views, created by {migrationObjects.Count} users that can be mapped.");
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
                                if (targetUserAndTeamRecords.Any(u => u.Id == poa.Principal.Id))
                                {
                                    // could be mapped via id
                                    personalViewMigrationObject.MappedSharings.Add(poa);
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
                                            personalViewMigrationObject.MappedSharings.Add(poaCopy);
                                            sharesThatCouldBeMapped++;
                                        }
                                        else
                                        {
                                            sharesThatCouldNotbeMapped++;
                                            CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a user with the id {poa.Principal.Id}. " +
                                                $"This Id does not exist in the target system and the user's domainname adress was not found in the target system for mapping. This sharing will be skipped.");
                                        }
                                    }
                                    else
                                    {
                                        // this source user has no domainname address and cant be mapped by id either
                                        sharesThatCouldNotbeMapped++;
                                        CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a user with the id {poa.Principal.Id}. This Id does not exist in the target system and the user didnt contain an domainname adress that could be used for mapping. This sharing will be skipped.");
                                    }
                                }
                            }
                            else if (poa.Principal.LogicalName == "team")
                            {
                                if (targetUserAndTeamRecords.Any(u => u.Id == poa.Principal.Id))
                                {
                                    // could be mapped via id
                                    personalViewMigrationObject.MappedSharings.Add(poa);
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
                                            personalViewMigrationObject.MappedSharings.Add(poaCopy);
                                            sharesThatCouldBeMapped++;
                                        }
                                        else
                                        {
                                            sharesThatCouldNotbeMapped++;
                                            CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a team with the id {poa.Principal.Id}. " +
                                                $"This Id does not exist in the target system and the teams's name was not found in the target system for mapping. This sharing will be skipped.");
                                        }
                                    }
                                    else
                                    {
                                        // this source team has no name  and cant be mapped by id either
                                        sharesThatCouldNotbeMapped++;
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

            foreach (var migrationObject in migrationObjects) 
            {
                currentUserTeamCount++;

                if ( !migrationObject.PersonalViewsMigrationObjects.Any())
                {
                    // this user does not own any views 
                    continue;
                }
                else if (migrationObject.MappedOwnerId == null || migrationObject.MappedOwnerId == Guid.Empty)
                {
                    CustomLog($"The user {migrationObject.OwnerId} has personal views but the user could not be mapped to a corresponding record in the target system. {migrationObject.PersonalViewsMigrationObjects.Count} views owned by that user will not be migrated.");
                    continue;
                }

                // impersonate the owner of this batch
                CustomLog($"Migrating Views owned by User / Team with: {migrationObject.OwnerName}..");
                var impersonatedConnection = targetConnection.GetCrmServiceClient();
                impersonatedConnection.CallerId = migrationObject.MappedOwnerId;

                foreach (var personalViewMigrationObject in migrationObject.PersonalViewsMigrationObjects)
                {
                    var upsertRecord = personalViewMigrationObject.PersonalView.Copy("columnsetxml", "conditionalformatting", "description", "fetchxml",
                   "layoutxml", "name", "querytype", "returnedtypecode", "statecode", "statuscode", "userqueryid");

                    upsertRecord.Attributes["ownerid"] = new EntityReference("systemuser", migrationObject.MappedOwnerId);

                    impersonatedConnection.Upsert(upsertRecord);
                    currentViewCount++;

                    // migrate all sharings
                    foreach (var poa in personalViewMigrationObject.MappedSharings)
                    {
                        impersonatedConnection.Execute(new GrantAccessRequest()
                        {
                            PrincipalAccess = new PrincipalAccess()
                            {
                                AccessMask = poa.AccessMask,
                                Principal = poa.Principal
                            },
                            Target = upsertRecord.ToEntityReference() // note this only works because the ID of the record is the same, better to use the id of the newly created record instead.
                        });
                        currentPoACount++;
                    }
                    CustomLog($"Migrated User / Team {currentUserTeamCount} / {totalUserTeamCount}. View {currentViewCount} / {totalViewCount}. PoA {currentPoACount} / {totalPoACount}.");
                }
            }
            CustomLog("----------------------");
            CustomLog($"Migration completed. Migrated {currentViewCount} views owned by {currentUserTeamCount} users or teams and shared them with {currentPoACount} users or teams.");

            if (currentViewCount != totalViewCount || currentPoACount != totalPoACount)
            {
                CustomLog("Not all views or sharings could be migrated. Please refer to the log file for a complete list.");
                MessageBox.Show("Not all views or sharings could be migrated. Please refer to the log file for a complete list.", "Not everything could be migrated", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                MessageBox.Show("Migration completed successfully", "", MessageBoxButtons.OK);
            }
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
                ColumnSet = new ColumnSet("fullname", "domainname", "internalemailaddress"),
                PageInfo = new PagingInfo()
                {
                    Count = 5000,
                    PageNumber = 1
                }
            };

            usersQuery.Criteria.AddCondition("domainname", ConditionOperator.NotNull);
            usersQuery.Criteria.AddCondition("domainname", ConditionOperator.NotEqual, string.Empty);
            usersQuery.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            usersQuery.AddOrder("fullname", OrderType.Ascending);

            var allRecords = service.RetrieveAll(usersQuery);

            var teamsQuery = new QueryExpression("team")
            {
                ColumnSet = new ColumnSet("name"),
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