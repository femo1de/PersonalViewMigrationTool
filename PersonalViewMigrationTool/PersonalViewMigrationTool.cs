using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace PersonalViewMigrationTool
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    // To generate Base64 string for Images below, you can use https://www.base64-image.de/
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Personal View Migration Tool"),
        ExportMetadata("Description", "Tool to migrate personal views including shares between environments."),
        // Please specify the base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGASURBVFhH7ZYxdsIwDIZl2gP0CKylS44A12kmlmYExnTpFPaeBI6QpenKEXKBYn4FOa+mrmvTpPQ98r0njB3bUiRZDl09SlqapJmWv14wafVe5Evp/pqRtA1aU42frUuguJRpnWJ7AIqq9fNMhiwe0mwKIza9euASXNyA6BAgU3bIxJ0MB6DKap3PpfOFMzygxqTUNFhIJ7LQya20wcQkYcjRtkKA2SVc4nTXB95kROol2gBPWBkrBLAm4Ti7hJXLtE5pDYCS1ak0CXfyDAu2zYK/YPL4tAmJ43fwWt5Duk7OOAXdMhgwGNAWIhdNBqOcVkXunWfg+2JPhPJ7BIsWfJQ1qVcZajHFrFMPsHJWauQ4qsafx+xnPYVgT3oOJTOXoKhYX1bRl1EIN7iC34rcWTER1hphlV5gDkg3GH5TjwFWXv0QAlWaj9IgkbsjBq8HYrlPsyU25CTrygP9878LUSwmBHzUlNa1DFugKCU4BHe9hgA7J83pcQgrl2kDgOgAb7jUXobkYcYAAAAASUVORK5CYII="),
        // Please specify the base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAFAAAABQCAYAAACOEfKtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHFSURBVHhe7doxUsJAFMbxt57AI9CKDUeAk9hLRSOlUsbGCnpPAkdII7YcgQvI+jYsI4UjmbxNYPH/m9mwgUmAL/tYEhAAAAAAAAAAADrh4q1Zfzz1sduIbjz7nBcvcTUbN/EWDSUP0HvZ6mJVp+moK+Nm2UpfwhrMevE6qvon3I+nQ91oGfqU8D9FgEYEaESARhczieiGG51JNvu+lSvXi2ISV1p1QSPQ9cS5YZImfhB32jpK2Oiqvgc2eQ1WjECj5CNQF6UelVof4F/6WXUj7i30GYGRHpGBvotlnXYIL2eUsFGyEr4bT/8sPyf+QZe90NfRN6vuPKJHcvUxL1ZxtZFzlHCyAE/pPz4t99/RRNbzopXnZRbOEAEaEaARARoRoBEBGhGgEQEaEaBRlmci4TriTqTa1zHd6fO+5zde3Pu+/7tUv0FnGWA47/4Jq5lUp5OUsFH2Ae7ET3Qojeo038J/cXS/3WirhEMwdS+DtXFFiBI2OssITIkRmLkOA3RluFKcpIW/gVyIzko4JSaRK0KARgRoRIBG2U8i4fTMeb+tHjjBixs4J7ehzyQSaQqDamat0Q7hpUQJAwAAAAAAAACQAZFv2yEaWy1qN08AAAAASUVORK5CYII="),
        ExportMetadata("BackgroundColor", "Lavender"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class PersonalViewMigrationTool : PluginBase
    {

        public override IXrmToolBoxPluginControl GetControl()
        {
            return new PersonalViewMigrationToolControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public PersonalViewMigrationTool()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}