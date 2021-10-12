using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        internal abstract MigrationObjectBase Parent {  get; }

        public MigrationObjectBase(Action<NodeUpdateObject> updateNodeUi)
        {
            this.updateNodeUi = updateNodeUi;
        }

        /// <summary>
        /// Sets the WillBeMigrated Flag to false if all child elements have been unchecked
        /// </summary>
        internal void UncheckTopNodeIfAllChildsAreUnchecked()
        {
            // propagate to the top node
            if (Parent != null)
            {
                Parent.UncheckTopNodeIfAllChildsAreUnchecked();
                return;
            }
            else
            {
                // we are executing on the top node now

                // check whether none of the immediate childs are selected for migration: 
                // User/Team -> Views -> Sharings
                // If there are still any Views that need to be migrated, we can't uncheck the User/Team Level. Sharings do not impact the selection of user/team or view level

                bool noCheckedMigrationChilds = true;

                if (ChildObjects != null)
                {
                    foreach (var child in ChildObjects)
                    {
                        if (child.WillBeMigrated)
                        {
                            // there is at least one child that should be migrated so we can't uncheck this element
                            noCheckedMigrationChilds = false;
                        }
                    }
                }

                if (noCheckedMigrationChilds)
                {
                    // there are no child elements that can be migrated so we uncheck this element but only if it was checked before
                    if(WillBeMigrated) WillBeMigrated = false;
                }
            }
        }

        internal bool WillBeMigrated
        {
            get => willBeMigrated;
            set
            {
                willBeMigrated = value;

                if (value) // will be migrated
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

                    // mark parent for migration if it was disabled before
                    if (Parent != null && !Parent.WillBeMigrated)
                    {
                        Parent.WillBeMigrated = true;
                    }
                }
                else // will not be migrated
                {
                    if (ChildObjects != null)
                    {
                        foreach (var child in ChildObjects)
                        {
                            child.WillBeMigrated = value;
                        }
                    }
                    UncheckTopNodeIfAllChildsAreUnchecked();
                }

                updateNodeUi(new NodeUpdateObject()
                {
                    MigrationObjectBase = this,
                    UpdateReason = UpdateReason.DetailsAdded,
                    WillMigrate = value
                }); ;
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