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

            Dictionary<string, string> globalProperties = new Dictionary<string, string>();
            globalProperties.Add("DevEnvDir", "c:\\Program Files %28x86%29\\Microsoft Visual Studio 10.0\\Common7\\IDE\\");
            globalProperties.Add("SolutionDir", "C:\\Users\\ceztko\\Temp\\VsProbe\\");
            globalProperties.Add("SolutionExt", ".sln");
            globalProperties.Add("SolutionFileName", "NewSolution.sln");
            globalProperties.Add("SolutionName", "NewSolution");
            globalProperties.Add("SolutionPath", "C:\\Users\\ceztko\\Temp\\VsProbe\\NewSolution.sln");
            globalProperties.Add("LangID", "1033");
            globalProperties.Add("Platform", "Win32");
            globalProperties.Add("Configuration", "Debug");

            Project debugProject = new Project("C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj", globalProperties, null, null, collection, ProjectLoadSettings.RecordDuplicateButNotCircularImports | ProjectLoadSettings.RejectCircularImports);
            using (var writer = new StreamWriter(Path.Combine(tempDir, "debug.txt")))
            {
                foreach (ResolvedImport import in debugProject.Imports)
                    writer.WriteLine(import.ImportedProject.FullPath);
            }

            globalProperties = new Dictionary<string, string>();
            globalProperties.Add("DevEnvDir", "c:\\Program Files %28x86%29\\Microsoft Visual Studio 10.0\\Common7\\IDE\\");
            globalProperties.Add("SolutionDir", "C:\\Users\\ceztko\\Temp\\VsProbe\\");
            globalProperties.Add("SolutionExt", ".sln");
            globalProperties.Add("SolutionFileName", "NewSolution.sln");
            globalProperties.Add("SolutionName", "NewSolution");
            globalProperties.Add("SolutionPath", "C:\\Users\\ceztko\\Temp\\VsProbe\\NewSolution.sln");
            globalProperties.Add("LangID", "1033");
            globalProperties.Add("Platform", "Win32");
            globalProperties.Add("Configuration", "Release");

            Project releaseProject = new Project("C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj", globalProperties, null, null, collection, ProjectLoadSettings.RecordDuplicateButNotCircularImports | ProjectLoadSettings.RejectCircularImports);
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
