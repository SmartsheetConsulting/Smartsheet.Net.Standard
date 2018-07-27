using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities {
    public class CrossSheetReference : ISmartsheetObject {
        public long Id { get; set; }
        public long EndColumnId { get; set; }
        public long EndRowId { get; set; }
        public long SourceSheetId { get; set; }
        public long StartColumnId { get; set; }
        public long StartRowId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
