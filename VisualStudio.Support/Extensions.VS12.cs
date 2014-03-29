// Copyright (c) 2014 Francesco Pretto. Subject to the MIT license

using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudio.Support
{
    public partial class Extensions
    {
        public static IVsBrowseObjectContext GetBrowseObjectContext(this VCConfiguration configuration)
        {
            return configuration as IVsBrowseObjectContext;
        }

        public static IVsBrowseObjectContext GetBrowseObjectContext(this VCProject project)
        {
            return project as IVsBrowseObjectContext;
        }
    }
}
