using System;
using System.Collections.Generic;
using Smartsheet.NET.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Responses
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
