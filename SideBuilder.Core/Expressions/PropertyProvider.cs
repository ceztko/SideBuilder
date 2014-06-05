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
    internal interface PropertyProvider
    {
        string Name
        {
            get;
        }

        string EvaluatedValue
        {
            get;
        }
    }

    internal class MSBuildPropertyWrapper : PropertyProvider
    {
        private ProjectProperty _property;

        public MSBuildPropertyWrapper(ProjectProperty property)
        {
            _property = property;
        }

        public string Name
        {
            get { return _property.Name; }
        }

        public string EvaluatedValue
        {
            get { return _property.EvaluatedValue; }
        }
    }

    internal class MSBuildPropertyInstanceWrapper : PropertyProvider
    {
        private ProjectPropertyInstance _property;

        public MSBuildPropertyInstanceWrapper(ProjectPropertyInstance property)
        {
            _property = property;
        }

        public string Name
        {
            get { return _property.Name; }
        }

        public string EvaluatedValue
        {
            get { return _property.EvaluatedValue; }
        }
    }

    internal class PropertyProviderImpl : PropertyProvider
    {
        public PropertyProviderImpl(string name, string unevaluatedValue)
        {
            Name = name;
            UnevaluatedValue = unevaluatedValue;
        }

        public string Name
        {
            get;
            private set;
        }

        public string EvaluatedValue
        {
            get;
            internal set;
        }

        public string UnevaluatedValue
        {
            get;
            internal set;
        }
    }
}
