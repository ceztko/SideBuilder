// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.IO;
using Microsoft.Build.Expressions;
using Microsoft.Build.Execution;
using MSBuild.Support;
using Collections.Specialized;

namespace SideBuilder.Core
{
    public delegate IEnumerable<string> ProjectImportHandler(ProjectImportElement import, PropertyItemProvider provider);

    public static class BuilderExtensions
    {
        public static IEnumerable<ProjectElement> Iterate(this ProjectRootElement xml, Project project,
            ProjectImportHandler importHandler = null)
        {
            if (importHandler == null)
                importHandler = DefaultProjectImportHandler;

            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(xml, new MSBuildProjectWrapper(project), importHandler, visitedImports);
        }

        public static IEnumerable<ProjectElement> Iterate(this ProjectRootElement xml, ProjectInstance project,
            ProjectImportHandler importHandler = null)
        {
            if (importHandler == null)
                importHandler = DefaultProjectImportHandler;

            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(xml, new MSBuildProjectInstanceWrapper(project), importHandler, visitedImports);
        }

        public static IEnumerable<ProjectElement> Iterate(this ProjectRootElement xml, PropertyItemProvider provider,
            ProjectImportHandler importHandler = null)
        {
            if (importHandler == null)
                importHandler = DefaultProjectImportHandler;

            HashSet<string> visitedImports = new HashSet<string>();
            return Iterate(xml, provider, importHandler, visitedImports);
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectRootElement root, PropertyItemProvider provider,
            ProjectImportHandler importHandler, HashSet<string> visitedImports)
        {
            foreach (ProjectElement element1 in root.Children)
            {
                ProjectImportGroupElement group = element1 as ProjectImportGroupElement;
                if (group != null)
                {
                    foreach (ProjectElement element2 in Iterate(group, provider, importHandler, visitedImports))
                        yield return element2;
                }

                ProjectImportElement import = element1 as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, provider, importHandler, visitedImports))
                        yield return element2;
                }

                yield return element1;
            }
        }

        public static IEnumerable<String> DefaultProjectImportHandler(ProjectImportElement import, PropertyItemProvider provider)
        {
            string path = new ExpressionEvaluator(provider, null).Evaluate(import.Project);
            string basepath = Path.GetDirectoryName(import.Location.File);
            string[] paths = PathUtils.ResolvePath(basepath, path);
            Array.Sort(paths, StringComparer.InvariantCulture);
            return paths;
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectImportGroupElement group,
            PropertyItemProvider provider, ProjectImportHandler importHandler, HashSet<string> visitedImports)
        {
            foreach (ProjectElement element in group.Children)
            {
                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, provider, importHandler, visitedImports))
                        yield return element2;
                }
            }
        }

        private static IEnumerable<ProjectElement> Iterate(ProjectImportElement import,
            PropertyItemProvider provider, ProjectImportHandler importHandler, HashSet<string> visitedImports)
        {
            IEnumerable<string> paths = importHandler(import, provider);
            foreach (string filepath in paths)
            {
                if (visitedImports.Contains(filepath))
                    continue;

                visitedImports.Add(filepath);

                ProjectRootElement importRoot;
                try
                {
                    importRoot = ProjectRootElement.Open(filepath);
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (ProjectElement element in Iterate(importRoot, provider, importHandler, visitedImports))
                    yield return element;
            }
        }
    }
}
