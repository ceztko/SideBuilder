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
using Microsoft.VisualStudio.Modeling.Shell;
using System.Reflection;
using NUnit.Core;
using NUnit.Util;
using System.IO;
using VisualStudio.Support;

namespace VisualStudio.Probe
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.VSPackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]       // Load if no solution
    [ProvideBindingPath]
    public sealed partial class MainSite : PackageEx
    {
        private static MainSite _Instance;
        private DTE2 _DTE2;
        private DTEEvents _DTEEvents;
        private string _TempDir;

        public MainSite()
        {
            _Instance = this;
        }

        protected override void Initialize()
        {
            base.Initialize();
            _DTE2 = GetDTE2();
            _DTEEvents = _DTE2.Events.DTEEvents;
            _DTEEvents.OnStartupComplete += new _dispDTEEvents_OnStartupCompleteEventHandler(_DTEEvents_OnStartupComplete);
        }

        void _DTEEvents_OnStartupComplete()
        {
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _TempDir = Path.Combine(userDir, "Temp", "VsProbe");

            try
            {
                Directory.Delete(_TempDir, true);
            }
            catch (DirectoryNotFoundException) { }

            Directory.CreateDirectory(_TempDir);

            CoreExtensions.Host.InitializeService();
            using (SimpleTestRunner simpleTestRunner = new SimpleTestRunner())
            {
                TestPackage package = new TestPackage(@"Probe");
                Assembly assembly = Assembly.GetExecutingAssembly();
                package.Assemblies.Add(assembly.Location);
                simpleTestRunner.Load(package);
                TestResult testResult = simpleTestRunner.Run(new NullListener(), TestFilter.Empty, false, LoggingThreshold.Off);
                XmlResultWriter writer = new XmlResultWriter(Path.Combine(_TempDir, "TestResult.xml"));
                writer.SaveTestResult(testResult);
            }
            _DTE2.Quit();
        }

        public DTE2 DTE2
        {
            get { return _DTE2; }
        }

        public string TempDir
        {
            get { return _TempDir; }
        }

        public static MainSite Instance
        {
            get { return _Instance; }
        }
    }
}
