using System;

namespace PersonalViewMigrationTool.Dto
{
    internal class MigrationObjectBase
    {
        private readonly Action<NodeUpdateObject> updateNodeUi;
        private bool willBeMigrated;
        private MigrationResult migrationResult;
        private string notMigrateReason;

        public string ElementId { get; set; }

        public MigrationObjectBase(Action<NodeUpdateObject> updateNodeUi)
        {
            this.updateNodeUi = updateNodeUi;
        }

        internal bool WillBeMigrated
        {
            get => willBeMigrated;
            set
            {
                willBeMigrated = value;
                updateNodeUi(new NodeUpdateObject()
                {
                    MigrationObjectBase = this,
                    UpdateReason = UpdateReason.DetailsAdded,
                    WillMigrate = value
                }); 
            }
        }

        internal MigrationResult MigrationResult
        {
            get => migrationResult;
            set
            {
                migrationResult = value;

                // update ui
                updateNodeUi(new NodeUpdateObject()
                {
                    MigrationObjectBase = this,
                    UpdateReason = (value == MigrationResult.Sucessful) ? UpdateReason.MigrationSucceeded : UpdateReason.MigrationFailed,
                });
            }
        }

        internal string NotMigrateReason
        {
            get => notMigrateReason;
            set
            {
                notMigrateReason = value;
                updateNodeUi(new NodeUpdateObject()
                {
                    MigrationObjectBase = this,
                    NotMigrateReason = value,
                    UpdateReason = UpdateReason.DetailsAdded
                });
            }
        }
    }
}