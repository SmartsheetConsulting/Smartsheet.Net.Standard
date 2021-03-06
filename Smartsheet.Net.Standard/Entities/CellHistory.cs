using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class CellHistory : Cell
    {
        public DateTime ModifiedAt { get; set; }
        public User ModifiedBy { get; set; }
    }
}