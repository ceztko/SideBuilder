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

namespace VisualStudio.Probe
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.VSPackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]       // Load if no solution
    [ProvideBindingPath]
    public sealed partial class MainSite : Package
    {
        private static MainSite _Instance;
        private DTE2 _DTE2;
        private DTEEvents _DTEEvents;

        public MainSite()
        {
            _Instance = this;
        }

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
            CoreExtensions.Host.InitializeService();
            SimpleTestRunner remoteTestRunner = new SimpleTestRunner();
            TestPackage package = new TestPackage(@"Probe");
            string loc = Assembly.GetExecutingAssembly().Location;
            package.Assemblies.Add(loc);
            remoteTestRunner.Load(package);
            TestResult testResult = remoteTestRunner.Run(new NullListener(), TestFilter.Empty, false, LoggingThreshold.Off);
            XmlResultWriter writer = new XmlResultWriter(@"C:\Temp\TestResult.xml");
            writer.SaveTestResult(testResult);
            _DTE2.Quit();
        }

        public void GetService<T>(out T service)
        {
            service = (T)GetService(typeof(T));
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public DTE2 DTE2
        {
            get { return _DTE2; }
        }

        public static MainSite Instance
        {
            get { return _Instance; }
        }
    }
}
