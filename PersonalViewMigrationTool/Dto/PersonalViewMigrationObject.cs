using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace PersonalViewMigrationTool.Dto
{

    internal class PersonalViewMigrationObject
    {
        private readonly Action<NodeUpdateObject> updateNodeUi;

        private bool willBeMigrated = true;

        private string notMigrateReason;

        internal MigrationObject _parentMigrationObject;

        internal bool MigrationResult { get; set; }

        internal Entity PersonalView { get; set; }

        internal string PersonalViewName { get; set; }

        internal bool WillBeMigrated
        {
            get => willBeMigrated;
            set
            {
                willBeMigrated = value;
                updateNodeUi(new NodeUpdateObject()
                {
                    ParentNodeId = _parentMigrationObject.OwnerId.ToString(),
                    NodeId = PersonalView.Id.ToString(),
                    NodeText = PersonalViewName,
                    NotMigrateReason = NotMigrateReason,
                    WillMigrate = value
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
                    ParentNodeId = _parentMigrationObject.OwnerId.ToString(),
                    NodeId = PersonalView.Id.ToString(),
                    NodeText = PersonalViewName,
                    NotMigrateReason = value,
                    WillMigrate = WillBeMigrated
                });
            }
        }

        internal List<PrincipalAccess> Sharings { get; set; } = new List<PrincipalAccess>();

        internal List<PrincipalAccess> MappedSharings { get; set; } = new List<PrincipalAccess>();

        internal PersonalViewMigrationObject(Action<NodeUpdateObject> updateNodeUiDelegate, MigrationObject parentMigrationObject, Entity personalView, string personalViewName)
        {
            updateNodeUi = updateNodeUiDelegate;
            _parentMigrationObject = parentMigrationObject;
            PersonalView = personalView;
            PersonalViewName = personalViewName;

            // ctor being called means this needs to be added to the treeview. this needs to be called after the properties have been initialized!
            updateNodeUi(new NodeUpdateObject()
            {
                ParentNodeId = _parentMigrationObject.OwnerId.ToString(),
                NodeId = PersonalView.Id.ToString(),
                NodeText = PersonalViewName,
                NotMigrateReason = NotMigrateReason,
                WillMigrate = WillBeMigrated
            });
        }
    }
}