using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Expressions.Internal;
using Microsoft.Build.Construction;

namespace SideBuilder.Core
{
    public abstract class BuildConfigWriter
    {
        private PropertyItemProvider _provider;

        internal BuildConfigWriter(PropertyItemProvider provider)
        {
            _provider = provider;
        }

        public void Write(ProjectItemGroupElement element)
        {
            ExpressionList explist = null;
            if (!String.IsNullOrEmpty(element.Condition))
                explist = new ExpressionParser().Parse(element.Condition, ExpressionValidationType.StrictBoolean);

            if (explist.Count != 1)
                explist = null;

            if (explist != null)
            {
                bool success;
                bool? result = new ExpressionEvaluator(_provider).EvaluateAsBoolean(explist.First(), out success);
                if (success && !result.Value)
                    return;
            }
        }

        private BuildConditional OpenConditional(BinaryExpression expression)
        {
            return new BuildConditional(expression);
        }
    }

    public class BuildConditional : IDisposable
    {
        internal BuildConditional(BinaryExpression expression)
        {
        }

        ~BuildConditional()
        {
            throw new Exception();
        }

        void Else(BinaryExpression expression)
        {
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
