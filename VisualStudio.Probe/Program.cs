// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;

namespace VisualStudio.Probe
{
    class Program
    {
        private static DTE2 _dte;

        static void Main(string[] args)
        {
            Type type = Type.GetTypeFromProgID(GetProgId(DTEVersion.VS10), true);
            object obj = Activator.CreateInstance(type, true);
            _dte = obj as DTE2;

            // Register the IOleMessageFilter to handle any threading errors.
            MessageFilter.Register();

#if VISIBLE
            MakeVisible();
#endif

            DoWork();

            _dte.Quit();

            // and turn off the IOleMessageFilter.
            MessageFilter.Revoke();
        }

        private static void DoWork()
        {
            Solution2 solution = (Solution2)_dte.Solution;
            // create a new solution
            solution.Create(@"C:\Temp\", "NewSolution");

            solution.AddFromTemplate(@"C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcprojects\Win32Wiz.vsz", @"C:\Temp", "Test");
            solution.SaveAs(@"C:\Temp\NewSolution.sln");
        }

        private static void MakeVisible(DTE2 dte)
        {
            // Display the Visual Studio IDE.
            dte.MainWindow.Activate();
            dte.MainWindow.Visible = true;
        }

        public static string GetProgId(DTEVersion version)
        {
            switch (version)
            {
                case DTEVersion.Latest:
                    return "VisualStudio.DTE";
                case DTEVersion.VS10:
                    return "VisualStudio.DTE.10.0";
                case DTEVersion.VS11:
                    return "VisualStudio.DTE.10.0";
                case DTEVersion.VS12:
                    return "VisualStudio.DTE.12.0";
                default:
                    throw new Exception();
            }
        }

        public enum DTEVersion
        {
            Latest,
            VS10,
            VS11,
            VS12
        }
    }
}
