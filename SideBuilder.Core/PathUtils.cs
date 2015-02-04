// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SideBuilder.Core
{
    public static class PathUtils
    {
        public static string[] ResolvePath(string searchpath)
        {
            return ResolvePath(String.Empty, searchpath, ResolvePathOptions.IgnoreRootedSearchPath);
        }

        /// <summary>
        /// Resolve path with current (".") or top-level directory ("..") nodes
        /// or with file part wildcards ("\*.jpg")
        /// </summary>
        /// <param name="basepath">Optional basepath</param>
        /// <returns>Empty list if the searchpath can't be resolved</returns>
        public static string[] ResolvePath(string basepath, string searchpath,
            ResolvePathOptions options = ResolvePathOptions.IgnoreRootedSearchPath)
        {
            if (basepath == null)
                basepath = String.Empty;

            if (String.IsNullOrEmpty(searchpath))
                goto ReturnEmpty;

            string filepart = Path.GetFileName(searchpath);
            string dirpart = searchpath.Substring(0, searchpath.Length - filepart.Length);
            bool dirpartRooted = false;
            if (dirpart == String.Empty)
            {
                if (filepart == String.Empty)
                    goto ReturnEmpty;
                
                dirpart = basepath;
            }
            else
            {
                dirpartRooted = IsPathAbsolutelyRooted(dirpart);
                if (dirpartRooted && !options.HasFlag(ResolvePathOptions.IgnoreRootedSearchPath))
                    throw new Exception("Search path is rooted");

                dirpart = dirpartRooted ? dirpart : Path.Combine(basepath, dirpart);
            }

            if (!IsPathAbsolutelyRooted(dirpart))
                goto ReturnEmpty;

            string fullPath = Path.GetFullPath(dirpart);
            if (!Directory.Exists(fullPath))
                goto ReturnEmpty;

            if (filepart == String.Empty)
                return new string[] { fullPath };

            return Directory.GetFiles(fullPath, filepart, SearchOption.TopDirectoryOnly);

        ReturnEmpty:
                return new string[] { };
        }

        /// <returns>True if the path has a volume or UNC path root</returns>
        public static bool IsPathAbsolutelyRooted(string path)
        {
            string root = Path.GetPathRoot(path);
            if (String.IsNullOrEmpty(root)
                    || root[0] == Path.DirectorySeparatorChar               // "\", true on Windows only
                    || root[root.Length - 1] == Path.VolumeSeparatorChar)   // "C:", true on Windows only
                return false;

            return true;
        }

        public enum ResolvePathOptions
        {
            None = 0,

            /// <summary>
            /// Ignore a rooted search path on not null given basepath
            /// </summary>
            IgnoreRootedSearchPath
        }
    }
}
