using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Execution;
using Microsoft.Build.Evaluation;

namespace Microsoft.Build.Expressions.Internal
{
    internal interface IPropertyProvider
    {
        public abstract string Name
        {
            get;
        }

        public abstract string EvaluatedValue
        {
            get;
        }
    }

    internal class ProjectPropertyProvider : IPropertyProvider
    {
        private ProjectProperty _property;

        public ProjectPropertyProvider(ProjectProperty property)
        {
            _property = property;
        }

        public override string Name
        {
            get { return _property.Name; }
        }

        public override string EvaluatedValue
        {
            get { return _property.EvaluatedValue; }
        }
    }

    internal class ProjectPropertyInstanceProvider : IPropertyProvider
    {
        private ProjectPropertyInstance _property;

        public ProjectPropertyInstanceProvider(ProjectPropertyInstance property)
        {
            _property = property;
        }

        public override string Name
        {
            get { return _property.Name; }
        }

        public override string EvaluatedValue
        {
            get { return _property.EvaluatedValue; }
        }
    }
}
