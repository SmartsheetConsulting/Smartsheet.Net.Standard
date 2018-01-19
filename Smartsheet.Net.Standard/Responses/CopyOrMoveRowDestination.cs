using System;
using Smartsheet.NET.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Responses
{
    public class CopyOrMoveRowDestination : ISmartsheetResult
    {
        public CopyOrMoveRowDestination()
        {
        }

        public long? SheetId { get; set; }
    }
}
