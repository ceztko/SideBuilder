using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Build.Evaluation;
using System.IO;
using Microsoft.Build.Execution;
using System.Collections;

namespace Samples
{
    public class Auditor
    {
        public static string TempDir
        {
            get { return null; }
        }

        public static void foo(ProjectCollection collection, string path = null)
        {
            if (path == null)
                path = Path.Combine(TempDir, "debug.txt");

            PropertyInfo property = typeof(ProjectCollection).GetProperty("EnvironmentProperties", BindingFlags.Instance | BindingFlags.NonPublic);
            IEnumerable dictionary = property.GetValue(collection, new object[] { }) as IEnumerable;

            using (var writer = new StreamWriter(path))
            {
                foreach (ProjectPropertyInstance prop in dictionary)
                {
                    writer.WriteLine(prop.Name + "\t" + prop.EvaluatedValue);
                }
            }
        }

        public static void foo2(Project project, string path = null)
        {
            if (path == null)
                path = Path.Combine(TempDir, "variables.txt");

            using (var writer = new StreamWriter(path))
            {
                foreach (ResolvedImport import in project.Imports)
                    writer.WriteLine(import.ImportedProject.FullPath);
            }
        }
    }
}
