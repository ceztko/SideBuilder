using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualStudio.Support
{
    // IVsSolution.AdviseSolutionEvents()
    public interface ISolutionLoadEvents : IVsSolutionLoadEvents, IVsSolutionEvents, IVsSolutionEvents2, IVsSolutionEvents3, IVsSolutionEvents4, IVsSolutionEventsProjectUpgrade
    {
    }

    // IVsTrackProjectDocuments2.AdviseTrackProjectDocumentsEvents()
    public interface ITrackProjectDocumentsEvents : IVsTrackProjectDocumentsEvents2, IVsTrackProjectDocumentsEvents3
    {
    }
}
