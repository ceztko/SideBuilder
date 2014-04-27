// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Execution;
using Microsoft.Build.Evaluation;

namespace Microsoft.Build.Expressions.Internal
{
    internal interface ItemProvider
    {
        string EvaluatedInclude
        {
            get;
        }

        string GetMetadataValue(string name);
    }

    internal class ProjectItemProvider : ItemProvider
    {
        private ProjectItem _item;

        public ProjectItemProvider(ProjectItem item)
        {
            _item = item;
        }

        public string EvaluatedInclude
        {
            get { return _item.EvaluatedInclude; }
        }

        public string GetMetadataValue(string name)
        {
            return _item.GetMetadataValue(name);
        }
    }

    internal class ProjectItemInstanceProvider : ItemProvider
    {
        private ProjectItemInstance _item;

        public ProjectItemInstanceProvider(ProjectItemInstance item)
        {
            _item = item;
        }

        public string EvaluatedInclude
        {
            get { return _item.EvaluatedInclude; }
        }

        public string GetMetadataValue(string name)
        {
            return _item.GetMetadataValue(name);
        }
    }

    internal class ItemProviderImpl : ItemProvider
    {
        public string EvaluatedInclude
        {
            get;
            set;
        }

        public string GetMetadataValue(string name)
        {
            throw new NotImplementedException();
        }
    }
}
