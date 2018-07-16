using Smartsheet.Net.Standard.Interfaces;
using System.Collections.Generic;

namespace Smartsheet.Net.Standard.Responses
{
    public class ResultResponse<T> : ISmartsheetResult
    {
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
        public int Version { get; set; }
        public ICollection<BulkItemFailureResponse> FailedItems { get; set; }
    }
}
