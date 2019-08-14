using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class CellHistory : ISmartsheetObject
    {
        public long? ColumnId { get; set; }
        public string DisplayValue { get; set; }
        public dynamic Value { get; set; }
        public DateTime ModifiedAt { get; set; }
        public ContactOptions ModifiedBy { get; set; }
    }
}