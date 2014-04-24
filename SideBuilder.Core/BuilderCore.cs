// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using System.Globalization;
using System.IO;

namespace SideBuilder.Core
{
    public class BuilderCore
    {
        private const string USER_ROOOT_DIR_PROP = "UserRootDir";
        private const string VC_TARGETS_PATH_PROP = "VCTargetsPath";
        private const string MS_BUILD_TOOLS_PATH_PROP = "MSBuildToolsPath";

        public static void Process(Project project)
        {
            string UserRootDir = project.GetPropertyValue(USER_ROOOT_DIR_PROP);
            string VCTargetsPath = project.GetPropertyValue(VC_TARGETS_PATH_PROP);
            string MSBuildToolsPath = project.GetPropertyValue(MS_BUILD_TOOLS_PATH_PROP);
            HashSet<string> pathsToExclude = new HashSet<string>();
            pathsToExclude.Add(UserRootDir);
            pathsToExclude.Add(VCTargetsPath);
            pathsToExclude.Add(MSBuildToolsPath);

            Dictionary<string, FileStream> streams = new Dictionary<string,FileStream>();

            foreach (ProjectItem item in project.ItemsIgnoringCondition)
            {
                string basePath = Path.GetDirectoryName(item.Xml.Location.File);
                if (pathsToExclude.Contains(basePath))
                    continue;

                ItemType type = ToItem(item.ItemType);

                switch (type)
                {
                    case ItemType.ClCompile:
                    case ItemType.ClInclude:
                    {
                        FileStream stream;
                        bool found = streams.TryGetValue(basePath, out stream);
                        if (!found)
                        {
                            string sideBuilderPath = Path.Combine(basePath, "prova.txt");
                            stream = new FileStream(sideBuilderPath, FileMode.Truncate);
                            streams.Add(basePath, stream);
                        }

                        StreamWriter writer = new StreamWriter(stream);
                        writer.WriteLine(item.EvaluatedInclude);
                        foreach (ProjectMetadata metadata in item.DirectMetadata)
                        {
                            switch (metadata.Name)
                            {
                                case "PrecompiledHeader":
                                {
                                    writer.WriteLine(metadata.UnevaluatedValue);
                                    break;
                                }
                            }
                        }

                        writer.Flush();

                        break;
                    }
                }
            }

            foreach (FileStream stream in streams.Values)
                stream.Close();
        }

        public static Dictionary<string, string> GetGlobalProperties(string solutionName, string solutionDir,
            string solutionFilename, string solutionPath, string solutionExp, bool defaultLangID,
            string configuration = null, string platform = null)
        {
            string langID = null;
            if (defaultLangID)
                langID = new CultureInfo("en-US").LCID.ToString();

            return GetGlobalProperties(solutionName, solutionDir, solutionFilename, solutionPath,
                solutionExp, langID, configuration, platform);
        }

        public static Dictionary<string, string> GetGlobalProperties(string solutionName, string solutionDir,
            string solutionFilename, string solutionPath, string solutionExp, string configuration = null,
            string platform = null, string langID = null)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("DevEnvDir", "c:\\Program Files %28x86%29\\Microsoft Visual Studio 10.0\\Common7\\IDE\\");

            ret.Add("SolutionName", solutionName);
            ret.Add("SolutionDir", solutionDir);
            ret.Add("SolutionFileName", solutionFilename);
            ret.Add("SolutionPath", solutionPath);
            ret.Add("SolutionExt", solutionExp);

            if (configuration != null)
                ret.Add("Configuration", configuration);
            if (platform != null)
                ret.Add("Platform", platform);
            if (langID != null)
                ret.Add("LangID", langID);

            return ret;
        }

        public static Project LoadProject(string projectPath, ProjectCollection collection,
            Dictionary<string, string> globalProperties)
        {
            return new Project(projectPath, globalProperties, null, null, collection,
                ProjectLoadSettings.RecordDuplicateButNotCircularImports | ProjectLoadSettings.RejectCircularImports);
        }

        public static ItemType ToItem(string str)
        {
            switch (str)
            {
                case "ClCompile":
                    return ItemType.ClCompile;
                case "ClInclude":
                    return ItemType.ClInclude;
                default:
                    return ItemType.Unsupported;
            }
        }

        public static string ToString(ItemType type)
        {
            switch (type)
            {
                case ItemType.ClCompile:
                    return "ClCompile";
                case ItemType.ClInclude:
                    return "ClInclude";
                default:
                    throw new Exception();
            }
        }
    }
}
