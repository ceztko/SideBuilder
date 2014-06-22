// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.IO;
using Microsoft.Build.Expressions.Internal;
using Microsoft.Build.Execution;

namespace SideBuilder.Core
{
    public static class Extensions
    {
        public static IEnumerable<ProjectElement> Iterate(this ProjectRootElement xml, Project project)
        {
            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(xml, new MSBuildProjectWrapper(project), visitedImports);
        }

        public static IEnumerable<ProjectElement> Iterate(this ProjectRootElement xml, ProjectInstance project)
        {
            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(xml, new MSBuildProjectInstanceWrapper(project), visitedImports);
        }

        internal static IEnumerable<ProjectElement> Iterate(this ProjectRootElement xml, PropertyItemProvider provider)
        {
            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(xml, provider, visitedImports);
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectRootElement root,
            PropertyItemProvider provider, HashSet<string> visitedImports)
        {
            foreach (ProjectElement element in root.Children)
            {
                ProjectImportGroupElement group = element as ProjectImportGroupElement;
                if (group != null)
                {
                    foreach (ProjectElement element2 in Iterate(group, provider, visitedImports))
                        yield return element2;
                }

                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, provider, visitedImports))
                        yield return element2;
                }

                yield return element;
            }
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectImportGroupElement group,
            PropertyItemProvider provider, HashSet<string> visitedImports)
        {
            foreach (ProjectElement element in group.Children)
            {
                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, provider, visitedImports))
                        yield return element2;
                }
                else
                {
                }
            }
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectImportElement import,
            PropertyItemProvider provider, HashSet<string> visitedImports)
        {
            string path = new ExpressionEvaluator(provider, null).Evaluate(import.Project);
            string basepath = Path.GetDirectoryName(import.Location.File);
            string[] paths = PathUtils.ExpandPath(basepath, path);
            Array.Sort(paths, StringComparer.InvariantCulture);
            foreach (string filepath in paths)
            {
                if (visitedImports.Contains(filepath))
                    continue;

                visitedImports.Add(filepath);
                ProjectRootElement importRoot = null;
                try
                {
                    importRoot = ProjectRootElement.Open(filepath);
                }
                catch (Exception)
                {
                    continue;
                }
                foreach (ProjectElement element2 in Iterate(importRoot, provider, visitedImports))
                    yield return element2;
            }
        }

        public static void Test(Project project)
        {
            string test = "!$([System.String]::IsNullOrEmpty('$(TargetFrameworkVersion)'))";

            bool test3 = new ExpressionEvaluator(project).EvaluateAsBoolean(test);
            test = "$(TargetFrameworkVersion) == $(TargetFramork) and $(TargetFrameworkVersion) == '4.0'";
            bool success;
            bool? result = new ExpressionEvaluator(project).EvaluateAsBoolean(test, out success);

            ExpressionList exp2 = new ExpressionParser().Parse("Exists('$(MSBuildToolsPath)\\Microsoft.WorkflowBuildExtensions.targets')", ExpressionValidationType.StrictBoolean);
            ExpressionList exp = new ExpressionParser().Parse(test, ExpressionValidationType.StrictBoolean);
            List<ExpressionList> list = new List<ExpressionList>();

            foreach (ProjectElement element in project.Xml.Iterate(project))
            {
                foo(element);

                if (!String.IsNullOrEmpty(element.Condition))
                {
                    ExpressionList exps = new ExpressionParser().Parse(element.Condition, ExpressionValidationType.StrictBoolean);
                    list.Add(exps);
                }
            }
        }

        public static void foo(ProjectElement element)
        {
            switch (element.GetElementType())
            {
                case ElementType.ItemGroup:
                {
                    ProjectItemGroupElement itemGroup = element as ProjectItemGroupElement;
                    break;
                }
                case ElementType.PropertyGroup:
                {
                    ProjectPropertyGroupElement propertyGroup = element as ProjectPropertyGroupElement;
                    break;
                }
            }
        }

        private static ElementType GetElementType(this ProjectElement element)
        {
            switch (element.GetType().Name)
            {
                case "ProjectItemGroupElement":
                    return ElementType.ItemGroup;
                case "ProjectPropertyGroupElement":
                    return ElementType.PropertyGroup;
                default:
                    return ElementType.Unsupported;
            }
        }
    }

    public enum ItemType
    {
        Unsupported = 0,
        ClCompile,
        ClInclude
    }

    public enum KnownItemDefinition
    {
        ClCompile
    }

    public enum KnwownClCompileMetatada
    {
        PrecompiledHeader,
        PrecompiledHeaderFile,
        WarningLevel,
        Optimization,
        PreprocessorDefinitions
    }

    internal enum ElementType
    {
        Unsupported,
        ItemGroup,
        PropertyGroup
    }
}
