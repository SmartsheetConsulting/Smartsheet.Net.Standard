using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class UserSettings : ISmartsheetObject
    {
        public bool CriticalPathEnabled { get; set; }
        public bool DisplaySummaryTasks { get; set; }
    }
}
