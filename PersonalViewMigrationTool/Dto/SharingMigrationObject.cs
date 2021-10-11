using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;

namespace PersonalViewMigrationTool.Dto
{
    internal class SharingMigrationObject : MigrationObjectBase
    {
        private PersonalViewMigrationObject _personalViewMigrationObject;

        internal PrincipalAccess PoAObject { get; set; }

        internal string PoAName { get; set; }

        // need to override ChildObjects but make it clear that this is the end of the chain
        internal override IEnumerable<MigrationObjectBase> ChildObjects => default;

        #region ctor

        internal SharingMigrationObject(Action<NodeUpdateObject> updateNodeUiDelegate, PersonalViewMigrationObject parentPersonalViewMigrationObject, PrincipalAccess sharingPoA, string sharingName, bool willBeMigrated = false) 
            : base(updateNodeUiDelegate)
        {
            _personalViewMigrationObject = parentPersonalViewMigrationObject;
            PoAObject = sharingPoA;
            PoAName = sharingName;
            ElementId = PoAObject.Principal.Id.ToString();

            // ctor being called means this needs to be added to the treeview. this needs to be called after the properties have been initialized!
            updateNodeUiDelegate(new NodeUpdateObject()
            {
                ParentNodeId = _personalViewMigrationObject.PersonalView.Id.ToString(),
                MigrationObjectBase = this,
                NodeText = PoAName,
                UpdateReason= UpdateReason.AddedToList
            });

            // this needs to happen after the node object is created!
            WillBeMigrated = willBeMigrated;
        }

        #endregion
    }
}
