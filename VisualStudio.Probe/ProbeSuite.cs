﻿// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

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
using NStackFrame = System.Diagnostics.StackFrame;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Expressions;

namespace VisualStudio.Probe
{
    [TestFixture]
    public class ProbeSuite : IVsSolutionEvents3
    {
        private string _sourceDir;
        private string _tempDir;
        private MainSite _mainSite;
        private DTE2 _DTE2;
        private DTEProject _project;
        private ProjectCollection _projectCollection;
        private VCProject _vcproject;
        private VCConfiguration _debugConfig;
        private VCConfiguration _releaseConfig;
        private IVsHierarchy _hierarchy;

        private EventHandler<ProjectPropertyChangedEventArgs> _debugPropHandler;
        private EventHandler<ProjectPropertyChangedEventArgs> _releasePropHandler;
        private EventHandler<ProjectXmlChangedEventArgs> _collectionXmlChangedHandler;
        private EventHandler<ProjectCollectionChangedEventArgs> _collectionChangedHandler;
        private EventHandler<ProjectChangedEventArgs> _projectChangedHandler;
        private EventHandler<ProjectCollection.ProjectAddedToProjectCollectionEventArgs> _projectAddedHandler;

        [TestFixtureSetUp]
        public void Setup()
        {
            _mainSite = MainSite.Instance;
            _DTE2 = _mainSite.DTE2;
            _tempDir = _mainSite.TempDir;
            _projectCollection = ProjectCollection.GlobalProjectCollection;

            _projectCollection.ProjectAdded += new ProjectCollection.ProjectAddedEventHandler(_projectCollection_ProjectAdded);
            _projectCollection.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(_projectCollection_ProjectChanged);
            _projectCollection.ProjectCollectionChanged += new EventHandler<ProjectCollectionChangedEventArgs>(_projectCollection_ProjectCollectionChanged);
            _projectCollection.ProjectXmlChanged += new EventHandler<ProjectXmlChangedEventArgs>(_projectCollection_ProjectXmlChanged);

            NStackFrame stackFrame = new NStackFrame(true);
            string filename = stackFrame.GetFileName();
            string sourcePath = Path.GetDirectoryName(filename);
            DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);
            _sourceDir = directoryInfo.Parent.FullName;

            Solution2 solution = (Solution2)_DTE2.Solution;

            IVsSolution vsSolution = _mainSite.GetService<SVsSolution>() as IVsSolution;
            int hr;
            uint pdwCookie;
            hr = vsSolution.AdviseSolutionEvents(this, out pdwCookie);
            Marshal.ThrowExceptionForHR(hr);

            // Create a new solution
            solution.Create(_tempDir, "NewSolution");

            // Create the project
            List<DTEProject> projects = TemplateHelper.AddFromTemplate(_DTE2,
                Path.Combine(_sourceDir, @"Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate"),
                Path.Combine(_tempDir, "Test"), "Test");
            solution.SaveAs(Path.Combine(_tempDir, "NewSolution.sln"));

            _project = projects.FirstOrDefault();
            _vcproject = _project.Object as VCProject;
            IVCCollection configurations = _vcproject.Configurations;
            _debugConfig = configurations.Item("Debug");
            _releaseConfig = configurations.Item("Release");

            ConfiguredProject confproject = _debugConfig.GetConfiguredProject();
            IProjectPropertiesProvider propertiesProvider = confproject.GetProjectPropertiesProvider();
            propertiesProvider.ProjectPropertyChanged += new EventHandler<ProjectPropertyChangedEventArgs>(debugPropertiesProvider_ProjectPropertyChanged);

            confproject = _releaseConfig.GetConfiguredProject();
            propertiesProvider = confproject.GetProjectPropertiesProvider();
            propertiesProvider.ProjectPropertyChanged += new EventHandler<ProjectPropertyChangedEventArgs>(releasePropertiesProvider_ProjectPropertyChanged);
        }

