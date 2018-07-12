using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Responses
{
    public class CopyOrMoveRowDestination : ISmartsheetResult
    {
        public CopyOrMoveRowDestination()
        {
        }

        public long? SheetId { get; set; }
    }
}
