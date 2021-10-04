using System;
using System.Collections.Generic;

namespace PersonalViewMigrationTool.Dto
{
    /// <summary>
    /// Holds a systemuser or team and a list of the personalviews owned by it. 
    /// </summary>
    internal class MigrationObject : MigrationObjectBase
    {
        internal string ParentNodeId { get => (OwnerLogicalName == "systemuser") ? "nUsers" : "nTeams"; }

        internal string OwnerLogicalName { get; set; }

        internal Guid MappedOwnerId { get; set; }

        internal Guid OwnerId { get; set; }

        internal string OwnerName { get; set; }

        internal List<PersonalViewMigrationObject> PersonalViewsMigrationObjects { get; set; } = new List<PersonalViewMigrationObject>();

        #region ctor
        public MigrationObject(Action<NodeUpdateObject> UpdateUiDelegate, string ownerLogicalName, Guid ownerId, string ownerName, bool willBeMigrated = false) : base(UpdateUiDelegate)
        {
            OwnerLogicalName = ownerLogicalName;
            OwnerId = ownerId;
            OwnerName = ownerName;
            ElementId = ownerId.ToString();

            // trigger refresh of the tree view
            if (UpdateUiDelegate != null) UpdateUiDelegate.Invoke(new NodeUpdateObject()
            {   
                MigrationObjectBase = this,
                ParentNodeId = ParentNodeId,
                NodeText = OwnerName,
                UpdateReason= UpdateReason.AddedToList
            });
            // this needs to happen after the node object is created!
            WillBeMigrated = willBeMigrated;
        }
        #endregion
    }
}