using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class AutoNumberFormat : ISmartsheetObject
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Fill { get; set; }
        public long StartingNumber { get; set; }
    }
}
