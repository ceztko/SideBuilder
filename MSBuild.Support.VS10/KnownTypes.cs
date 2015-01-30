// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Construction;

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
        ExtensionsElement,
        ImportElement,
        MetadataElement,
        OnErrorElement,
        OutputElement,
        PropertyElement,
        UsingTaskBodyElement,
        UsingTaskParameterElement,
        ChooseElement,
        ImportGroupElement,
        ItemDefinitionElement,
        ItemDefinitionGroupElement,
        ItemElement,
        ItemGroupElement,
        OtherwiseElement,
        PropertyGroupElement,
        RootElement,
        TargetElement,
        TaskElement,
        UsingTaskElement,
        WhenElement,
        UsingTaskParameterGroupElement,
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

        public static ElementType GetElementType(this ProjectElement element)
        {
            switch (element.GetType().Name)
            {
                case "ProjectExtensionsElement":
                    return ElementType.ExtensionsElement;
                case "ProjectImportElement":
                    return ElementType.ImportElement;
                case "ProjectMetadataElement":
                    return ElementType.MetadataElement;
                case "ProjectOnErrorElement":
                    return ElementType.OnErrorElement;
                case "ProjectOutputElement":
                    return ElementType.OutputElement;
                case "ProjectPropertyElement":
                    return ElementType.PropertyElement;
                case "ProjectUsingTaskBodyElement":
                    return ElementType.UsingTaskBodyElement;
                case "ProjectUsingTaskParameterElement":
                    return ElementType.UsingTaskParameterElement;
                case "ProjectChooseElement":
                    return ElementType.ChooseElement;
                case "ProjectImportGroupElement":
                    return ElementType.ImportGroupElement;
                case "ProjectItemDefinitionElement":
                    return ElementType.ItemDefinitionElement;
                case "ProjectItemDefinitionGroupElement":
                    return ElementType.ItemDefinitionGroupElement;
                case "ProjectItemElement":
                    return ElementType.ItemElement;
                case "ProjectItemGroupElement":
                    return ElementType.ItemGroupElement;
                case "ProjectOtherwiseElement":
                    return ElementType.OtherwiseElement;
                case "ProjectPropertyGroupElement":
                    return ElementType.PropertyGroupElement;
                case "ProjectRootElement":
                    return ElementType.RootElement;
                case "ProjectTargetElement":
                    return ElementType.TargetElement;
                case "ProjectTaskElement":
                    return ElementType.TaskElement;
                case "ProjectUsingTaskElement":
                    return ElementType.UsingTaskElement;
                case "ProjectWhenElement":
                    return ElementType.WhenElement;
                case "UsingTaskParameterGroupElement":
                    return ElementType.UsingTaskParameterGroupElement;
                default:
                    return ElementType.Unsupported;
            }
        }
    }
}