        [Test]
        public void EnvironmentVariables()
        {
            PropertyInfo property = typeof(ProjectCollection).GetProperty("EnvironmentVariables", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<string, ProjectPropertyInstance> dictionary = property.GetValue(_projectCollection, new object[] { }) as Dictionary<string, ProjectPropertyInstance>;
            //foreach ()
        }

        public void Test21312()
        {
            BuildProject project = null;
            ConfiguredProject confproject = _debugConfig.GetConfiguredProject();
            MSBuildProjectService service = confproject.GetProjectService();
            using (var wrapper = service.GlobalCheckout(false))
            {
                project = wrapper.Project;
            }

            string test = "!$([System.String]::IsNullOrEmpty('$(TargetFrameworkVersion)'))";
            bool test3 = new ExpressionEvaluator(project).EvaluateAsBoolean(test);
            test = "$(TargetFrameworkVersion) == $(TargetFramork) and $(TargetFrameworkVersion) == '4.0'";
            bool success;
            bool? result = new ExpressionEvaluator(project).EvaluateAsBoolean(test, out success);
        }

        [Test]
        public void Test1()
        {
            ConfiguredProject confproject = _debugConfig.GetConfiguredProject();
            MSBuildProjectService service = confproject.GetProjectService();
            using (var wrapper = service.GlobalCheckout(false))
            {
                BuildProject project = wrapper.Project;
                using(var writer = new StreamWriter(Path.Combine(_tempDir, "debug.txt")))
                {
                    foreach (ResolvedImport import in project.Imports)
                        writer.WriteLine(import.ImportedProject.FullPath);
                }

                using (var writer = new StreamWriter(Path.Combine(_tempDir, "debug-prop.txt")))
                {
                    foreach (var property in project.GlobalProperties)
                        writer.WriteLine(property.Key + " " + property.Value);
                }
            }

            confproject = _releaseConfig.GetConfiguredProject();
            service = confproject.GetProjectService();
            using (var wrapper = service.GlobalCheckout(false))
            {
                BuildProject project = wrapper.Project;
                using (var writer = new StreamWriter(Path.Combine(_tempDir, "release.txt")))
                {
                    foreach (ResolvedImport import in project.Imports)
                        writer.WriteLine(import.ImportedProject.FullPath);
                }

                using (var writer = new StreamWriter(Path.Combine(_tempDir, "release-prop.txt")))
                {
                    foreach (var property in project.GlobalProperties)
                        writer.WriteLine(property.Key + " " + property.Value);
                }
            }

            using (var writer = new StreamWriter(Path.Combine(_tempDir, "global-prop.txt")))
            {
                foreach (var property in ProjectCollection.GlobalProjectCollection.GlobalProperties)
                    writer.WriteLine(property.Key + " " + property.Value);
            }
        }

        [Test]
        public void Test2()
        {

            IVsBuildPropertyStorage storage = _hierarchy as IVsBuildPropertyStorage;
            storage.SetPropertyValue("test1", null, (uint)_PersistStorageType.PST_PROJECT_FILE, "test");
            storage.SetPropertyValue("test1", "Debug", (uint)_PersistStorageType.PST_PROJECT_FILE, "test");
            storage.SetPropertyValue("test2", "Debug|Win32", (uint)_PersistStorageType.PST_PROJECT_FILE, "test");
            storage.SetPropertyValue("test3", null, (uint)_PersistStorageType.PST_USER_FILE, "test");
            _project.Save();
        }

        [Test]
        public void Test3()
        {
            ConfiguredProject confproject = _vcproject.GetConfiguredProject();

            MSBuildProjectXmlService service = confproject.GetProjectXmlService();
            MSBuildProjectXmlContainer containerUser = service.GetProjectXml(".user");
            using (var wrapper = containerUser.GlobalCheckout(true))
            {
                ProjectRootElement rootElement = wrapper.ProjectXml;

                ProjectPropertyGroupElement group = rootElement.AddPropertyGroup();
                ProjectPropertyElement propElement = rootElement.CreatePropertyElement("test6");
                group.AppendChild(propElement);
                propElement.Value = "Ciao";
            }

            MSBuildProjectXmlContainer containerProject = service.GetProjectXml(String.Empty);
            using (var wrapper = containerProject.GlobalCheckout(true))
            {
                ProjectRootElement rootElement = wrapper.ProjectXml;

                ProjectPropertyGroupElement group = rootElement.AddPropertyGroup();
                ProjectPropertyElement propElement = rootElement.CreatePropertyElement("test6");
                group.AppendChild(propElement);
                propElement.Value = "Ciao";
            }
            _project.Save();
        }

        [Test]
        public void ConfigurationType()
        {
            _collectionXmlChangedHandler = (sender, args) =>
            {
                string propertyName = args.Reason.TrimStart("Set property Value ");
                Assert.AreEqual(propertyName, "StaticLibrary");
            };

            _debugPropHandler = (sender, args) =>
            {
                Assert.AreEqual(args.PropertyName , "ConfigurationType");
            };

            _debugConfig.ConfigurationType = ConfigurationTypes.typeStaticLibrary;
        }

        [Test]
        public void Filters()
        {
            IVCCollection filters = _vcproject.Filters;
            VCFilter filter = filters.Item("Source Files");
            Assert.AreEqual(filter.Filter, "cpp;c;cc;cxx;def;odl;idl;hpj;bat;asm;asmx");
            filter = filters.Item("Header Files");
            Assert.AreEqual(filter.Filter, "h;hpp;hxx;hm;inl;inc;xsd");
        }

        [TearDown]
        public void TearDown()
        {
            _collectionXmlChangedHandler = null;
            _collectionChangedHandler = null;
            _projectChangedHandler = null;
            _projectAddedHandler = null;
            _debugPropHandler = null;
            _releasePropHandler = null;
        }

        void _projectCollection_ProjectXmlChanged(object sender, ProjectXmlChangedEventArgs args)
        {
            EventHandler<ProjectXmlChangedEventArgs> handler = _collectionXmlChangedHandler;
            if (handler != null)
                handler(sender, args);
        }

        void _projectCollection_ProjectCollectionChanged(object sender, ProjectCollectionChangedEventArgs args)
        {
            EventHandler<ProjectCollectionChangedEventArgs> handler = _collectionChangedHandler;
            if (handler != null)
                handler(sender, args);
        }

        void _projectCollection_ProjectChanged(object sender, ProjectChangedEventArgs args)
        {
            EventHandler<ProjectChangedEventArgs> handler = _projectChangedHandler;
            if (handler != null)
                handler(sender, args);
        }

        void _projectCollection_ProjectAdded(object sender, ProjectCollection.ProjectAddedToProjectCollectionEventArgs args)
        {
            EventHandler<ProjectCollection.ProjectAddedToProjectCollectionEventArgs> handler = _projectAddedHandler;
            if (handler != null)
                handler(sender, args);
        }

        void debugPropertiesProvider_ProjectPropertyChanged(object sender, ProjectPropertyChangedEventArgs args)
        {
            EventHandler<ProjectPropertyChangedEventArgs> handler = _debugPropHandler;
            if (handler != null)
                handler(sender, args);
        }

        void releasePropertiesProvider_ProjectPropertyChanged(object sender, ProjectPropertyChangedEventArgs args)
        {
            EventHandler<ProjectPropertyChangedEventArgs> handler = _releasePropHandler;
            if (handler != null)
                handler(sender, args);
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            _hierarchy = pHierarchy;
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }
    }
}
