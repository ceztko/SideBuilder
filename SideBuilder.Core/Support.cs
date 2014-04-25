// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Internal.Expressions;

namespace SideBuilder.Core
{
    public static class Extensions
    {
        public static IEnumerable<ProjectElement> Iterate(this Project project)
        {
            return Iterate(project.Xml, project);
        }

        public static IEnumerable<ProjectElement> Iterate(ProjectRootElement root, Project project)
        {
            foreach (ProjectElement element in root.Children)
            {
                ProjectImportGroupElement group = element as ProjectImportGroupElement;
                if (group != null)
                {
                    foreach (ProjectElement element2 in Iterate(group, project))
                        yield return element2;
                }

                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, project))
                        yield return element2;
                }

                yield return element;
            }
        }

        public static IEnumerable<ProjectElement> Iterate(ProjectImportGroupElement group, Project project)
        {
            foreach (ProjectElement element in group.Children)
            {
                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, project))
                        yield return element2;
                }
                else
                {
                }
            }
        }

        public static IEnumerable<ProjectElement> Iterate(ProjectImportElement import, Project project)
        {
            string path = new ExpressionEvaluator(project, null).Evaluate(import.Project);
            ProjectRootElement importRoot = ProjectRootElement.Open(path, project.ProjectCollection);
            foreach (ProjectElement element2 in Iterate(importRoot, project))
                yield return element2;
        }
    }

    public enum ItemType
    {
        Unsupported = 0,
        ClCompile,
        ClInclude
    }
}
