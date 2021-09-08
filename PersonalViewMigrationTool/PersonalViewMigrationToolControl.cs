using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using XrmToolBox.Extensibility;

namespace PersonalViewMigrationTool
{
    public partial class PersonalViewMigrationToolControl : MultipleConnectionsPluginControlBase
    {
        private Settings mySettings;

        private List<Entity> sourceUserAndTeamRecords = new List<Entity>();
        private List<Entity> targetUserAndTeamRecords = new List<Entity>();

        private ConnectionDetail sourceConnection;
        private ConnectionDetail targetConnection;

        private List<MigrationObject> migrationObjects = new List<MigrationObject>();

        // TODO: Limit columns
        const string fetch_PersonalViewsByUser = @"
                    <fetch>
                      <entity name='userquery'>
                        <filter>
                          <condition attribute='ownerid' operator='eq' value='{0}'/>
                        </filter>
                      </entity>
                    </fetch>";

        public PersonalViewMigrationToolControl()
        {
            InitializeComponent();
        }

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
            AddAdditionalOrganization();
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

        #endregion

        private void RetrieveUsers(BackgroundWorker worked, DoWorkEventArgs args)
        {
            var sourceUsers = RetrieveUserRecords(Service);
            var destinationUsers = RetrieveUserRecords(AdditionalConnectionDetails[0].GetCrmServiceClient());

            args.Result = new object[] { sourceUsers, destinationUsers };
        }

        private void OnUsersRetrieved(RunWorkerCompletedEventArgs obj)
        {
            var results = (object[])obj.Result;

            sourceUserAndTeamRecords = (List<Entity>)results[0];
            targetUserAndTeamRecords = (List<Entity>)results[1];

            CustomLog($"Retrieved {sourceUserAndTeamRecords.Count} users/teams from the source system.");
            CustomLog($"Retrieved {targetUserAndTeamRecords.Count} users/teams from the target system.");
            CustomLog("Checking whether those can be mapped 1:1 via their IDs...");

            tbUsersLoadStatus.Text = $"{sourceUserAndTeamRecords.Count} users retrieved";

            var usersNotInTarget = sourceUserAndTeamRecords.Where(u => !targetUserAndTeamRecords.Any(x => x.Id == u.Id)).Select(x => x).ToList();

            if (usersNotInTarget.Count > 0)
            {
                CustomLog($"There are {usersNotInTarget.Count} users or teams in the source system, which do not exist in the target system:");
                foreach (var user in usersNotInTarget)
                {
                    CustomLog($"Id: {user.Id}");
                }
            }
            btnLoadPersonalViews.Enabled = true;
        }

        private void RetrievePersonalViews(BackgroundWorker worker, DoWorkEventArgs args)
        {
            foreach (var sourceUserOrTeam in sourceUserAndTeamRecords)
            {
                var mo = new MigrationObject()
                {
                    OwnerId = sourceUserOrTeam.Id,
                    PersonalViewsMigrationObjects = new List<PersonalViewMigrationObject>()
                };

                if (targetUserAndTeamRecords.Any(u => u.Id == sourceUserOrTeam.Id))
                {
                    // could be mapped via id
                    mo.MappedOwnerId = sourceUserOrTeam.Id;
                }
                else
                {
                    if (sourceUserOrTeam.LogicalName == "systemuser")
                    {
                        // needs to be mapped via domainname
                        if (sourceUserOrTeam.Attributes.TryGetValue("domainname", out object sourceUserDomainName))
                        {
                            CustomLog($"Unable to map source user {sourceUserOrTeam.Id} {sourceUserOrTeam.Attributes["fullname"]} via ID, will try to find a user with the same domainname on the target system");
                            var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("domainname") && t.Attributes["domainname"].ToString() == sourceUserDomainName.ToString());

                            if (mappingCandidate != null)
                            {
                                mo.MappedOwnerId = mappingCandidate.Id;
                            }
                            else
                            {
                                CustomLog($"Unable to map source user {sourceUserOrTeam.Id} via ID or domainname. This User's views will not be migrated.");
                            }
                        }
                    }
                    else if (sourceUserOrTeam.LogicalName == "team")
                    {
                        // needs to be mapped via team name

                        object sourceTeamname = null;
                        // get the name from the source team with that id
                        if (sourceUserOrTeam.Attributes.TryGetValue("name", out  sourceTeamname))
                        {
                            var mappingCandidate = targetUserAndTeamRecords.FirstOrDefault(t => t.Attributes.ContainsKey("name") && t.Attributes["name"].ToString() == sourceTeamname.ToString());
                            if (mappingCandidate != null)
                            {
                                // found a team that can be mapped 
                                mo.MappedOwnerId = mappingCandidate.Id;
                            }
                            else
                            {
                                CustomLog($"Unable to map source team {sourceUserOrTeam.Id} via ID or name. This Team's views will not be migrated.");
                            }
                        }
                    }
                }

                CustomLog($"Getting personal views of user / team: {sourceUserOrTeam.Id}");
                var userPersonalViews = sourceConnection.GetCrmServiceClient().RetrieveAll(new FetchExpression(string.Format(fetch_PersonalViewsByUser, sourceUserOrTeam.Id)));

                CustomLog($"This user / team has {userPersonalViews.Count} personal views which could be migrated.");

                userPersonalViews.ForEach(x => mo.PersonalViewsMigrationObjects.Add(new PersonalViewMigrationObject { PersonalView = x }));

                migrationObjects.Add(mo);
            }

            CustomLog($"Retrieved a total of {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} personal views, owned between {migrationObjects.Count} users or teams.");
        }

