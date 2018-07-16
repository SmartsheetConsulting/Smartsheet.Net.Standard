using System;
namespace Smartsheet.NET.Standard.Definitions
{
	public enum SheetCopyInclusion
	{
		/// <summary>
		/// Includes the attachments.
		/// </summary>
		Attachments,

		/// <summary>
		/// Includes cell links.
		/// </summary>
		CellLinks,

		/// <summary>
		/// Includes the data with formatting.
		/// </summary>
		Data,

		/// <summary>
		/// Includes the comments.
		/// </summary>
		Discussions,

		/// <summary>
		/// Includes the filters.
		/// </summary>
		Filters,

		/// <summary>
		/// Includes the forms.
		/// </summary>
		Forms,

		/// <summary>
		/// Includes the notification recipients - must also include 'Rules' when using this attribute.
		/// </summary>
		RuleRecipients,

		/// <summary>
		/// Includes the notifications and workflow rules.
		/// </summary>
		Rules,

		/// <summary>
		/// Inclues the sharing permissions.
		/// </summary>
		Shares,

		/// <summary>
		/// (Deprecated) Includes everything.
		/// </summary>
		[Obsolete]
		All,
	}
}
