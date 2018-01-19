using Smartsheet.NET.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Responses
{
    public class RowMapping : ISmartsheetResult
    {
        public RowMapping()
        {
        }

        public long From { get; set; }
        public long To { get; set; }
    }
}
