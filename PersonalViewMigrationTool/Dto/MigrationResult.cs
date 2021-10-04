namespace PersonalViewMigrationTool
{
    internal enum MigrationResult
    {
        /// <summary>
        /// Default value for elements where migration has not been started
        /// </summary>
        NotYetMigrated = 0,

        /// <summary>
        /// Migration of this element has completed sucessful with no errors
        /// </summary>
        Sucessful,

        /// <summary>
        /// Migration of this element failed entirely
        /// </summary>
        Failed,

        /// <summary>
        /// Migration of this element was sucessful, but some underlying elements failed to migrate
        /// </summary>
        SucessfulWithSomeErrors

    }
}