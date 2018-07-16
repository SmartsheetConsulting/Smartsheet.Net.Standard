using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Responses
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
