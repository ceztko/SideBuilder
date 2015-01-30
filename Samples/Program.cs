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
using Microsoft.Build.Execution;
using Collections.Specialized;
using Microsoft.Build.Expressions;
using MSBuild.Support;

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
            string tempDir = @"C:\Users\ceztko\Documents\Temp";
            ProjectCollection collection = ProjectCollection.GlobalProjectCollection;


            Dictionary<string, string> globalProperties = BuilderCore.GetGlobalProperties(
                "NewSolution", "C:\\Users\\ceztko\\Temp\\VsProbe\\", "NewSolution.sln",
                "C:\\Users\\ceztko\\Temp\\VsProbe\\NewSolution.sln", ".sln",
                "Debug", "Win32", "1033");


            Project test5 = new Project(collection);

            TreeSparseDictionary<string> dictionary = new TreeSparseDictionary<string>();

            foreach (ProjectProperty property in test5.Properties)
            {
                dictionary[property.Name] = property.EvaluatedValue;
            }

            Project debugProject = BuilderCore.LoadProject(@"C:\Users\ceztko\Temp\VsProbe\Test\Test.vcxproj",
                collection, globalProperties);

            StreamWriter writer2 = new StreamWriter(@"C:\Users\ceztko\Temp\logica.txt");
            debugProject.SaveLogicalProject(writer2);

            Test(debugProject, dictionary);

            HashSet<string> test = new HashSet<string>();
            foreach (ProjectItem item in debugProject.ItemsIgnoringCondition)
            {
                test.Add(item.ItemType);
            }

            BuilderCore.Process(debugProject);

            /*
             * 1) Si filla un ProjectStatus di property (tipo sicuramente NO CLR o altre tautologie)
             * 2) Si unrolla Eagerly il Project/ProjectRootElement, creando branch ogni volta che ci sono delle condizioni
             * 3) Si ritorna sulle condizioni e si cerca di entrare dove non si era entrati prima, aprendo altri status.
             * 4) Si ritorna sugli stati di 2) e si tenta di capire se quanto fatto in 3) possa aprire altre strade 
             */

            /*
             * 1) Si filla un ProjectStatus di property (tipo sicuramente NO CLR o altre tautologie)
             * 2) Si unrolla Eagerly il Project/ProjectRootElement, creando branch ogni volta che si deve entrare in un import
             * condizionale. Si CHIUDE il branch quando si esce dall'import.
             * 
             */

            globalProperties = BuilderCore.GetGlobalProperties(
                "NewSolution", "C:\\Users\\ceztko\\Temp\\VsProbe\\", "NewSolution.sln",
                "C:\\Users\\ceztko\\Temp\\VsProbe\\NewSolution.sln", ".sln", "Release",
                "Win32", "1033");

            Project releaseProject = BuilderCore.LoadProject("C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj",
                collection, globalProperties);

            collection.SetGlobalProperty("VSIDEResolvedNonMSBuildProjectOutputs", "<VSIDEResolvedNonMSBuildProjectOutputs />");
            collection.SetGlobalProperty("CurrentSolutionConfigurationContents",
                "<SolutionConfiguration>\n<ProjectConfiguration Project=\"{e312acae-f92e-4ce9-8ce5-a9911bba621f}\" AbsolutePath=\"C:\\Users\\ceztko\\Temp\\VsProbe\\Test\\Test.vcxproj\">Debug|Win32</ProjectConfiguration>\n</SolutionConfiguration>");
        }

        public static void Test(Project project, TreeSparseDictionary<string> dictionary)
        {
            List<ExpressionList> list = new List<ExpressionList>();
            List<bool?> results = new List<bool?>();

            ProjectStatus dictWrapper = new ProjectStatus(dictionary);

            foreach (ProjectElement element in project.Xml.Iterate(dictWrapper))
            {
                foo(element, dictionary);

                if (!String.IsNullOrEmpty(element.Condition))
                {
                    ExpressionEvaluator evaluator = new ExpressionEvaluator(dictWrapper);

                    bool success2;
                    bool? value = evaluator.EvaluateAsBoolean(element.Condition, out success2);
                    results.Add(value);

                    ExpressionList exps = new ExpressionParser().Parse(element.Condition, ExpressionValidationType.StrictBoolean);
                    list.Add(exps);
                }
            }
        }

        public static void foo(ProjectElement element, TreeSparseDictionary<string> dictionary)
        {
            switch (element.GetElementType())
            {

                case ElementType.ItemGroupElement:
                {
                    ProjectItemGroupElement itemGroup = element as ProjectItemGroupElement;
                    break;
                }
                case ElementType.PropertyGroupElement:
                {
                    ProjectPropertyGroupElement propertyGroup = element as ProjectPropertyGroupElement;
                    foreach (ProjectPropertyElement property in propertyGroup.Properties)
                    {
                        dictionary[property.Name] = property.Value;
                    }
                    break;
                }
                case ElementType.ItemDefinitionGroupElement:
                {
                    ProjectItemDefinitionGroupElement itemDefinitionGroup = element as ProjectItemDefinitionGroupElement;

                    break;
                }
            }
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
