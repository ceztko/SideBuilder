// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Expressions.Internal
{
    internal interface IPropertyItemProvider
    {
        IEnumerable<IItemProvider> GetItems(string name);

        IEnumerable<IItemProvider> AllItems
        {
            get;
        }

        IPropertyProvider GetProperty(string name);
    }

    internal class ProjectProvider : IPropertyItemProvider
    {
        private Project _project;

        public ProjectProvider(Project project)
        {
            _project = project;
        }

        public override IEnumerable<IItemProvider> GetItems(string name)
        {
            foreach (ProjectItem item in _project.GetItems(name))
                yield return new ProjectItemProvider(item);
        }

        public override IEnumerable<IItemProvider> AllItems
        {
            get
            {
                foreach (ProjectItem item in _project.AllEvaluatedItems)
                    yield return new ProjectItemProvider(item);
            }
        }

        public override IPropertyProvider GetProperty(string name)
        {
            ProjectProperty obj = _project.GetProperty(name);
            return obj == null ? null : new ProjectPropertyProvider(_project.GetProperty(name));
        }
    }

    internal class ProjectInstanceProvider : IPropertyItemProvider
    {
        private ProjectInstance _project;

        public ProjectInstanceProvider(ProjectInstance project)
        {
            _project = project;
        }

        public override IEnumerable<IItemProvider> GetItems(string name)
        {
            foreach (ProjectItemInstance item in _project.GetItems(name))
                yield return new ProjectItemInstanceProvider(item);
        }

        public override IEnumerable<IItemProvider> AllItems
        {
            get
            {
                foreach (ProjectItemInstance item in _project.Items)
                    yield return new ProjectItemInstanceProvider(item);
            }
        }

        public override IPropertyProvider GetProperty(string name)
        {
            ProjectPropertyInstance obj = _project.GetProperty(name);
            return obj == null ? null : new ProjectPropertyInstanceProvider(_project.GetProperty(name));
        }
    }
}
