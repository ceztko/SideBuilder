// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using EnvDTE80;
using System.Threading;

namespace VisualStudio.Probe
{
    class ProbeTemplate : IWizard
    {
        private static object _Lock;
        private static TemplateHandle _Current;

        private List<Project> _Projects;

        static ProbeTemplate()
        {
            _Lock = new object();
            _Current = null;
        }

        public ProbeTemplate()
        {
            _Projects = new List<Project>();

            TemplateHandle handle;
            lock (_Lock)
            {
                handle = _Current;
                if (handle != null)
                {
                    // AddFromTemplate has been called
                    handle.Template = this;
                    _Current = null;
                }
            }

            if (handle != null)
            {
                // Release the first lock set on AddFromTemplate
                Monitor.Exit(_Lock);
            }
        }

        public static List<Project> AddFromTemplate(DTE2 dte, string filename, string destination, string projectname)
        {
            TemplateHandle current = new TemplateHandle();
            Monitor.Enter(_Lock);
            try
            {
                Solution2 solution = dte.Solution as Solution2;
                _Current = current;
                solution.AddFromTemplate(filename, destination, projectname);
                if (current.Template != null)
                    return current.Template.Projects;
                
            }
            finally
            {
                _Current = null;
                if (current.Template == null)
                {
                    // The lock wasn't released elsewhere
                    Monitor.Exit(_Lock);
                }
            }

            return null;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
            _Projects.Add(project);
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            string safeprojectname = replacementsDictionary["$safeprojectname$"];
            replacementsDictionary["$safeprjnameupcase$"] = safeprojectname.ToUpper();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public List<Project> Projects
        {
            get { return _Projects; }
        }
    }

    internal class TemplateHandle
    {
        public ProbeTemplate Template
        {
            get;
            set;
        }
    }
}
