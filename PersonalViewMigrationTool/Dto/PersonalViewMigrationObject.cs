using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace PersonalViewMigrationTool.Dto
{
    internal class PersonalViewMigrationObject
    {
        internal Entity PersonalView { get; set; }

        internal List<PrincipalAccess> Sharings { get; set; } = new List<PrincipalAccess>();

        internal List<PrincipalAccess> MappedSharings { get; set; } = new List<PrincipalAccess>();
    }
}