using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using System.Reflection;
using BuildProject = Microsoft.Build.Evaluation.Project;
using DTEProject = EnvDTE.Project;
using System.IO;
using Microsoft.VisualStudio.VCProjectEngine;
using VisualStudio.Support;
using Microsoft.VisualStudio.Project.Framework.INTERNAL.VS2010ONLY;
using Microsoft.VisualStudio.Project.Contracts.INTERNAL.VS2010ONLY;
using NUnit.Core;
using NUnit.Framework;

namespace VisualStudio.Probe
{
    //extern alias VC;
    //using VC::Microsoft.VisualStudio.Project.VisualC.VCProjectEngine;

    [TestFixture]
    public class ProbeSuite
    {
        private MainSite _MainSite;
        private DTE2 _DTE2;
        private DTEProject _Project;

        [TestFixtureSetUp]
        public void Setup()
        {
            _MainSite = MainSite.Instance;
            _DTE2 = _MainSite.DTE2;

            ProjectCollection.GlobalProjectCollection.ProjectAdded += new ProjectCollection.ProjectAddedEventHandler(GlobalProjectCollection_ProjectAdded);
            ProjectCollection.GlobalProjectCollection.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(GlobalProjectCollection_ProjectChanged);
            ProjectCollection.GlobalProjectCollection.ProjectCollectionChanged += new EventHandler<ProjectCollectionChangedEventArgs>(GlobalProjectCollection_ProjectCollectionChanged);
            ProjectCollection.GlobalProjectCollection.ProjectXmlChanged += new EventHandler<ProjectXmlChangedEventArgs>(GlobalProjectCollection_ProjectXmlChanged);

            Directory.Delete(@"C:\Temp", true);
            Directory.CreateDirectory(@"C:\Temp");

            Solution2 solution = (Solution2)_DTE2.Solution;

            // Create a new solution
            solution.Create(@"C:\Temp\", "NewSolution");

            // Create the project
            List<DTEProject> projects = ProbeTemplate.AddFromTemplate(_DTE2, @"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp\Test", "Test");
            solution.SaveAs(@"C:\Temp\NewSolution.sln");

            _Project = projects.FirstOrDefault();
        }

        [Test]
        public void DoWork()
        {
            VCProject vcproject = _Project.Object as VCProject;
            IVCCollection configurations = vcproject.Configurations;
            VCConfiguration configuration = configurations.Item("Debug");

            ConfiguredProject confproject = configuration.GetConfiguredProject();
            IProjectPropertiesProvider propProvider = confproject.GetProjectPropertiesProvider();
            propProvider.ProjectPropertyChanged += new EventHandler<ProjectPropertyChangedEventArgs>(propProvider_ProjectPropertyChanged);

            configuration.ConfigurationType = ConfigurationTypes.typeStaticLibrary;
        }


        void propProvider_ProjectPropertyChanged(object sender, ProjectPropertyChangedEventArgs e)
        {

        }

        void GlobalProjectCollection_ProjectXmlChanged(object sender, ProjectXmlChangedEventArgs e)
        {

        }

        void GlobalProjectCollection_ProjectCollectionChanged(object sender, ProjectCollectionChangedEventArgs e)
        {

        }

        void GlobalProjectCollection_ProjectChanged(object sender, ProjectChangedEventArgs e)
        {

        }

        void GlobalProjectCollection_ProjectAdded(object sender, ProjectCollection.ProjectAddedToProjectCollectionEventArgs e)
        {

        }
    }
}