        private void OnPersonalViewsRetrieved(RunWorkerCompletedEventArgs obj)
        {
            btnLoadSharing.Enabled = true;
            tbPersonalViewsLoadedStatus.Text = $"Retrieved {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} views.";
        }

        private void RetrieveSharings(BackgroundWorker worker, DoWorkEventArgs args)
        {
            CustomLog("Retrieving the Sharings for the loaded personal views..");

            foreach (var migrationObject in migrationObjects)
            {
                // user level
                foreach (var personalViewMigrationObject in migrationObject.PersonalViewsMigrationObjects)
                {
                    // personal view level
                    var accessRequest = new RetrieveSharedPrincipalsAndAccessRequest
                    {
                        Target = personalViewMigrationObject.PersonalView.ToEntityReference()
                    };
                    var accessResponse = (RetrieveSharedPrincipalsAndAccessResponse) sourceConnection.GetCrmServiceClient().ExecuteCrmOrganizationRequest(accessRequest);

                    // perform mapping if necessary
                    foreach (var poa in accessResponse.PrincipalAccesses)
                    {
                        // add the non-mapped poa object
                        personalViewMigrationObject.Sharings.Add(poa);

                        // try mapping
                        if (poa.Principal.LogicalName == "systemuser")
                        {
                            if (targetUserAndTeamRecords.Any(u => u.Id == poa.Principal.Id))
                            {
                                // could be mapped via id
                                personalViewMigrationObject.MappedSharings.Add(poa);
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
                                    }
                                    else
                                    {
                                        CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a user with the id {poa.Principal.Id}. " +
                                            $"This Id does not exist in the target system and the user's domainname adress was not found in the target system for mapping. This sharing will be skipped.");
                                    }
                                }
                                else
                                {
                                    // this source user has no domainname address and cant be mapped by id either
                                    CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a user with the id {poa.Principal.Id}. This Id does not exist in the target system and the user didnt contain an domainname adress that could be used for mapping. This sharing will be skipped.");
                                } 
                            }
                        }
                        else if(poa.Principal.LogicalName == "team")
                        {
                            if (targetUserAndTeamRecords.Any(u => u.Id == poa.Principal.Id))
                            {
                                // could be mapped via id
                                personalViewMigrationObject.MappedSharings.Add(poa);
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
                                    }
                                    else
                                    {
                                        CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a team with the id {poa.Principal.Id}. " +
                                            $"This Id does not exist in the target system and the teams's name was not found in the target system for mapping. This sharing will be skipped.");
                                    }
                                }
                                else
                                {
                                    // this source team has no name  and cant be mapped by id either
                                    CustomLog($"The view {personalViewMigrationObject.PersonalView.Id} was shared with a team with the id {poa.Principal.Id}. This Id does not exist in the target system and the team didnt contain a name adress that could be used for mapping. This sharing will be skipped.");
                                }
                            }
                        }
                    }
                }
            }

            CustomLog($"Done. Retrieved {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count))} PrincipalAccessObjects.");
        }

        private void OnSharingRetrieved(RunWorkerCompletedEventArgs obj)
        {
            tbSharingRetrievedStatus.Text = $"Retrieved {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count))} PoAs";
            btnStartMigration.Enabled = true;
        }

        private void OnMigrateCompleted(RunWorkerCompletedEventArgs obj)
        {
            btnConnectTargetOrg.Enabled = false;
            btnLoadPersonalViews.Enabled = false;
            btnLoadSharing.Enabled = false;
            btnLoadUsers.Enabled = false;
            btnStartMigration.Enabled = false;

            tbMigrationResult.Text = "Migration completed.";
        }

        private void Migrate(BackgroundWorker arg1, DoWorkEventArgs arg2)
        {
            CustomLog("Starting migration..");

            int currentUserCount = 0;
            int totalUserCount = migrationObjects.Count;

            int currentViewCount = 0;
            int totalViewCount = migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count);

            int currentPoACount = 0;
            int totalPoACount = migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count));

            foreach (var migrationObject in migrationObjects.Where(x => x.MappedOwnerId != null && x.PersonalViewsMigrationObjects.Any())) 
            {
                currentUserCount++;
                // impersonate the owner of this batch
                CustomLog($"Migrating User with ID: {migrationObject.OwnerId}..");
                var impersonatedConnection = targetConnection.GetCrmServiceClient();
                impersonatedConnection.CallerId = migrationObject.MappedOwnerId;

                // whoAmICheck
                var whoAmIResponse = impersonatedConnection.Execute(new WhoAmIRequest()) as WhoAmIResponse;
                if (whoAmIResponse.UserId != migrationObject.MappedOwnerId)
                {
                    CustomLog("Unable to Impersonate the user! Will skip the views owned by that user.");
                    continue;
                }

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
                    CustomLog($"Migrated User / Team {currentUserCount} / {totalUserCount}. View {currentViewCount} / {totalViewCount}. PoA {currentPoACount} / {totalPoACount}.");
                }
            }
            CustomLog("Migration completed");
        }


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
            }
        }


        private List<Entity> RetrieveUserRecords(IOrganizationService service)
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

        delegate void UpdateLogWindowDelegate(string msg);

        private void CustomLog(string text)
        {
            LogInfo(text);

            if (lbDebugOutput.InvokeRequired)
            {
                UpdateLogWindowDelegate update = new UpdateLogWindowDelegate(CustomLog);
                lbDebugOutput.Invoke(update, text);
            }
            else
            {
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

        private void openLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLogFile();
        }

        private void openLogFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(LogFilePath));
        }
    }
}