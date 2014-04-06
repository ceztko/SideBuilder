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

namespace VisualStudio.Probe
{
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
            Solution2 solution = (Solution2)_DTE2.Solution;
            // create a new solution
            solution.Create(@"C:\Temp\", "NewSolution");
            solution.AddFromTemplate(@"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp\Test", "Test");
            solution.AddFromTemplate(@"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp\Test2", "Test2");
            solution.SaveAs(@"C:\Temp\NewSolution.sln");
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
