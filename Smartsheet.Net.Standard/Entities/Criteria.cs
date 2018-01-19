using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class Criteria : ISmartsheetObject
    {
        public string Operator { get; set; }
        public dynamic Value1 { get; set; }
        public dynamic Value2 { get; set; }
    }
}
