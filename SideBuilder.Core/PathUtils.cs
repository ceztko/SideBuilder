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
        /// <summary>
        /// Expand path with wildcard
        /// </summary>
        public static string[] ExpandPath(string basepath, string path)
        {
            if (String.IsNullOrEmpty(path))
                return new string[] { };

            string filepart = Path.GetFileName(path);
            string dirpart = path.Substring(0, path.Length - filepart.Length);
            if (dirpart == String.Empty)
            {
                if (filepart == String.Empty)
                    return new string[] { };
                else
                    dirpart = basepath;
            }
            else
            {
                dirpart = Path.IsPathRooted(dirpart) ? dirpart : Path.Combine(basepath, dirpart);
                if (filepart == String.Empty)
                    return new string[] { Path.GetFullPath(dirpart) };
            }

            string fullPath = Path.GetFullPath(dirpart);
            if (Directory.Exists(fullPath))
                return Directory.GetFiles(fullPath, filepart, SearchOption.TopDirectoryOnly);
            else
                return new string[] { };
        }
    }
}
