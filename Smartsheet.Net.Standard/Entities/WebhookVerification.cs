﻿using System;
namespace Smartsheet.NET.Standard.Entities
{
	public class WebhookVerification
	{
		public WebhookVerification()
		{
		}

		public string WebhookId { get; set; }
		public string ChangeAgent { get; set; }
	}
}
