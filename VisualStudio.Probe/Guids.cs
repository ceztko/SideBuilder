// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

namespace VisualStudio.Probe
{
    static class GuidList
    {
#if VS12
        public const string guidSolutionConfigurationNamePkgString = "";
#else
        public const string VSPackageGuidString = "ff3685a2-6d5c-4627-b185-7050e4078718";
#endif
    };
}