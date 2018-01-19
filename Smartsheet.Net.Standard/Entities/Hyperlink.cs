using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class Hyperlink : ISmartsheetObject
    {
        public long? SheetId { get; set; }
        public string Url { get; set; }
    }
}
