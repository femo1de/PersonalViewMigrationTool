using System;
using System.Collections.Generic;

namespace PersonalViewMigrationTool
{
    internal class MigrationObject
    {
        internal Guid OwnerId { get; set; }

        internal Guid MappedOwnerId { get; set; }

        internal List<PersonalViewMigrationObject> PersonalViewsMigrationObjects { get; set; } = new List<PersonalViewMigrationObject>();
    }
}