using System;
using System.Collections.Generic;

namespace PersonalViewMigrationTool.Dto
{
    internal class MigrationObject
    {
        private bool _willBeMigrated = true;

        private string _notMigrateReason;

        private bool migrationResult;

        private readonly Action<NodeUpdateObject> updateNodeUi;

        internal string ParentNodeId { get => (OwnerLogicalName == "systemuser") ? "nUsers" : "nTeams"; }

        internal bool WillBeMigrated
        {
            get => _willBeMigrated;
            set
            {
                _willBeMigrated = value;

                if (!value)
                {
                    // if this user can not be migrated, that means none of his views will be migrated
                    PersonalViewsMigrationObjects.ForEach(x => x.WillBeMigrated = false);
                }

                // trigger refresh of the tree view
                if (updateNodeUi != null) updateNodeUi.Invoke(new NodeUpdateObject()
                {

                    ParentNodeId = (OwnerLogicalName == "systemuser") ? "nUsers" : "nTeams",
                    NodeId = OwnerId.ToString(),
                    NodeText = OwnerName,
                    NotMigrateReason = NotMigrateReason,
                    WillMigrate = value
                });
            }
        }

        internal string NotMigrateReason
        {
            get => _notMigrateReason;
            set
            {
                _notMigrateReason = value;

                // trigger refresh of the tree view
                if (updateNodeUi != null) updateNodeUi.Invoke(new NodeUpdateObject()
                {

                    ParentNodeId = (OwnerLogicalName == "systemuser") ? "nUsers" : "nTeams",
                    NodeId = OwnerId.ToString(),
                    NodeText = OwnerName,
                    NotMigrateReason = value,
                    WillMigrate = WillBeMigrated
                });
            }
        }

        internal string OwnerLogicalName { get; set; }

        internal Guid MappedOwnerId { get; set; }

        internal Guid OwnerId { get; set; }

        internal string OwnerName { get; set; }

        internal bool MigrationResult
        {
            get => migrationResult;
            set
            {
                migrationResult = value;

                // update ui
                updateNodeUi(new NodeUpdateObject()
                {
                    NodeId = OwnerId.ToString(),
                    Migrated = value
                });
            }
        }

        internal List<PersonalViewMigrationObject> PersonalViewsMigrationObjects { get; set; } = new List<PersonalViewMigrationObject>();

        public MigrationObject(Action<NodeUpdateObject> UpdateUiDelegate, string ownerLogicalName, Guid ownerId, string ownerName)
        {
            updateNodeUi = UpdateUiDelegate;
            OwnerLogicalName = ownerLogicalName;
            OwnerId = ownerId;
            OwnerName = ownerName;

            // trigger refresh of the tree view
            if (updateNodeUi != null) updateNodeUi.Invoke(new NodeUpdateObject()
            {
                ParentNodeId = ParentNodeId,
                NodeId = OwnerId.ToString(),
                NodeText = OwnerName,
                NotMigrateReason = NotMigrateReason,
                WillMigrate = WillBeMigrated
            });
        }
    }
}