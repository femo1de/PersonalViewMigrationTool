namespace PersonalViewMigrationTool.Dto
{
    // This object is on ly used to transfer data to update the UI, it is not meant to hold any data on itself
    internal class NodeUpdateObject
    {
        internal string NodeId { get; set; }
        internal string ParentNodeId { get; set ; }
        internal bool WillMigrate { get ; set; }
        internal string NotMigrateReason { get ; set ; }
        internal string NodeText { get ; set ; }
        internal bool Migrated { get; set; } = false;
    }
}