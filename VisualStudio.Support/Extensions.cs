// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;

namespace VisualStudio.Support
{
    public static partial class Extensions
    {
        public static Guid GetProjectGuid(this IVsHierarchy project)
        {
            Guid ret;
            project.GetGuidProperty(VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out ret);
            return ret;
        }

        public static IVsHierarchy GetHierarchy(this IVsSolution solution)
        {
            return solution as IVsHierarchy;
        }

        public static IVsBuildMacroInfo GetBuildMacroInfo(this IVsProject project)
        {
            return project as IVsBuildMacroInfo;
        }

        public static IVsBuildPropertyStorage GetBuildPropertyStorage(this IVsHierarchy project)
        {
            return project as IVsBuildPropertyStorage;
        }

        public static IVsHierarchy GetProjectHierarchy(IVsSolution solution, Project project)
        {
            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);
            return hierarchy;
        }

        public static IVsWindowFrame GetWindowFrame(this IVsTextView textView)
        {
            IVsTextViewEx textViewEx = textView as IVsTextViewEx;

            object ret;
            textViewEx.GetWindowFrame(out ret);
            return (IVsWindowFrame)ret;
        }

        public static HiearchyItemPair GetHiearchyItemPair(this IVsWindowFrame frame)
        {
            object hiearchy;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out hiearchy);

            object itemid;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out itemid);

            return new HiearchyItemPair((IVsHierarchy)hiearchy, (uint)(int)itemid);
        }

        public static ProjectItem GetProjectItem(this HiearchyItemPair pair)
        {
            object item;
            pair.Hierarchy.GetProperty(pair.ItemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out item);
            return item as ProjectItem;
        }

        public static Project GetProject(this IVsHierarchy hierarchy)
        {
            object project;
             if (!ErrorHandler.Succeeded(hierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ExtObject, out project)))
                 return null;

            return project as Project;
        }

        public static IEnumerable<Project> AllProjects(this Projects projects)
        {
            foreach (Project project in projects)
            {
                foreach (Project subproject in AllProjects(project))
                    yield return subproject;
            }
        }

        private static IEnumerable<Project> AllProjects(this Project project)
        {
            if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
            {
                // The Project is a Solution folder
                foreach (ProjectItem projectItem in project.ProjectItems)
                {
                    if (projectItem.SubProject != null)
                    {
                        // The ProjectItem is actually a Project
                        foreach (Project subproject in AllProjects(projectItem.SubProject))
                            yield return subproject;
                    }
                }
            }
#if MORE_PEDANTIC
            else if (project.ConfigurationManager != null)
#else
            else
#endif
                yield return project;
        }

        public static Lazy<T, IDictionary<string, object>> FindExport<T>(IEnumerable<Lazy<T, IDictionary<string, object>>> collection, string metadataName, string metadataValue)
        {
            foreach (Lazy<T, IDictionary<string, object>> lazy in collection)
            {
                object obj = lazy.Metadata[metadataName];
                string str = obj as string;
                if (str != null)
                {
                    if (str.Equals(metadataValue, StringComparison.OrdinalIgnoreCase))
                        return lazy;
                }
                else
                {
                    string[] array = obj as string[];
                    if (array.Contains(metadataValue, StringComparer.OrdinalIgnoreCase))
                        return lazy;
                }
            }

            return null;
        }

        public static string TrimStart(this string str, string start)
        {
            if (!str.StartsWith(start))
                return null;

            return str.Substring(start.Length);
        }
    }

    public class HiearchyItemPair
    {
        private IVsHierarchy _Hierarchy;
        private uint _ItemId;

        public HiearchyItemPair(IVsHierarchy hierarchy, uint itemid)
        {
            _Hierarchy = hierarchy;
            _ItemId = itemid;
        }

        public IVsHierarchy Hierarchy
        {
            get { return _Hierarchy; }
        }

        public uint ItemId
        {
            get { return _ItemId; }
        }
    }
}
