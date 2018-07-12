using System;
using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Responses
{
    public class CopyOrMoveRowResult : ISmartsheetResult
    {
        public CopyOrMoveRowResult()
        {
        }

        public long DestinationSheetId { get; set; }
        public ICollection<RowMapping> RowMappings { get; set; }
    }
}
