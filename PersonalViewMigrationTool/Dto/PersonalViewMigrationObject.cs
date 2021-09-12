using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        internal ObservableCollection<PrincipalAccess> Sharings { get; set; } = new ObservableCollection<PrincipalAccess>();

        internal ObservableCollection<PrincipalAccess> MappedSharings { get; set; } = new ObservableCollection<PrincipalAccess>();

        internal PersonalViewMigrationObject(Action<NodeUpdateObject> updateNodeUiDelegate, MigrationObject parentMigrationObject, Entity personalView, string personalViewName, bool willBeMigrated = false)
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
            WillBeMigrated = willBeMigrated;

            MappedSharings.CollectionChanged += MappedSharings_CollectionChanged;
        }

        private void MappedSharings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // removing PoA from the list is not supported currently
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    var newPoA = (PrincipalAccess)newItem;

                    updateNodeUi(new NodeUpdateObject()
                    {
                        ParentNodeId = PersonalView.Id.ToString(),
                        NodeId = $"{PersonalView.Id}_{newPoA.Principal.Id}",
                        NodeText = $"{newPoA.Principal.LogicalName}: {newPoA.Principal.Id}",
                        NotMigrateReason = NotMigrateReason,
                        WillMigrate = WillBeMigrated
                    });
                }     
            }
        }
    }
}