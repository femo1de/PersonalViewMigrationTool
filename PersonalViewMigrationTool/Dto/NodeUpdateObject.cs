namespace PersonalViewMigrationTool.Dto
{
    // This object is on ly used to transfer data to update the UI, it is not meant to hold any data on itself
    internal class NodeUpdateObject
    {
        internal UpdateReason UpdateReason { get; set; }
        internal string NodeId { get { return MigrationObjectBase.ElementId; } }
        internal string ParentNodeId { get; set ; }
        internal bool WillMigrate { get ; set; }
        internal bool CanBeMigrated { get; set; }
        internal string NotMigrateReason { get ; set ; }
        internal string NodeText { get ; set ; }
        internal MigrationObjectBase MigrationObjectBase { get; set; }
    }
}

namespace PersonalViewMigrationTool
{
    internal enum UpdateReason
    {
        AddedToList,
        RemovedFromList,
        DetailsAdded,
        MigrationFailed,
        MigrationSucceeded,
        MigrationSucessfulWithErrors
    }
}