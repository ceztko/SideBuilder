// Copyright (c) 2014 Francesco Pretto. Subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem;
using SolutionConfigurationName;
using BuildProject = Microsoft.Build.Evaluation.Project;
using DTEProject = EnvDTE.Project;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.VisualStudio.Shell.Interop;
using VisualStudio.Support;

namespace SideBuilder
{
    public partial class MainSite
    {
        private static AsyncLock _lock;

        public void foo()
        {
            _lock = new AsyncLock();
        }

        public static void Foo()
        {

        }

        static bool done = false;

        public static async void EnsureVCProjectsPropertiesConfigured(IVsHierarchy hiearchy)
        {
            DTEProject project = hiearchy.GetProject();
            if (project == null || !(project.Object is VCProject))
                return;

            using (await _lock.LockAsync())
            {

                if (done)
                    return;

                // Inspired from Nuget: https://github.com/Haacked/NuGet/blob/master/src/VisualStudio12/ProjectHelper.cs
                IVsBrowseObjectContext context = project.Object as IVsBrowseObjectContext;
                UnconfiguredProject unconfiguredProject = context.UnconfiguredProject;
                IProjectLockService service = unconfiguredProject.ProjectService.Services.ProjectLockService;

                ProjectCollection collection = null;

                using (ProjectWriteLockReleaser releaser = await service.WriteLockAsync())
                {
                    collection = releaser.ProjectCollection;

                    // The following was present in the NuGet code: it seesms unecessary,
                    // as the lock it's release anyway after the using block (check
                    // service.IsAnyLockHeld). Also it seemed to cause a deadlock sometimes
                    // when switching solution configuration
                    //await releaser.ReleaseAsync();
                }

                collection.ProjectChanged += collection_ProjectChanged;
                collection.ProjectCollectionChanged += collection_ProjectCollectionChanged;
                collection.ProjectXmlChanged += collection_ProjectXmlChanged;
                done = true;
            }
        }


        static void collection_ProjectXmlChanged(object sender, ProjectXmlChangedEventArgs e)
        {

        }

        static void collection_ProjectCollectionChanged(object sender, ProjectCollectionChangedEventArgs e)
        {

        }

        static void collection_ProjectChanged(object sender, ProjectChangedEventArgs e)
        {

        }
    }
}
