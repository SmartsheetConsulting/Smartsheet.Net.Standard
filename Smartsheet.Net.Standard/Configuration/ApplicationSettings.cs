﻿using System;
namespace Smartsheet.Net.Standard.Configuration
{
	public class ApplicationSettings
	{
		public ApplicationSettings()
		{
		}

		public SmartsheetCredentials SmartsheetCredentials { get; set; }
		public SmartsheetConfiguration SmartsheetConfiguration { get; set; }
	}
}
