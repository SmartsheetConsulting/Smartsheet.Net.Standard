using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Responses
{
    public class BulkItemFailureResponse : ISmartsheetResult
    {
        public long Index { get; set; }
        public long RowId { get; set; }
        public ErrorResponse Error { get; set; }
    }
}
