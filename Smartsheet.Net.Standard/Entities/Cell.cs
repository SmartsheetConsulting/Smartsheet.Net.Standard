using System;
using Newtonsoft.Json;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{ 
    public enum CellType
    {
        Standard,
        Formula
    }

	public class Cell : ISmartsheetObject
	{
		public Cell()
		{
			this.Column = new Column();
		}

		public Cell(long? columnId, dynamic value, CellType cellType = CellType.Standard)
		{
			this.ColumnId = columnId;

            switch(cellType)
            {
                case CellType.Standard:
                    this.Value = value;
                    this.Formula = null;
                    break;
                case CellType.Formula:
                    if (!Convert.ToString(value).Contains("="))
                    {
                        throw new Exception("Cannot create a formula that does not begin with '=");
                    }
                    this.Formula = value;
                    this.Value = null;
                    break;
            }
		}

		public Cell Build()
		{
			this.Column = null;

			return this;
		}

		public Cell Build(bool? strict)
		{
			this.Column = null;
			this.Strict = strict;

			if (this.Hyperlink != null)
			{
				this.Hyperlink.Url = null;
			}

            if (!string.IsNullOrWhiteSpace(this.Formula))
            {
                this.Value = null;
            }

			return this;
		}

		public long? ColumnId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
		public dynamic Value { get; set; }

		public string DisplayValue { get; set; }

        public string Formula { get; set; }

		public bool? Strict { get; set; }

		public Hyperlink Hyperlink { get; set; }

		public Column Column { get; set; }

		public CellLink LinkInFromCell { get; set; }
	}
}
