using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Responses
{
    public class ErrorResponse : ISmartsheetResult
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
