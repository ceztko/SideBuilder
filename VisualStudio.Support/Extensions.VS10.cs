// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.Build.Evaluation;
using System.Reflection;
using Microsoft.VisualStudio.Project.Framework.INTERNAL.VS2010ONLY;
using Microsoft.VisualStudio.Project.Contracts.INTERNAL.VS2010ONLY;

namespace VisualStudio.Support
{
    extern alias VC;
    using VCProjectShim = VC::Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim;
    using VCConfigurationShim = VC::Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCConfigurationShim;
    using Microsoft.VisualStudio.Text;

    public static partial class Extensions
    {
        static PropertyInfo _PropGetter;

        static Extensions()
        {
            _PropGetter = typeof(VCConfigurationShim).GetProperty("ConfiguredProject", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static ITextDocument GetTextDocument(this ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            bool success = textBuffer.Properties.TryGetProperty<ITextDocument>(
              typeof(ITextDocument), out textDocument);
            if (success)
                return textDocument;
            else
                return null;
        }

        public static ConfiguredProject GetConfiguredProject(this VCConfiguration configuration)
        {
            return _PropGetter.GetValue(configuration, null) as ConfiguredProject;
        }

        public static MSBuildProjectService GetProjectService(this ConfiguredProject confproj)
        {
            return confproj.GetServiceFeature<MSBuildProjectService>();
        }

        public static IProjectPropertiesProvider GetProjectPropertiesProvider(this ConfiguredProject project)
        {
            return GetFeature <IProjectPropertiesProvider>(project, "Name", "ProjectFile");
        }

        public static IProjectPropertiesProvider GetUserPropertiesProvider(this ConfiguredProject project)
        {
            return GetFeature<IProjectPropertiesProvider>(project, "Name", "UserFile");
        }

        public static T GetFeature<T>(IProjectContractQuery contract, string metadataName, string metadataValue)
        {
            Lazy<T, IDictionary<string, object>> export = FindExport(contract.GetFeature<T, IDictionary<string, object>>(), metadataName, metadataValue);
            return export.Value;
        }
    }
}
