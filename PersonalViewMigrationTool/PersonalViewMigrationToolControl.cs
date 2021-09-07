using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

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

            LogToFakeConsole($"Retrieved {sourceUserAndTeamRecords.Count} users/teams from the source system.");
            LogToFakeConsole($"Retrieved {targetUserAndTeamRecords.Count} users/teams from the target system.");
            LogToFakeConsole("Checking whether those can be mapped 1:1 via their IDs...");

            lblUsersLoadStatus.Text = $"{sourceUserAndTeamRecords.Count} users retrieved";

            var usersNotInTarget = sourceUserAndTeamRecords.Where(u => !targetUserAndTeamRecords.Any(x => x.Id == u.Id)).Select(x => x).ToList();

            if (usersNotInTarget.Count > 0)
            {
                LogToFakeConsole($"There are {usersNotInTarget.Count} users or teams in the source system, which do not exist in the target system:");
                foreach (var user in usersNotInTarget)
                {
                    LogToFakeConsole($"Id: {user.Id}");
                }
            }
            btnLoadPersonalViews.Enabled = true;
        }

        private void RetrievePersonalViews(BackgroundWorker worker, DoWorkEventArgs args)
        {
            foreach (var user in sourceUserAndTeamRecords)
            {
                LogToFakeConsole($"Getting personal views of user: {user.Id}");
                var userPersonalViews = sourceConnection.GetCrmServiceClient().RetrieveAll(new FetchExpression(string.Format(fetch_PersonalViewsByUser, user.Id)));

                LogToFakeConsole($"This user has {userPersonalViews.Count} personal views which could be migrated.");

                var mo = new MigrationObject()
                {
                    OwnerId = user.Id,
                    PersonalViewsMigrationObjects = new List<PersonalViewMigrationObject>()
                };

                userPersonalViews.ForEach(x => mo.PersonalViewsMigrationObjects.Add(new PersonalViewMigrationObject { PersonalView = x }));

                migrationObjects.Add(mo);
            }

            LogToFakeConsole($"Retrieved a total of {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} personal views, owned between {migrationObjects.Count} users or teams.");
        }

        private void OnPersonalViewsRetrieved(RunWorkerCompletedEventArgs obj)
        {
            btnLoadSharing.Enabled = true;
            lblPersonalViewsLoadedStatus.Text = $"Retrieved {migrationObjects.Sum(x => x.PersonalViewsMigrationObjects.Count)} views.";
        }

        private void RetrieveSharings(BackgroundWorker worker, DoWorkEventArgs args)
        {
            LogToFakeConsole("Retrieving the Sharings for the loaded personal views..");

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

                    personalViewMigrationObject.Sharings.AddRange(accessResponse.PrincipalAccesses);
                }
            }

            LogToFakeConsole($"Done. Retrieved {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count))} PrincipalAccessObjects.");
        }

        private void OnSharingRetrieved(RunWorkerCompletedEventArgs obj)
        {
            lblSharingRetrievedStatus.Text = $"Retrieved {migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count))} PoAs";
            btnStartMigration.Enabled = true;
        }

        private void OnMigrateCompleted(RunWorkerCompletedEventArgs obj)
        {
            btnConnectTargetOrg.Enabled = false;
            btnLoadPersonalViews.Enabled = false;
            btnLoadSharing.Enabled = false;
            btnLoadUsers.Enabled = false;
            btnStartMigration.Enabled = false;
        }

        private void Migrate(BackgroundWorker arg1, DoWorkEventArgs arg2)
        {
            LogToFakeConsole("Starting migration..");

            int currentUserCount = 0;
            int totalUserCount = migrationObjects.Count;

            int currentViewCount = 0;
            int totalViewCount = migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Count);

            int currentPoACount = 0;
            int totalPoACount = migrationObjects.Sum(m => m.PersonalViewsMigrationObjects.Sum(o => o.Sharings.Count));

            foreach (var migrationObject in migrationObjects)
            {
                currentUserCount++;
                // impersonate the owner of this batch
                LogToFakeConsole($"Impersonating User with ID: {migrationObject.OwnerId}");
                var impersonatedConnection = targetConnection.GetCrmServiceClient();
                impersonatedConnection.CallerId = migrationObject.OwnerId;

                // whoAmICheck
                var whoAmIResponse = impersonatedConnection.Execute(new WhoAmIRequest()) as WhoAmIResponse;
                if (whoAmIResponse.UserId != migrationObject.OwnerId)
                {
                    LogToFakeConsole("Unable to Impersonate the user! Will skip the views owned by that user.");
                    return;
                }

                foreach (var personalView in migrationObject.PersonalViewsMigrationObjects)
                {
                    impersonatedConnection.Upsert(personalView.PersonalView);
                    currentViewCount++;

                    // migrate all sharings
                    foreach (var poa in personalView.Sharings)
                    {
                        impersonatedConnection.Execute(new GrantAccessRequest()
                        {
                            PrincipalAccess = new PrincipalAccess()
                            {
                                AccessMask = poa.AccessMask,
                                Principal = poa.Principal
                            },
                            Target = personalView.PersonalView.ToEntityReference()
                        });
                        currentPoACount++;
                    }
                    LogToFakeConsole($"Migrated User {currentUserCount} / {totalUserCount}. View {currentViewCount} / {totalViewCount}. PoA {currentPoACount} / {totalPoACount}.");
                }
            }
            LogToFakeConsole("Migration completed");
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

                lblConnectedSourceOrg.Text = detail.ConnectionName;
                LogToFakeConsole($"Connected Source to: {detail.ConnectionName}");

                var whoAmIResponse = detail.GetCrmServiceClient().Execute(new WhoAmIRequest()) as WhoAmIResponse;
                LogToFakeConsole($"BusinessUnitId: {whoAmIResponse.BusinessUnitId.ToString("b")}, OrganizationId: {whoAmIResponse.OrganizationId.ToString("b")},  UserId: {whoAmIResponse.UserId.ToString("b")}");

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
                lblConnectedTargetOrg.Text = targetConnection.ConnectionName;

                LogInfo("Connection has changed to: {0}", targetConnection.WebApplicationUrl);

                LogToFakeConsole($"Connected Target to: {targetConnection.ConnectionName}");

                var whoAmIResponse = targetConnection.GetCrmServiceClient().Execute(new WhoAmIRequest()) as WhoAmIResponse;
                LogToFakeConsole($"BusinessUnitId: {whoAmIResponse.BusinessUnitId.ToString("b")}, OrganizationId: {whoAmIResponse.OrganizationId.ToString("b")},  UserId: {whoAmIResponse.UserId.ToString("b")}");
                btnLoadUsers.Enabled = true;
            }
        }



        private List<Entity> RetrieveUserRecords(IOrganizationService service)
        {
            var usersQuery = new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet("fullname"),
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

        private void LogToFakeConsole(string text)
        {
            if (lbDebugOutput.InvokeRequired)
            {
                UpdateLogWindowDelegate update = new UpdateLogWindowDelegate(LogToFakeConsole);
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


    }

    internal class MigrationObject
    {
        internal Guid OwnerId { get; set; }
        internal List<PersonalViewMigrationObject> PersonalViewsMigrationObjects { get; set; } = new List<PersonalViewMigrationObject>();
    }

    internal class PersonalViewMigrationObject
    {
        internal Entity PersonalView { get; set; }

        internal List<PrincipalAccess> Sharings { get; set; } = new List<PrincipalAccess>();
    }

}