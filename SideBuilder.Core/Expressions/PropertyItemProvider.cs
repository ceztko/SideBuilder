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
        IEnumerable<ItemProvider> GetItems(string itemType);

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

        public IEnumerable<ItemProvider> GetItems(string itemType)
        {
            foreach (ProjectItem item in _project.GetItems(itemType))
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

        public IEnumerable<ItemProvider> GetItems(string itemType)
        {
            foreach (ProjectItemInstance item in _project.GetItems(itemType))
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
        private Dictionary<string, List<ItemProviderImpl>> _items;
        private Dictionary<string, PropertyProviderImpl> _properties;

        public PropertyItemProviderImpl()
        {
            _properties = new Dictionary<string, PropertyProviderImpl>();
            _items = new Dictionary<string, List<ItemProviderImpl>>();
        }

        public PropertyProviderImpl SetProperty(string name, string unevaluatedValue)
        {
            PropertyProviderImpl ret;
            bool found = _properties.TryGetValue(name, out ret);
            if (!found)
                ret = new PropertyProviderImpl(name, unevaluatedValue);

            ret.UnevaluatedValue = unevaluatedValue;
            return ret;
        }

        public bool RemoveProperty(PropertyProviderImpl property)
        {
            return _properties.Remove(property.Name);
        }

        public ItemProviderImpl AddItem(string itemtype, string unevaluatedInclude)
        {
            List<ItemProviderImpl> items;
            bool found = _items.TryGetValue(itemtype, out items);
            if (!found)
                items = new List<ItemProviderImpl>();

            ItemProviderImpl newitem = new ItemProviderImpl(unevaluatedInclude);
            newitem.ItemType = itemtype;
            items.Add(newitem);

            if (!found)
                _items.Add(itemtype, items);
            
            return newitem;
        }

        public IEnumerable<ItemProvider> GetItems(string itemType)
        {
            List<ItemProviderImpl> items;
            bool found = _items.TryGetValue(itemType, out items);
            if (!found)
                yield break;

            foreach (ItemProviderImpl item in items)
                yield return item;
        }

        public IEnumerable<ItemProvider> AllItems
        {
            get
            {
                foreach (List<ItemProviderImpl> list in _items.Values)
                {
                    foreach (ItemProviderImpl item in list)
                        yield return item;
                }
            }
        }

        public PropertyProvider GetProperty(string name)
        {
            PropertyProviderImpl ret;
            bool found = _properties.TryGetValue(name, out ret);
            if (!found)
                return null;

            return ret;
        }
    }
}
