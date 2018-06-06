﻿using System;
namespace Smartsheet.NET.Standard.Entities
{
	public class WebhookCallbackEvent
	{
		public WebhookCallbackEvent()
		{
		}

		public string ObjectType { get; set; }
		public string EventType { get; set; }
		public long? Id { get; set; }
		public long? RowId { get; set; }
		public long? ColumnId { get; set; }
		public long? UserId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
