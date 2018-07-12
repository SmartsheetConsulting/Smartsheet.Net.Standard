using System;
namespace Smartsheet.Net.Standard.Entities
{
    public class CellLink
    {
        public CellLink()
        {
        }

        public string Status { get; set; }
        public long? SheetId { get; set; }
        public long? RowId { get; set; }
        public long? ColumnId { get; set; }
        public string SheetName { get; set; }
    }
}
