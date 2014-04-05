// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using System.Reflection;
using System.Diagnostics;
using Support.VisualStudio;

namespace VisualStudio.Probe
{
    class Program
    {
        private static DTE2 _dte;

        static void Main(string[] args)
        {
            DTEWrapper wrapper = DTEInstanceManager.GetExternalDTE(DTEVersion.VS10, "/rootsuffix Exp");
            _dte = wrapper.DTE;
            GC.KeepAlive(wrapper);

#if !VISIBLE
            MakeVisible();
#endif
            DoWork();
        }

        private static void DoWork()
        {
            Solution2 solution = (Solution2)_dte.Solution;
            // create a new solution
            solution.Create(@"C:\Temp\", "NewSolution");
            solution.AddFromTemplate(@"C:\Users\ceztko\Documents\GitHub\SideBuilder\Resources\TemplatesVS10\Win32Dll\MyTemplate.vstemplate", @"C:\Temp", "Test");
            solution.SaveAs(@"C:\Temp\NewSolution.sln");
        }

        private static void MakeVisible()
        {
            // Display the Visual Studio IDE.
            _dte.MainWindow.Activate();
            _dte.MainWindow.Visible = true;
        }
    }
}
