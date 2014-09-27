// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSBuild.Support
{
    public enum KnownEnvironmentProperties
    {
        MSBuildExtensionsPath32,
        MSBuildExtensionsPath64,
        MSBuildExtensionsPath,
        MSBuildUserExtensionsPath,
        VisualStudioDir
    }

    public enum ItemType
    {
        Unsupported = 0,
        ClCompile,
        ClInclude
    }

    public enum KnownItemDefinitions
    {
        ClCompile
    }

    public enum KnownItems
    {
        ClCompile,
        ClInclude
    }

    public enum KnwownClCompileMetatada
    {
        PrecompiledHeader,
        PrecompiledHeaderFile,
        WarningLevel,
        Optimization,
        PreprocessorDefinitions
    }

    public enum ElementType
    {
        Unsupported,
        ItemDefinitionGroup,
        ItemGroup,
        PropertyGroup
    }

    public static class Extension
    {
        public static string ConvertToString(this KnownEnvironmentProperties prop)
        {
            switch (prop)
            {
                case KnownEnvironmentProperties.MSBuildExtensionsPath32:
                    return "MSBuildExtensionsPath32";
                case KnownEnvironmentProperties.MSBuildExtensionsPath64:
                    return "MSBuildExtensionsPath64";
                case KnownEnvironmentProperties.MSBuildExtensionsPath:
                    return "MSBuildExtensionsPath";
                case KnownEnvironmentProperties.MSBuildUserExtensionsPath:
                    return "MSBuildUserExtensionsPath";
                case KnownEnvironmentProperties.VisualStudioDir:
                    return "VisualStudioDir";
                default:
                    throw new Exception();
            }
        }
    }
}
