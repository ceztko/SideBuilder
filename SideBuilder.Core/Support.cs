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

namespace SideBuilder.Core
{
    public static class Extensions
    {
        public static IEnumerable<ProjectElement> Iterate(this Project project)
        {
            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(project.Xml, project, visitedImports);
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectRootElement root,
            Project project, HashSet<string> visitedImports)
        {
            foreach (ProjectElement element in root.Children)
            {
                ProjectImportGroupElement group = element as ProjectImportGroupElement;
                if (group != null)
                {
                    foreach (ProjectElement element2 in Iterate(group, project, visitedImports))
                        yield return element2;
                }

                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, project, visitedImports))
                        yield return element2;
                }

                yield return element;
            }
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectImportGroupElement group,
            Project project, HashSet<string> visitedImports)
        {
            foreach (ProjectElement element in group.Children)
            {
                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, project, visitedImports))
                        yield return element2;
                }
                else
                {
                }
            }
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectImportElement import,
            Project project, HashSet<string> visitedImports)
        {
            string path = new ExpressionEvaluator(project, null).Evaluate(import.Project);
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
                    importRoot = ProjectRootElement.Open(filepath, project.ProjectCollection);
                }
                catch (Exception)
                {
                    continue;
                }
                foreach (ProjectElement element2 in Iterate(importRoot, project, visitedImports))
                    yield return element2;
            }
        }

        public static void Test(Project project)
        {
            string test = "!$([System.String]::IsNullOrEmpty('$(TargetFrameworkVersion)'))";
            bool test3 = new ExpressionEvaluator(project, null).EvaluateAsBoolean(test);
            ExpressionList exp2 = new ExpressionParser().Parse("Exists('$(MSBuildToolsPath)\\Microsoft.WorkflowBuildExtensions.targets')", ExpressionValidationType.StrictBoolean);
            ExpressionList exp = new ExpressionParser().Parse(test, ExpressionValidationType.StrictBoolean);
            List<ExpressionList> list = new List<ExpressionList>();
            foreach (ProjectElement element in project.Iterate())
            {
                if (!String.IsNullOrEmpty(element.Condition))
                {
                    ExpressionList exps = new ExpressionParser().Parse(element.Condition, ExpressionValidationType.StrictBoolean);
                    list.Add(exps);
                }
            }
        }
    }

    public enum ItemType
    {
        Unsupported = 0,
        ClCompile,
        ClInclude
    }
}
