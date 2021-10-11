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
        private bool canBeMigrated;

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

                if (value)
                {
                    if (ChildObjects != null)
                    {
                        foreach (var child in ChildObjects)
                        {
                            // only do this for childs that dont have a technical reason why they could not be migrated
                            if (child.CanBeMigrated)
                            {
                                child.WillBeMigrated = value;
                            }
                        }
                    }
                }
                else
                {
                    if (ChildObjects != null)
                    {
                        foreach (var child in ChildObjects)
                        {
                            child.WillBeMigrated = value;
                        }
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

        internal bool CanBeMigrated
        {
            get => canBeMigrated;
            set => canBeMigrated = value;
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