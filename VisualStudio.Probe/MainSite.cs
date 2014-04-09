// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using System.Reflection;
using BuildProject = Microsoft.Build.Evaluation.Project;
using DTEProject = EnvDTE.Project;
using System.IO;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.VisualStudio.Modeling.Shell;
using VisualStudio.Support;
using Microsoft.VisualStudio.Project.Framework.INTERNAL.VS2010ONLY;
using Microsoft.VisualStudio.Project.Contracts.INTERNAL.VS2010ONLY;

namespace VisualStudio.Probe
{
    //extern alias VC;
    //using VC::Microsoft.VisualStudio.Project.VisualC.VCProjectEngine;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.VSPackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]       // Load if no solution
    [ProvideBindingPath]
    public sealed partial class MainSite : Package
    {
        private static DTE2 _DTE2;
        private static DTEEvents _DTEEvents;

        public MainSite() { }

        protected override void Initialize()
        {
            base.Initialize();
            IVsExtensibility extensibility = GetService<IVsExtensibility>();
            _DTE2 = (DTE2)extensibility.GetGlobalsObject(null).DTE;
            _DTEEvents = _DTE2.Events.DTEEvents;
            _DTEEvents.OnStartupComplete += new _dispDTEEvents_OnStartupCompleteEventHandler(_DTEEvents_OnStartupComplete);
        }

        void _DTEEvents_OnStartupComplete()
        {
            DoWork();
        }

        private void DoWork()
        {
            ProjectCollection.GlobalProjectCollection.ProjectAdded += new ProjectCollection.ProjectAddedEventHandler(GlobalProjectCollection_ProjectAdded);
            ProjectCollection.GlobalProjectCollection.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(GlobalProjectCollection_ProjectChanged);
            ProjectCollection.GlobalProjectCollection.ProjectCollectionChanged += new EventHandler<ProjectCollectionChangedEventArgs>(GlobalProjectCollection_ProjectCollectionChanged);
            ProjectCollection.GlobalProjectCollection.ProjectXmlChanged += new EventHandler<ProjectXmlChangedEventArgs>(GlobalProjectCollection_ProjectXmlChanged);

            Directory.Delete(@"C:\Temp", true);
            Directory.CreateDirectory(@"C:\Temp");

            Solution2 solution = (Solution2)_DTE2.Solution;
            // create a new solution
            solution.Create(@"C:\Temp\", "NewSolution");

            List<DTEProject> projects = ProbeTemplate.AddFromTemplate(_DTE2, @"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp\Test", "Test");
            //DTEProject project2 = solution.AddFromTemplate(@"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp\Test2", "Test2");

            solution.SaveAs(@"C:\Temp\NewSolution.sln");

            DTEProject project = projects.FirstOrDefault();
            VCProject vcproject = project.Object as VCProject;

            IVCCollection configurations = vcproject.Configurations;

            VCConfiguration configuration = configurations.Item("Debug");

            ConfiguredProject confproject = configuration.GetConfiguredProject();
            IProjectPropertiesProvider propProvider = confproject.GetProjectPropertiesProvider();
            propProvider.ProjectPropertyChanged += new EventHandler<ProjectPropertyChangedEventArgs>(propProvider_ProjectPropertyChanged);

            configuration.ConfigurationType = ConfigurationTypes.typeStaticLibrary;

            //solution.AddFromTemplate(@"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp\Test2", "Test2");

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

        private void GetService<T>(out T service)
        {
            service = (T)GetService(typeof(T));
        }

        private T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public static DTE2 DTE2
        {
            get { return _DTE2; }
        }
    }
}
