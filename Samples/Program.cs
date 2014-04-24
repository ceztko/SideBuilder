using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using System.Reflection;
using System.Diagnostics;
using Support.VisualStudio;
using Microsoft.Build.Evaluation;
using System.IO;
using Microsoft.Build.Construction;
using SideBuilder.Core;

namespace VisualStudio.Probe
{
    class Program
    {
        private static DTE2 _dte;

        static void Main(string[] args)
        {
            Sample1();
            //Sample2();
        }

        private static void Sample1()
        {
            string tempDir = @"C:\Users\ceztko\Temp";
            ProjectCollection collection = ProjectCollection.GlobalProjectCollection;

            Dictionary<string, string> globalProperties = BuilderCore.GetGlobalProperties(
                "NewSolution", "C:\\Users\\ceztko\\Temp\\VsProbe\\", "NewSolution.sln",
                "C:\\Users\\ceztko\\Temp\\VsProbe\\NewSolution.sln", ".sln",
                "Debug", "Win32", "1033");

            Project debugProject = BuilderCore.LoadProject("C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj",
                collection, globalProperties);

            HashSet<string> test = new HashSet<string>();
            foreach (ProjectItem item in debugProject.ItemsIgnoringCondition)
            {
                test.Add(item.ItemType);
            }

            BuilderCore.Process(debugProject);

            using (var writer = new StreamWriter(Path.Combine(tempDir, "debug.txt")))
            {
                foreach (ResolvedImport import in debugProject.Imports)
                    writer.WriteLine(import.ImportedProject.FullPath);
            }

            globalProperties = BuilderCore.GetGlobalProperties(
                "NewSolution", "C:\\Users\\ceztko\\Temp\\VsProbe\\", "NewSolution.sln",
                "C:\\Users\\ceztko\\Temp\\VsProbe\\NewSolution.sln", ".sln", "Release",
                "Win32", "1033");

            Project releaseProject = BuilderCore.LoadProject("C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj",
                collection, globalProperties);
            using (var writer = new StreamWriter(Path.Combine(tempDir, "release.txt")))
            {
                foreach (ResolvedImport import in releaseProject.Imports)
                    writer.WriteLine(import.ImportedProject.FullPath);
            }

            collection.SetGlobalProperty("VSIDEResolvedNonMSBuildProjectOutputs", "<VSIDEResolvedNonMSBuildProjectOutputs />");
            collection.SetGlobalProperty("CurrentSolutionConfigurationContents",
                "<SolutionConfiguration>\n<ProjectConfiguration Project=\"{e312acae-f92e-4ce9-8ce5-a9911bba621f}\" AbsolutePath=\"C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj\">Debug|Win32</ProjectConfiguration>\n</SolutionConfiguration>");
        }

        private static void Sample2()
        {
            DTEWrapper wrapper = DTEInstanceManager.GetExternalDTE(DTEVersion.VS10, "/rootsuffix Exp");
            _dte = wrapper.DTE;
            GC.KeepAlive(wrapper);

#if VISIBLE
            MakeVisible();
#endif
            DoWork();
        }

        private static void DoWork()
        {
            Solution2 solution = (Solution2)_dte.Solution;
            // create a new solution
            solution.Create(@"C:\Temp\", "NewSolution");
            solution.AddFromTemplate(@"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32App\MyTemplate.vstemplate", @"C:\Temp", "Test");
            solution.SaveAs(@"C:\Temp\NewSolution.sln");
        }

        private static void MakeVisible()
        {
            // Display the Visual Studio IDE.
            _dte.MainWindow.Activate();
            _dte.MainWindow.Visible = true;
        }
    }
}
