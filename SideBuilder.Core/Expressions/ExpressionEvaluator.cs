//
// ExpressionEvaluator.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//   Francesco Pretto (ceztko@gmail.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
// Copyright (C) 2014 Francesco Pretto
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Linq;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.IO;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Expressions.Internal
{
	class ExpressionEvaluator
	{
        public ExpressionEvaluator(Project project, string replacementForMissingPropertyAndItem)
        {
            ReplacementForMissingPropertyAndItem = replacementForMissingPropertyAndItem;
            Project = new MSBuildProjectWrapper(project);
        }

        public ExpressionEvaluator(ProjectInstance project, string replacementForMissingPropertyAndItem)
        {
            ReplacementForMissingPropertyAndItem = replacementForMissingPropertyAndItem;
            Project = new MSBuildProjectInstanceWrapper(project);
        }

        public PropertyItemProvider Project { get; private set; }
		
		public string ReplacementForMissingPropertyAndItem { get; set; }
		
		// it is to prevent sequential property value expansion in boolean expression
        public string Wrapper
        {
            get { return ReplacementForMissingPropertyAndItem != null ? "'" : null; }
        }

        public string Evaluate(string source)
        {
            return Evaluate(source, new ExpressionParserManual(source ?? string.Empty, ExpressionValidationType.LaxString).Parse());
        }

        internal string Evaluate(string source, EvaluateOptions options, out bool success)
        {
            return Evaluate(source, new ExpressionParserManual(source ?? string.Empty, ExpressionValidationType.LaxString).Parse(), options, out success);
        }

        internal string Evaluate(string source, ExpressionList exprList)
        {
            bool success;
            return Evaluate(source, exprList, EvaluateOptions.EvaluateAll, out success);
        }

        internal string Evaluate(string source, ExpressionList exprList, EvaluateOptions options, out bool success)
		{
			if (exprList == null)
				throw new ArgumentNullException ("exprList");

            bool outersucc = true;
            string outerval = string.Concat(exprList.Select(e =>
            {
                bool innersucc;
                string innerval = e.EvaluateAsString(CreateContext(source), options, out innersucc);
                if (!innersucc)
                    outersucc = false;

                return innerval;
            }));

            success = outersucc;
            return outerval;
		}

        public bool EvaluateAsBoolean(string source)
        {
            bool success;
            return EvaluateAsBoolean(source, EvaluateOptions.EvaluateAll, out success).Value;
        }

        public bool? EvaluateAsBoolean(string source, out bool success)
        {
            return EvaluateAsBoolean(source, EvaluateOptions.EvaluateEagerly, out success);
        }

        private bool? EvaluateAsBoolean(string source, EvaluateOptions options, out bool success)
        {
            ExpressionList el = null;

            try
            {
                el = new ExpressionParser().Parse(source, ExpressionValidationType.StrictBoolean);
            }
            catch (yyParser.yyException ex)
            {
                throw new InvalidProjectFileException(string.Format("failed to evaluate expression as boolean: '{0}': {1}", source, ex.Message), ex);
            }

            if (el.Count != 1)
                throw new InvalidProjectFileException("Unexpected number of tokens: " + el.Count);

            return el.First().EvaluateAsBoolean(CreateContext(source), options, out success);
        }

        private EvaluationContext CreateContext(string source)
        {
            return new EvaluationContext(source, this);
        }
	}
	
	class EvaluationContext
	{
		public EvaluationContext(string source, ExpressionEvaluator evaluator)
		{
			Source = source;
			Evaluator = evaluator;
		}

		public string Source { get; private set; }
		
		public ExpressionEvaluator Evaluator { get; private set; }
		public ItemProvider ContextItem { get; set; }

        private Stack<ItemProvider> evaluating_items = new Stack<ItemProvider>();
        private Stack<PropertyProvider> evaluating_props = new Stack<PropertyProvider>();
		
		public string EvaluateItem(string itemType, ItemProvider item)
		{
			if (evaluating_items.Contains (item))
				throw new InvalidProjectFileException (string.Format ("Recursive reference to item '{0}' was found", itemType));

            try
            {
                evaluating_items.Push(item);
                return item.EvaluatedInclude;
            }
            finally
            {
                evaluating_items.Pop();
            }
		}

        internal string EvaluateProperty(string name)
        {
            bool success;
            return EvaluateProperty(name, EvaluateOptions.None, out success);
        }

		public string EvaluateProperty(string name, EvaluateOptions options, out bool success)
		{
            var prop = Evaluator.Project.GetProperty (name);

            if (prop == null)
            {
                success = false;
                if (options.HasFlag(EvaluateOptions.EvaluateEagerly))
                    return "$(" + name + ")";

                return null;
            }

            success = true;
            return prop.EvaluatedValue;
		}
		
		public string EvaluateProperty(PropertyProvider prop, string name, string value)
		{
			if (evaluating_props.Contains (prop))
				throw new InvalidProjectFileException (string.Format ("Recursive reference to property '{0}' was found", name));
            try
            {
                evaluating_props.Push(prop);
                // CHECK-ME: needs verification on whether string evaluation is appropriate or not.
                return Evaluator.Evaluate(value);
            }
            finally
            {
                evaluating_props.Pop();
            }
		}
	}
	
	abstract partial class Expression
	{
        public bool EvaluateAsBoolean(EvaluationContext context)
        {
            bool success;
            return EvaluateAsBoolean(context, EvaluateOptions.EvaluateAll, out success).Value;
        }

        public bool? EvaluateAsBoolean(EvaluationContext context, out bool success)
        {
            return EvaluateAsBoolean(context, EvaluateOptions.EvaluateEagerly, out success);
        }

        public string EvaluateAsString(EvaluationContext context)
        {
            bool success;
            return EvaluateAsString(context, EvaluateOptions.EvaluateAll, out success);
        }

        public string EvaluateAsString(EvaluationContext context, out bool success)
        {
            return EvaluateAsString(context, EvaluateOptions.EvaluateEagerly, out success);
        }

        public object EvaluateAsObject(EvaluationContext context)
        {
            bool success;
            return EvaluateAsObject(context, EvaluateOptions.EvaluateAll, out success);
        }

        public object EvaluateAsObject(EvaluationContext context, out bool success)
        {
            return EvaluateAsObject(context, EvaluateOptions.EvaluateEagerly, out success);
        }
 
        public bool EvaluateStringAsBoolean(EvaluationContext context, string ret)
        {
            bool success;
            return EvaluateStringAsBoolean(context, ret, EvaluateOptions.None, out success).Value;
        }

        public bool? EvaluateStringAsBoolean(EvaluationContext context, string ret, out bool success)
        {
            return EvaluateStringAsBoolean(context, ret, EvaluateOptions.EvaluateEagerly, out success);
        }

        internal bool? EvaluateStringAsBoolean(EvaluationContext context, string ret, EvaluateOptions options, out bool success)
		{
			if (ret != null)
            {
                if (ret.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
                {
                    success = true;
                    return true;
                }
                else if (ret.Equals("FALSE", StringComparison.InvariantCultureIgnoreCase))
                {
                    success = true;
                    return false;
                }
			}

            if (!options.HasFlag(EvaluateOptions.EvaluateEagerly))
			    throw new InvalidProjectFileException (this.Location,
                    string.Format("Condition '{0}' is evaluated as '{1}' and cannot be converted to boolean",
                        context.Source, ret));

            success = false;
            return null;
		}

        protected internal abstract bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success);

        protected internal abstract string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success);

        protected internal abstract object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success);
	}
	
	partial class BinaryExpression : Expression
	{
        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            bool result = false;

            bool lsucc;
            bool rsucc;
			switch (Operator)
            {
                case Operator.EQ:
                {
                    string lres = Left.EvaluateAsString(context, options, out lsucc);
                    string rres = Right.EvaluateAsString(context, options, out rsucc);
                    success = lsucc && rsucc;
                    if (success)
                        return string.Equals(StripStringWrap(lres), StripStringWrap(rres), StringComparison.OrdinalIgnoreCase);
                    else
                        return null;
                }
                case Operator.NE:
                {
                    success = true;
                    string lres = Left.EvaluateAsString(context, options, out lsucc);
                    string rres = Right.EvaluateAsString(context, options, out rsucc);
                    success = lsucc && rsucc;
                    if (success)
                        return !string.Equals(StripStringWrap(lres), StripStringWrap(rres), StringComparison.OrdinalIgnoreCase);
                    else
                        return null;
                }
                case Operator.And:
                {
                    // Always evaluate to detect possible syntax error on right expr.
                    bool? lres = Left.EvaluateAsBoolean(context, options, out lsucc);
                    bool? rres = Right.EvaluateAsBoolean(context, options, out rsucc);
                    if (options.HasFlag(EvaluateOptions.EvaluateEagerly))
                    {
                        if (lsucc)
                        {
                            if (rsucc)
                            {
                                success = true;
                                return lres.Value && rres.Value;
                            }
                            else
                            {
                                if (lres.Value)
                                {
                                    success = false;
                                    return null;
                                }
                                else
                                {
                                    success = true;
                                    return false;
                                }
                            }
                        }
                        else if (rsucc)
                        {
                            if (rres.Value)
                            {
                                success = false;
                                return null;
                            }
                            else
                            {
                                success = true;
                                return false;
                            }
                        }
                        else
                        {
                            success = false;
                            return null;
                        }
                    }
                    else
                    {
                        success = lsucc && rsucc;
                        if (success)
                            return lres.Value && rres.Value;
                        else
                            return null;
                    }
                }
                case Operator.Or:
                {
                    // Always evaluate to detect possible syntax error on right expr.
                    bool? lres = Left.EvaluateAsBoolean(context, options, out lsucc);
                    bool? rres = Right.EvaluateAsBoolean(context, options, out rsucc);
                    if (options.HasFlag(EvaluateOptions.EvaluateEagerly))
                    {
                        if (lsucc)
                        {
                            if (rsucc)
                            {
                                success = true;
                                return lres.Value || rres.Value;
                            }
                            else
                            {
                                if (lres.Value)
                                {
                                    success = true;
                                    return true;
                                }
                                else
                                {
                                    success = false;
                                    return null;
                                }
                            }
                        }
                        else if (rsucc)
                        {
                            if (rres.Value)
                            {
                                success = true;
                                return true;
                            }
                            else
                            {
                                success = false;
                                return null;
                            }
                        }
                        else
                        {
                            success = false;
                            return null;
                        }
                    }
                    else
                    {
                        success = lsucc && rsucc;
                        if (success)
                            return lres.Value || rres.Value;
                        else
                            return null;
                    }
                }
			}

			// comparison expressions - evaluate comparable first, then compare values.
            var left = Left.EvaluateAsObject(context, options, out lsucc);
            var right = Right.EvaluateAsObject(context, options, out rsucc);
            if (!(lsucc && rsucc))
            {
                success = false;
                return null;
            }

            IComparable comparable = left as IComparable;
            if (!(comparable != null && right is IComparable))
            {
                if (options.HasFlag(EvaluateOptions.EvaluateEagerly))
                {
                    success = false;
                    return null;
                }

                throw new InvalidProjectFileException("expression cannot be evaluated as boolean");
            }

            int compared = comparable.CompareTo(right);

			switch (Operator)
            {
			case Operator.GE:
				result = compared >= 0;
                break;
			case Operator.GT:
                result = compared > 0;
                break;
			case Operator.LE:
                result = compared <= 0;
                break;
			case Operator.LT:
                result = compared < 0;
                break;
            default:
                throw new InvalidOperationException();
			}

            success = true;
            return result;
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            throw new NotImplementedException();
        }

        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            bool lsucc;
            bool rsucc;
            string left = Left.EvaluateAsString(context, options, out lsucc);
            string right = Right.EvaluateAsString(context, options, out rsucc);
            success = lsucc && rsucc;
            return left + strings[Operator] + right;
        }

		private string StripStringWrap(string s)
		{
			if (s == null)
				return string.Empty;
			s = s.Trim ();
			if (s.Length > 1 && s [0] == '"' && s [s.Length - 1] == '"')
				return s.Substring (1, s.Length - 2);
			else if (s.Length > 1 && s [0] == '\'' && s [s.Length - 1] == '\'')
				return s.Substring (1, s.Length - 2);
			return s;
		}

        private static readonly Dictionary<Operator, string> strings = new Dictionary<Operator, string>() {
			{Operator.EQ, " == "},
			{Operator.NE, " != "},
			{Operator.LT, " < "},
			{Operator.LE, " <= "},
			{Operator.GT, " > "},
			{Operator.GE, " >= "},
			{Operator.And, " And "},
			{Operator.Or, " Or "},
		};
    }
	
	partial class BooleanLiteral : Expression
	{
        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            success = true;
            return Value ? "True" : "False";
        }

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            success = true;
            return Value;
        }

        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            success = true;
            return Value;
        }
    }

	partial class NotExpression : Expression
	{
        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            string val = Negated.EvaluateAsString(context, options, out success);
            if (val == null)
                return null;

            // no negation for string
            return "!" + val;
        }

        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            bool? result = Negated.EvaluateAsBoolean(context, options, out success);
            if (!success)
                return null;

            return !result.Value;
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            return EvaluateAsString(context, options, out success);
        }
	}

	partial class PropertyAccessExpression : Expression
	{
        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            var ret = EvaluateAsString(context, options, out success);
            if (!success)
                return null;

            return EvaluateStringAsBoolean(context, ret, options, out success);
        }

        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            var ret = EvaluateAsObject(context, options, out success);

			// CHECK-ME: this "wrapper" is kind of hack, to prevent sequential property references such as $(X)$(Y).
            return ret == null ? context.Evaluator.ReplacementForMissingPropertyAndItem : context.Evaluator.Wrapper + ret.ToString() + context.Evaluator.Wrapper;
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            try
            {
                return DoEvaluateAsObject(context, options, out success);
            }
            catch (TargetInvocationException ex)
            {
                throw new InvalidProjectFileException("Access to property caused an error", ex);
            }
		}

        private object DoEvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            if (Access.Target == null)
            {
                return context.EvaluateProperty(Access.Name.Name, options, out success);
            }
            else
            {
                if (this.Access.TargetType == PropertyTargetType.Object && options.HasFlag(EvaluateOptions.EvaluateInstanceMembers))
                {
                    var obj = Access.Target.EvaluateAsObject(context, options, out success);
                    if (!success || obj == null)
                        return null;

                    if (Access.Arguments != null)
                    {
                        bool outersucc = true;
                        var args = Access.Arguments.Select(e =>
                        {
                            bool innersucc;
                            object ret = e.EvaluateAsObject(context, options, out innersucc);
                            if (!innersucc)
                                outersucc = false;

                            return ret;
                        }).ToArray();
                        success = outersucc;
                        if (!success)
                            return null;

                        var method = FindMethod(obj.GetType(), Access.Name.Name, args);
                        if (method == null)
                        {
                            throw new InvalidProjectFileException(Location, string.Format("access to undefined method '{0}' of '{1}' at {2}",
                                Access.Name.Name, Access.Target.EvaluateAsString(context, options, out success), Location));
                        }

                        return method.Invoke(obj, AdjustArgsForCall(method, args));
                    }
                    else
                    {
                        var prop = obj.GetType().GetProperty(Access.Name.Name);
                        if (prop == null)
                            throw new InvalidProjectFileException(Location, string.Format("access to undefined property '{0}' of '{1}' at {2}",
                                Access.Name.Name, Access.Target.EvaluateAsString(context, options, out success), Location));
                        return prop.GetValue(obj, null);
                    }
                }
                else if (options.HasFlag(EvaluateOptions.EvaluateStaticMembers))
                {
                    var type = Type.GetType(Access.Target.EvaluateAsString(context, options, out success));
                    if (type == null)
                        throw new InvalidProjectFileException(Location, string.Format("specified type '{0}' was not found",
                            Access.Target.EvaluateAsString(context, options, out success)));

                    if (Access.Arguments != null)
                    {
                        bool outersucc = true;
                        var args = Access.Arguments.Select(e =>
                        {
                            bool innersucc;
                            object ret = e.EvaluateAsObject(context, options, out innersucc);
                            if (!innersucc)
                                outersucc = false;

                            return ret;
                        }).ToArray();

                        success = outersucc;
                        if (!success)
                            return null;

                        var method = FindMethod(type, Access.Name.Name, args);
                        if (method == null)
                            throw new InvalidProjectFileException(Location, string.Format("access to undefined static method '{0}' of '{1}' at {2}",
                                Access.Name.Name, Access.Target.EvaluateAsString(context, options, out success), Location));

                        return method.Invoke(null, AdjustArgsForCall(method, args));
                    }
                    else
                    {
                        var prop = type.GetProperty(Access.Name.Name);
                        if (prop == null)
                            throw new InvalidProjectFileException(Location, string.Format("access to undefined static property '{0}' of '{1}' at {2}",
                                Access.Name.Name, Access.Target.EvaluateAsString(context, options, out success), Location));

                        return prop.GetValue(null, null);
                    }
                }
                else
                {
                    success = false;
                    return null;
                }
            }
		}
	
		private MethodInfo FindMethod(Type type, string name, object [] args)
		{
            var methods = type.GetMethods().Where(m =>
            {
                if (m.Name != name)
                    return false;
                var pl = m.GetParameters();
                if (pl.Length == args.Length)
                    return true;
                // calling String.Format() with either set of arguments is valid:
                // - three strings (two for varargs)
                // - two strings (happen to be exact match)
                // - one string (no varargs)
                if (pl.Length > 0 && pl.Length - 1 <= args.Length &&
                    pl.Last().GetCustomAttributesData().Any(a => a.Constructor.DeclaringType == typeof(ParamArrayAttribute)))
                    return true;
                return false;
            });

			if (methods.Count () == 1)
				return methods.First ();

			return args.Any (a => a == null) ? 
				type.GetMethod (name) :
				type.GetMethod (name, args.Select (o => o.GetType ()).ToArray ());
		}
		
		private object [] AdjustArgsForCall (MethodInfo m, object[] args)
		{
			if (m.GetParameters ().Length == args.Length + 1)
				return args.Concat (new object[] {Array.CreateInstance (m.GetParameters ().Last ().ParameterType.GetElementType (), 0)}).ToArray ();
			else
				return args;
		}
    }

	partial class ItemAccessExpression : Expression
	{
        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            string val = EvaluateAsString(context, options, out success);
            if (!success)
                return null;

            return EvaluateStringAsBoolean(context, val, options, out success);
        }

        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            success = false;
			string itemType = Application.Name.Name;
			var items = context.Evaluator.Project.GetItems(itemType);

			if (!items.Any())
				return context.Evaluator.ReplacementForMissingPropertyAndItem;

            if (Application.Expressions == null)
            {
                return string.Join(";", items.Select(item => context.EvaluateItem(itemType, item)));
            }
            else
            {
                bool outersucc = true;
                string ret = string.Join(";", items.Select(item =>
                {
                    context.ContextItem = item;
                    var innret1 = string.Concat(Application.Expressions.Select(e =>
                    {
                        bool innersucc;
                        string innret2 = e.EvaluateAsString(context, options, out innersucc);
                        if (!innersucc)
                            outersucc = false;

                        return innret2;
                    }));

                    context.ContextItem = null;
                    return innret1;
                }));

                success = outersucc;
                return ret;
            }
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            return EvaluateAsString(context, options, out success);
		}
    }

	partial class MetadataAccessExpression : Expression
	{
        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            string val = EvaluateAsString(context, options, out success);
            if (!success)
                return null;

            return EvaluateStringAsBoolean(context, val, options, out success);
        }

        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
		{
			string itemType = this.Access.ItemType != null ? this.Access.ItemType.Name : null;
			string metadataName = Access.Metadata.Name;
			IEnumerable<ItemProvider> items;
			if (this.Access.ItemType != null)
				items = context.Evaluator.Project.GetItems (itemType);
			else if (context.ContextItem != null)
                items = new ItemProvider[] { context.ContextItem };
			else
				items = context.Evaluator.Project.AllItems;
			
			var values = items.Select (i => i.GetMetadataValue(metadataName)).Where (s => !string.IsNullOrEmpty (s));
            success = true;
			return string.Join (";", values);
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            return EvaluateAsString(context, options, out success);
		}
    }
	partial class StringLiteral : Expression
	{
        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            var ret = EvaluateAsString(context, options, out success);
            if (!success)
                return null;
            
            return EvaluateStringAsBoolean(context, ret, options, out success);
        }

        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            return context.Evaluator.Evaluate(this.Value.Name, options, out success);
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            return EvaluateAsString(context, options, out success);
		}
    }
	partial class RawStringLiteral : Expression
	{
        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            success = true;
			return Value.Name;
		}

        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            if (!options.HasFlag(EvaluateOptions.EvaluateEagerly))
                throw new InvalidProjectFileException("raw string literal cannot be evaluated as boolean");

            success = false;
            return null;
        }

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
            return EvaluateAsString(context, options, out success);
		}
    }
	
	partial class FunctionCallExpression : Expression
	{
        protected internal override string EvaluateAsString(EvaluationContext context, EvaluateOptions options, out bool success)
		{
			throw new NotImplementedException ();
		}

        protected internal override bool? EvaluateAsBoolean(EvaluationContext context, EvaluateOptions options, out bool success)
        {
            if (!options.HasFlag(EvaluateOptions.EvaluateFunctions))
            {
                success = false;
                return null;
            }

            if (string.Equals(Name.Name, "Exists", StringComparison.OrdinalIgnoreCase))
            {
                if (Arguments.Count != 1)
                    throw new InvalidProjectFileException(Location, "Function 'Exists' expects 1 argument");

                string val = Arguments.First().EvaluateAsString(context, options, out success);
                if (!success)
                    return null;

                val = WindowsCompatibilityExtensions.FindMatchingPath(val);
                return Directory.Exists(val) || System.IO.File.Exists(val);
            }
            if (string.Equals(Name.Name, "HasTrailingSlash", StringComparison.OrdinalIgnoreCase))
            {
                if (Arguments.Count != 1)
                    throw new InvalidProjectFileException(Location, "Function 'HasTrailingSlash' expects 1 argument");

                string val = Arguments.First().EvaluateAsString(context, options, out success);
                if (!success)
                    return null;
                
                return val.LastOrDefault() == '\\' || val.LastOrDefault() == '/';
            }

			throw new InvalidProjectFileException (Location, string.Format ("Unsupported function '{0}'", Name));
		}

        protected internal override object EvaluateAsObject(EvaluationContext context, EvaluateOptions options, out bool success)
		{
			throw new NotImplementedException ();
		}
    }

    [Flags]
    public enum EvaluateOptions
    {
        None = 0,
        EvaluateEagerly = 1,
        EvaluateFunctions = 2,
        EvaluateStaticMembers = 4,
        EvaluateInstanceMembers = 8,
        EvaluateAll = EvaluateFunctions | EvaluateStaticMembers | EvaluateInstanceMembers
    }
}

