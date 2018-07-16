using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Hyperlink : ISmartsheetObject
    {
        public long? ReportId { get; set; }
        public long? SheetId { get; set; }
        public long? SightId { get; set; }
        public string Url { get; set; }
    }
}
