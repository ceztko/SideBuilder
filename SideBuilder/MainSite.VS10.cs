// Copyright (c) 2014 Francesco Pretto. Subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VisualStudio.Support;
using Microsoft.VisualStudio.VCProjectEngine;
using Microsoft.VisualStudio.Project.Framework.INTERNAL.VS2010ONLY;
using BuildProject = Microsoft.Build.Evaluation.Project;
using DTEProject = EnvDTE.Project;
using Microsoft.VisualStudio.Project.Contracts.INTERNAL.VS2010ONLY;
using Microsoft.Build.Evaluation;

namespace SideBuilder
{
    public partial class MainSite
    {
        public static void Foo()
        {
            StreamWriter writer = new StreamWriter(@"C:\users\ceztko\desktop\test.txt");
            foreach (DTEProject project in _DTE2.Solution.Projects.AllProjects())
            {
                VCProject vcproject = project.Object as VCProject;
                if (vcproject == null)
                    continue;

                foreach (VCConfiguration configuration in vcproject.Configurations)
                {
                    ConfiguredProject confproj = configuration.GetConfiguredProject();
                    using (var wrapper = confproj.GetProjectService().GlobalCheckout(false))
                    {
                        BuildProject buildproj = wrapper.Project;
                    }

                    IProjectPropertiesProvider prov = confproj.GetProjectPropertiesProvider();

                    prov.ProjectPropertyChanged += new EventHandler<ProjectPropertyChangedEventArgs>(prov_ProjectPropertyChanged);

                    //writer.WriteLine(configuration.Name);
                }

                continue;

                foreach (VCFile file in vcproject.Files)
                {
                    foreach (object obj in file.FileConfigurations)
                    {

                    }

                    writer.WriteLine(file.SubType);
                    writer.WriteLine(file.ItemType);
                    writer.WriteLine(file.Name);
                    writer.WriteLine(file.Kind);
                }

                break;
            }
            writer.Close();
        }

        static void prov_ProjectPropertyChanged(object sender, ProjectPropertyChangedEventArgs e)
        {

        }

        void foo()
        {
            ProjectCollection.GlobalProjectCollection.ProjectCollectionChanged += new EventHandler<ProjectCollectionChangedEventArgs>(GlobalProjectCollection_ProjectCollectionChanged);
            ProjectCollection.GlobalProjectCollection.ProjectXmlChanged += new EventHandler<ProjectXmlChangedEventArgs>(GlobalProjectCollection_ProjectXmlChanged);
            ProjectCollection.GlobalProjectCollection.ProjectChanged += new EventHandler<ProjectChangedEventArgs>(GlobalProjectCollection_ProjectChanged);
        }

        void GlobalProjectCollection_ProjectChanged(object sender, ProjectChangedEventArgs e)
        {
        }

        void GlobalProjectCollection_ProjectXmlChanged(object sender, ProjectXmlChangedEventArgs e)
        {
        }

        void GlobalProjectCollection_ProjectCollectionChanged(object sender, ProjectCollectionChangedEventArgs e)
        {
        }

    }
}
