﻿using System.Collections.Generic;
using Smartsheet.Net.Standard.Definitions;
using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
	public class Column : ISmartsheetObject
	{
		public Column()
		{
			this.Options = new List<string>();
			this.Tags = new List<string>();
		}

		public Column Build()
		{
			this.Tags = null;
            this.Options = this.Options.Count > 0 ? this.Options : null;  

			return this;
		}

		public Column Build(
			string title,
			bool isPrimary,
			ColumnType type,
			string symbol = null,
			ICollection<string> options = null,
			string systemColumnType = null,
			dynamic autoNumberFormat = null,
			dynamic width = null)
		{
			this.Title = title;
			this.Primary = isPrimary;
			this.Type = Enum.GetName(typeof(ColumnType), type);
			this.Symbol = symbol;
			this.Options = options;
			this.SystemColumnType = systemColumnType;
			this.AutoNumberFormat = autoNumberFormat;
			this.Width = width;
			this.Tags = null;

			return this;
		}

		public long? Id { get; set; }
		public long? Index { get; set; }
		public long? Width { get; set; }

		public string Title { get; set; }
		public string Type { get; set; }
		public string Symbol { get; set; }
		public string SystemColumnType { get; set; }
		public string Format { get; set; }

		public bool? Primary { get; set; }
		public bool? Hidden { get; set; }
		public bool? Locked { get; set; }
		public bool? LockedForUser { get; set; }

		public AutoNumberFormat AutoNumberFormat { get; set; }
		public Filter Filter { get; set; }

		public ICollection<ContactOptions> ContactOptions { get; set; }
		public ICollection<string> Options { get; set; }
		public ICollection<string> Tags { get; set; }
	}
}
