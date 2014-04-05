using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;

namespace Support.VisualStudio
{
    public class DTEWrapper : IDisposable
    {
        private bool _filterRegistered;
        private DTE2 _DTE;

        public DTEWrapper(DTE2 dte)
        {
            _filterRegistered = false;
            _DTE = dte;
        }

        public DTE2 DTE
        {
            get
            {
                if (!_filterRegistered)
                {
                    MessageFilter.Register();
                    _filterRegistered = true;
                }
                return _DTE;
            }
        }

        public void Dispose()
        {
            _DTE.Quit();
            if (_filterRegistered)
                MessageFilter.Revoke();
        }
    }
}
