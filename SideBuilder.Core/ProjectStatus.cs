// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Collections.Specialized;
using Microsoft.Build.Expressions;

namespace SideBuilder.Core
{
    public class ProjectStatus : PropertyItemProvider
    {
        private TreeSparseDictionary<string> _Properties;

        public event EventHandler<PropertyRequestedArgs> PropertyRequested;

        public ProjectStatus(TreeSparseDictionary<string> dictionary)
        {
            _Properties = dictionary;
        }

        public IEnumerable<ItemProvider> GetItems(string itemType)
        {
            yield break;
        }

        public IEnumerable<ItemProvider> AllItems
        {
            get { yield break; }
        }

        public PropertyProvider GetProperty(string name)
        {
            String value;
            bool found = _Properties.TryGetValue(name, out value);

            PropertyRequestedArgs args = new PropertyRequestedArgs()
            {
                PropertyName = name,
                Found = found
            };

            OnPropertyRequested(args);

            if (!found)
                return null;

            return new PropertyProviderImpl(name, value);
        }

        public void OnPropertyRequested(PropertyRequestedArgs args)
        {
            EventHandler<PropertyRequestedArgs> handler = PropertyRequested;
            if (handler != null)
                handler(this, args);
        }
    }

    public class PropertyRequestedArgs : EventArgs
    {
        public string PropertyName
        {
            get;
            set;
        }

        public bool Found
        {
            get;
            set;
        }
    }
}
