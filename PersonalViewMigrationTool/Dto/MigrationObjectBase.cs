using System;
using System.Collections.Generic;

namespace PersonalViewMigrationTool.Dto
{

    internal abstract class MigrationObjectBase 
    {
        private readonly Action<NodeUpdateObject> updateNodeUi;
        private bool willBeMigrated;
        private MigrationResult migrationResult;
        private string notMigrateReason;

        public string ElementId { get; set; }

        internal abstract IEnumerable<MigrationObjectBase> ChildObjects { get; }

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

                // disable all child elements if there are any
                if (ChildObjects != null)
                {
                    foreach (var child in ChildObjects)
                    {
                        child.WillBeMigrated = value;
                    }
                }

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