using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Execution;
using Microsoft.Build.Evaluation;

namespace Microsoft.Build.Expressions.Internal
{
    internal interface IItemProvider
    {
        public abstract string EvaluatedInclude
        {
            get;
        }

        public abstract string GetMetadataValue(string name);
    }

    internal class ProjectItemProvider : IItemProvider
    {
        private ProjectItem _item;

        public ProjectItemProvider(ProjectItem item)
        {
            _item = item;
        }

        public override string EvaluatedInclude
        {
            get { return _item.EvaluatedInclude; }
        }

        public override string GetMetadataValue(string name)
        {
            return _item.GetMetadataValue(name);
        }
    }

    internal class ProjectItemInstanceProvider : IItemProvider
    {
        private ProjectItemInstance _item;

        public ProjectItemInstanceProvider(ProjectItemInstance item)
        {
            _item = item;
        }

        public override string EvaluatedInclude
        {
            get { return _item.EvaluatedInclude; }
        }

        public override string GetMetadataValue(string name)
        {
            return _item.GetMetadataValue(name);
        }
    }
}
