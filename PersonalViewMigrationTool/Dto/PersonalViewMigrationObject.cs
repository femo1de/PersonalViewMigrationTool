using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PersonalViewMigrationTool.Dto
{
    /// <summary>
    /// Represents a personal view and holds a list users / teams this view has been shared with
    /// </summary>
    internal class PersonalViewMigrationObject : MigrationObjectBase
    {
        internal MigrationObject _parentMigrationObject;

        internal Entity PersonalView { get; set; }

        internal string PersonalViewName { get; set; }

        internal ObservableCollection<PrincipalAccess> Sharings { get; set; } = new ObservableCollection<PrincipalAccess>();

        internal ObservableCollection<SharingMigrationObject> MappedSharings { get; set; } = new ObservableCollection<SharingMigrationObject>();

        // override ChildObjects to point to the sharings that sit below this view object
        internal override IEnumerable<MigrationObjectBase> ChildObjects => MappedSharings;

        #region ctor

        internal PersonalViewMigrationObject(Action<NodeUpdateObject> updateNodeUiDelegate,
                                             MigrationObject parentMigrationObject,
                                             Entity personalView,
                                             string personalViewName,
                                             bool willBeMigrated = false,
                                             bool canBeMigrated = false)
            : base(updateNodeUiDelegate)
        {
            _parentMigrationObject = parentMigrationObject;
            PersonalView = personalView;
            PersonalViewName = personalViewName;
            ElementId = PersonalView.Id.ToString();

            // ctor being called means this needs to be added to the treeview. this needs to be called after the properties have been initialized!
            updateNodeUiDelegate(new NodeUpdateObject()
            {
                MigrationObjectBase = this,
                ParentNodeId = _parentMigrationObject.SourceOwnerId.ToString(),
                NodeText = PersonalViewName,
                UpdateReason= UpdateReason.AddedToList
            });

            // this needs to happen after the node object is created!
            WillBeMigrated = willBeMigrated;
            CanBeMigrated = canBeMigrated;
        }

        #endregion

        private void MappedSharings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // removing PoA from the list is not supported currently
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // not implemented yet. Pressing the load sharings button twice will result in duplicate values
            }
        }
    }
}