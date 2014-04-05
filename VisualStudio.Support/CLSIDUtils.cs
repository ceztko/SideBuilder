using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Support.VisualStudio
{
    // http://stackoverflow.com/questions/10214183/get-dll-directory-from-progid richard druce
    public static class CLSIDUtils
    {
        public static string GetLocalServerFromProgID(string progID)
        {
            var classID = GetClassIDFromProgID(progID);
            var fileName = GetLocalServerFromClassID(classID);
            return fileName;
        }

        public static string GetLocalServerFromClassID(string classID)
        {
            var regPath = @"\CLSID\" + classID + @"\LocalServer32\";
            return GetDefaultRegistryValue(Registry.ClassesRoot, regPath);
        }

        public static string GetClassIDFromProgID(string progID)
        {
            var regPath = progID + @"\CLSID\";
            return GetDefaultRegistryValue(Registry.ClassesRoot, regPath);
        }

        private static string GetDefaultRegistryValue(RegistryKey rootKey, string regPath)
        {
            using (var regKey = rootKey.OpenSubKey(regPath))
            {
                if (regKey == null)
                    return null;
                else
                    return (string)regKey.GetValue("");
            }
        }
    }
}
