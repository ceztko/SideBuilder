// Copyright (c) 2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using EnvDTE80;
using EnvDTE;
using System.Diagnostics;
using W32Process = System.Diagnostics.Process;
using Microsoft.VisualStudio.OLE.Interop;
using W32Thread = System.Threading.Thread;

namespace Support.VisualStudio
{
    // http://social.msdn.microsoft.com/Forums/vstudio/en-US/3120db69-a89c-4545-874f-2d61c9317c8a/is-it-possible-to-get-the-dte2-com-object-when-starting-devenvexe-with-parameters?forum=vsx, Leonard Jiang, PD
    // http://www.codeproject.com/Articles/7984/Automating-a-specific-instance-of-Visual-Studio-NE, Mohamed Hendawi, PD
    public class DTEInstanceManager
    {
        [DllImport("ole32.dll", SetLastError = true)]
        static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        [DllImport("ole32.dll", SetLastError = true)]
        static extern int CreateBindCtx(uint reserved, out IBindCtx pctx);

        public static DTEWrapper GetExternalDTE(DTEVersion version)
        {
            Type type = Type.GetTypeFromProgID(GetProgId(version), true);
            DTE2 dte = Activator.CreateInstance(type, true) as DTE2;
            return new DTEWrapper(dte);
        }

        public static DTEWrapper GetExternalDTE(DTEVersion version, string parameter, int timeout = -1)
        {
            string progid = GetProgId(version);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = CLSIDUtils.GetLocalServerFromProgID(progid);
            startInfo.Arguments = parameter;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            string test = CLSIDUtils.GetClassIDFromProgID(GetProgId(version));

            W32Process process = new W32Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForInputIdle();
            // Example: "!VisualStudio.DTE.10.0:3972"
            string vsmonikername = "!" + progid + ":" + process.Id;

            DateTime start = DateTime.Now;
            TimeSpan elapsed;
            DTEWrapper ret = null;
            do
            {
                IRunningObjectTable prot;
                IBindCtx pctx;
                IEnumMoniker penummoniker;

                GetROTEnumerator(out prot, out pctx, out penummoniker);
                IMoniker[] monikers = new IMoniker[1];
                uint pfeteched;
                while (penummoniker.Next(1, monikers, out pfeteched) == 0)
                {
                    String displayname;
                    monikers[0].GetDisplayName(pctx, null, out displayname);

                    if (displayname == vsmonikername)
                    {
                        object punkObject;
                        prot.GetObject(monikers[0], out punkObject);

                        ret = new DTEWrapper((DTE2)punkObject);
                        break;
                    }
                }

                Marshal.ReleaseComObject(prot);
                Marshal.ReleaseComObject(pctx);
                Marshal.ReleaseComObject(penummoniker);

                elapsed = DateTime.Now - start;
                if (ret != null || process.HasExited || (timeout > 0 && elapsed.TotalMilliseconds > timeout))
                    break;
                else
                    W32Thread.Sleep(50);
            }
            while (true);

            return ret;
        }

        /// <summary>Get a table of the currently running instances
        /// of the Visual Studio .NET IDE</summary>
        /// <param name="openSolutionsOnly">Only return instances
        /// that have opened a solution</param>
        /// <returns>A dictionary mapping the name of the IDE
        ///  in the running object table to the corresponding
        ///  DTE object</returns>
        public static Dictionary<string, DTE2> GetIDEInstances(bool openSolutionsOnly)
        {
            Dictionary<string, DTE2> runningIDEInstances = new Dictionary<string, DTE2>();
            Dictionary<string, object> runningObjects = GetRunningObjectTable();

            foreach (KeyValuePair<string, object> pair in runningObjects)
            {
                if (!pair.Key.StartsWith("!VisualStudio.DTE"))
                    continue;

                DTE2 ide = pair.Value as DTE2;
                if (ide == null)
                    continue;

                if (openSolutionsOnly)
                {
                    Solution solution = ide.Solution;
                    string solutionFile = solution == null ? null : solution.FullName;
                    if (!String.IsNullOrEmpty(solutionFile))
                        runningIDEInstances[pair.Key] = ide;
                }
                else
                {
                    runningIDEInstances[pair.Key] = ide;
                }
            }
            return runningIDEInstances;
        }

        /// <summary>
        /// Get a snapshot of the running object table (ROT).
        /// </summary>
        /// <returns>A hashtable mapping the name of the object
        /// in the ROT to the corresponding object</returns>
        public static Dictionary<string, object> GetRunningObjectTable()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            IRunningObjectTable prot;
            IBindCtx pctx;
            IEnumMoniker penummoniker;
            GetROTEnumerator(out prot, out pctx, out penummoniker);

            IMoniker[] monikers = new IMoniker[1];
            uint pfeteched;
            while (penummoniker.Next(1, monikers, out pfeteched) == 0)
            {
                String displayname;
                monikers[0].GetDisplayName(pctx, null, out displayname);

                object runningObjectVal;
                prot.GetObject(monikers[0], out runningObjectVal);

                result[displayname] = runningObjectVal;
            }

            return result;
        }
        
        private static void GetROTEnumerator(out IRunningObjectTable prot,
            out IBindCtx pctx, out IEnumMoniker penummoniker)
        {
            int hr = GetRunningObjectTable(0, out prot);
            Marshal.ThrowExceptionForHR(hr);

            hr = CreateBindCtx(0, out pctx);
            Marshal.ThrowExceptionForHR(hr);

            prot.EnumRunning(out penummoniker);
            penummoniker.Reset();
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
    }

    public enum DTEVersion
    {
        Latest,
        VS10,
        VS11,
        VS12
    }
}
