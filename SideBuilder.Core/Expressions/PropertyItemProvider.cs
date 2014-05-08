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
    internal interface PropertyItemProvider
    {
        IEnumerable<ItemProvider> GetItems(string name);

        IEnumerable<ItemProvider> AllItems
        {
            get;
        }

        PropertyProvider GetProperty(string name);
    }

    internal class ProjectProvider : PropertyItemProvider
    {
        private Project _project;

        public ProjectProvider(Project project)
        {
            _project = project;
        }

        public IEnumerable<ItemProvider> GetItems(string name)
        {
            foreach (ProjectItem item in _project.GetItems(name))
                yield return new ProjectItemProvider(item);
        }

        public IEnumerable<ItemProvider> AllItems
        {
            get
            {
                foreach (ProjectItem item in _project.AllEvaluatedItems)
                    yield return new ProjectItemProvider(item);
            }
        }

        public PropertyProvider GetProperty(string name)
        {
            ProjectProperty obj = _project.GetProperty(name);
            return obj == null ? null : new ProjectPropertyProvider(_project.GetProperty(name));
        }
    }

    internal class ProjectInstanceProvider : PropertyItemProvider
    {
        private ProjectInstance _project;

        public ProjectInstanceProvider(ProjectInstance project)
        {
            _project = project;
        }

        public IEnumerable<ItemProvider> GetItems(string name)
        {
            foreach (ProjectItemInstance item in _project.GetItems(name))
                yield return new ProjectItemInstanceProvider(item);
        }

        public IEnumerable<ItemProvider> AllItems
        {
            get
            {
                foreach (ProjectItemInstance item in _project.Items)
                    yield return new ProjectItemInstanceProvider(item);
            }
        }

        public PropertyProvider GetProperty(string name)
        {
            ProjectPropertyInstance obj = _project.GetProperty(name);
            return obj == null ? null : new ProjectPropertyInstanceProvider(_project.GetProperty(name));
        }
    }

    internal class PropertyItemProviderImpl : PropertyItemProvider
    {
        private Dictionary<string, PropertyProviderImpl> _properties;

        public PropertyItemProviderImpl()
        {
            _properties = new Dictionary<string, PropertyProviderImpl>();
        }

        public void AddProperty(PropertyProviderImpl property)
        {
            _properties.Add(property.Name, property);
        }

        public void SetProperty(PropertyProviderImpl property)
        {
            _properties[property.Name] = property;
        }

        public void RemoveProperty(string name)
        {
            _properties.Remove(name);
        }

        public IEnumerable<ItemProvider> GetItems(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ItemProvider> AllItems
        {
            get { throw new NotImplementedException(); }
        }

        public PropertyProvider GetProperty(string name)
        {
            throw new NotImplementedException();
        }
    }
}
