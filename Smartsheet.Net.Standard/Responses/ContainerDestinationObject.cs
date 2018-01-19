using System;
using Smartsheet.NET.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Responses
{
	public class ContainerDestinationObject : ISmartsheetResult
	{
		public ContainerDestinationObject()
		{
		}

		public string NewName { get; set; }
		public string DestinationType { get; set; }
		public Int64 DestinationId { get; set; }
	}
}
