// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;

namespace VisualStudio.Probe
{
    class ProbeTemplateWizard : IWizard
    {
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
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
    }
}
