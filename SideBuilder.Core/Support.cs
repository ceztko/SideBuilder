// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace SideBuilder.Core
{
    public static class Extensions
    {
        public static IEnumerable<ProjectElement> Iterate(ProjectRootElement root, ProjectCollection collection)
        {
            foreach (ProjectElement element in root.Children)
            {
                ProjectImportGroupElement group = element as ProjectImportGroupElement;
                if (group != null)
                {
                    foreach (ProjectElement element2 in Iterate(group, collection))
                        yield return element2;
                }

                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, collection))
                        yield return element2;
                }

                yield return element;
            }
        }

        public static IEnumerable<ProjectElement> Iterate(ProjectImportGroupElement group, ProjectCollection collection)
        {
            foreach (ProjectElement element in group.Children)
            {
                ProjectImportElement import = element as ProjectImportElement;
                if (import != null)
                {
                    foreach (ProjectElement element2 in Iterate(import, collection))
                        yield return element2;
                }
                else
                {
                }
            }
        }

        public static IEnumerable<ProjectElement> Iterate(ProjectImportElement import, ProjectCollection collection)
        {
            ProjectRootElement importRoot = ProjectRootElement.Open(import.Project);
            foreach (ProjectElement element2 in Iterate(importRoot, collection))
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
