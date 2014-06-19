// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using Microsoft.VisualStudio.Editor;

namespace VisualStudio.Support
{
    public class PackageEx : Package
    {
        public void GetService<T>(out T service)
        {
            service = (T)GetService(typeof(T));
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public static void GetGlobalService<T>(out T service)
        {
            service = (T)GetGlobalService(typeof(T));
        }

        public static T GetGlobalService<T>()
        {
            return (T)GetGlobalService(typeof(T));
        }

        /// <summary>
        /// Get MEF components
        /// </summary>
        /// <returns>The MEF component of type T</returns>
        public T GetComponentModelService<T>()
            where T : class
        {
            IComponentModel componentModel = GetService<SComponentModel>() as IComponentModel;
            return componentModel.GetService<T>();
        }

        /// <summary>
        /// Get MEF components
        /// </summary>
        /// <param name="service">The MEF component of type T</param>
        public void GetComponentModelService<T>(out T service)
            where T : class
        {
            IComponentModel componentModel = GetService<SComponentModel>() as IComponentModel;
            service = componentModel.GetService<T>();
        }

        public IVsRegisterPriorityCommandTarget GetVsRegisterPriorityCommandTarget()
        {
            return GetService<SVsRegisterPriorityCommandTarget>() as IVsRegisterPriorityCommandTarget;
        }

        public IVsExtensibility GetVsExtensibility()
        {
            return GetService<IVsExtensibility>() as IVsExtensibility;
        }

        public IVsMonitorSelection GetVsMonitorSelection()
        {
            return GetService<SVsShellMonitorSelection>() as IVsMonitorSelection;
        }

        public IVsSolutionBuildManager3 GetVsSolutionBuildManager()
        {
            return GetService<SVsSolutionBuildManager>() as IVsSolutionBuildManager3;
        }

        public IVsEditorAdaptersFactoryService GetVsEditorAdaptersFactoryService()
        {
            return GetComponentModelService<IVsEditorAdaptersFactoryService>();
        }

        public IVsSolution GetVsSolution()
        {
            return GetService<SVsSolution>() as IVsSolution;
        }

        public IVsTrackProjectDocuments2 GetVsTrackProjectDocuments()
        {
            return GetService<SVsTrackProjectDocuments>() as IVsTrackProjectDocuments2;
        }

        public IVsRegisterScciProvider GetVsRegisterScciProvider()
        {
            return GetService<IVsRegisterScciProvider>();
        }

        public IVsShell GetVsShell()
        {
            return GetService<SVsShell>() as IVsShell;
        }
    }
}
