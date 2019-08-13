using System;
using Smartsheet.Net.Standard.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smartsheet.Net.Standard.Entities;
using System.Net.Http;
using Smartsheet.Net.Standard.Definitions;
using Smartsheet.Net.Standard.Responses;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Smartsheet.Net.Standard.Interfaces
{
	public interface ISmartsheetHttpClient
	{
		//	Root Request Exection
		Task<TResult> ExecuteRequest<TResult, T>(HttpVerb verb, string url, T data, string accessToken = null, FormUrlEncodedContent content = null);

		//	Authorization
		Task<HttpResponseMessage> RequestAuthorizationFromEndUser(string url, string clientId, string scopes, string state = "");
		Task<Token> ObtainAccessToken(string url, string code, string clientId, string clientSecret, string redirectUri = "");
		Task<Token> RefreshAccessToken(string url, string refreshToken, string clientId, string clientSecret, string redirectUri = "");
		Task<User> GetCurrentUserInformation(string accessToken);

		//	Workspaces
		Task<Workspace> CreateWorkspace(string workspaceName, string accessToken = null);
		Task<Workspace> GetWorkspaceById(long? workspaceId, string accessToken = null, bool loadAll = false);
		Task<IEnumerable<Workspace>> ListWorkspaces(string accessToken = null);
		
		//  Shares
		Task<IEnumerable<Share>> ShareWorkspace(long? workspaceId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null);
		Task<IEnumerable<Share>> ShareReport(long? reportId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null);
		Task<IEnumerable<Share>> ShareSheet(long? sheetId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null);
		Task<IEnumerable<Share>> ShareSight(long? sightId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null);
		//	Sheets
		Task<Sheet> GetSheetById(long? sheetId, string accessToken = null, string [] options = null);
		Task<Sheet> CreateSheet(string sheetName, IEnumerable<Column> columns, string folderId = null, string workspaceId = null, string accessToken = null);
		Task<ResultResponse> DeleteSheet(long? sheetId, string accessToken = null);
		Task<Sheet> CreateSheetFromTemplate(string sheetName, long? templateId, long? folderId = null, long? workspaceId = null, string accessToken = null);
		Task<Sheet> CopySheet(string newName, long? sourceSheetId, long? destinationId, DestinationType destinationType, IEnumerable<SheetCopyInclusion> includes, string accessToken = null);
		Task<IEnumerable<Sheet>> GetSheetsForWorkspace(long? workspaceId, string accessToken = null);
		Task<Sheet> UpdateSheet(long? sheetId, Sheet sheet, string accessToken = null);
		Task<IEnumerable<Sheet>> ListSheets(string accessToken = null);
		Task<IEnumerable<Sheet>> ListAllSheetsAndVersions(string accessToken = null);
		Task<Stream> GetSheetAsExcel(long? sheetId);
		Task<Stream> GetSheetAsPdf(long? sheetId, PaperSize? paperSize);
		Task<Stream> GetSheetAsCsv(long? sheetId);
		
		//	Rows
		Task<IEnumerable<Row>> CreateRows(long? sheetId, IEnumerable<Row> rows, bool? toTop = null, bool? toBottom = null, long? parentId = null, long? siblingId = null, string accessToken = null);
		Task<CopyOrMoveRowResult> MoveRows(long? sourceSheetId, long? destinationSheetId, IEnumerable<long> rowIds, string accessToken = null);
		Task<CopyOrMoveRowResult> CopyRows(long? sourceSheetId, long? destinationSheetId, IEnumerable<long> rowIds, string accessToken = null);
		
		//	Folders
		Task<Folder> CopyFolder(long? folderId, long? destinationId, string newName, string accessToken = null);
		Task<IEnumerable<Folder>> GetFoldersForWorkspace(long? workspaceId, string accessToken = null, bool loadAll = false);
		Task<Folder> GetFolderById(long? folderId, string accessToken = null);
		Task<IEnumerable<Row>> LockRows(long? sheetId, bool locked, IEnumerable<long?> rowIds, string accessToken = null);
		
		//	Reports
		Task<IEnumerable<Report>> ListReports(string accessToken = null);
		Task<IEnumerable<ISmartsheetObject>> GetReportsForWorkspace(long? workspaceId, string accessToken = null);

		//	Sights
		Task<IEnumerable<Sight>> ListSights(string accessToken = null);
		
		//	Templates
		Task<IEnumerable<Template>> ListTemplates(string accessToken = null);
		Task<IEnumerable<ISmartsheetObject>> GetTemplatesForWorkspace(long? workspaceId, string accessToken = null);

		//	Update Requests
		Task<UpdateRequest> CreateUpdateRequest(long? sheetId, IEnumerable<long> rowIds, IEnumerable<Recipient> sendTo, IEnumerable<long> columnIds, string subject = null, string message = null, bool ccMe = false, bool includeDiscussions = true, bool includeAttachments = true, string accessToken = null);

		//	Send Rows
		Task<MultiRowEmail> CreateSendRow(long? sheetId, MultiRowEmail email, string accessToken = null);

		//	Webhooks
		Task<IEnumerable<Webhook>> GetWebhooksForUser(string accessToken = null, bool includeAll = false);
		Task<Webhook> GetWebhook(long? webhookId, string accessToken = null);
		Task<Webhook> CreateWebhook(Webhook model, string accessToken = null);
		Task<Webhook> UpdateWebhook(long? webhookId, Webhook model, string accessToken = null);
		Task<Webhook> DeleteWebhook(long? webhookId, string accessToken = null);

		//	Columns
		Task<Column> EditColumn(long? sheetId, long? columnId, Column model, string accessToken = null);
		Task<Column> CreateColumn(long? sheetId, Column model, string accessToken = null);
		Task<ResultResponse> DeleteColumn(long? sheetId, long? columnId, string accessToken = null); 
		
		//Cell History
		Task<CellHistory> GetCellHistory(long? sheetId, long? rowId, long? columnId, string accessToken = null);

		//  Attachments
		[Obsolete("UploadAttachmentToRow is deprecated. Use AttachFileToRow.")]
		Task<Attachment> UploadAttachmentToRow(long? sheetId, long? rowId, string fileName, long length, Stream data, string contentType = null, string accessToken = null);
		[Obsolete("UploadAttachmentToRow is deprecated. Use AttachFileToRow.")]
		Task<Attachment> UploadAttachmentToRow(long? sheetId, long? rowId, IFormFile formFile, string accessToken = null);

		Task<Attachment> AttachFileToRow(long? sheetId, long? rowId, string fileName, long length, Stream stream, string contentType = null, string accessToken = null);
		Task<Attachment> AttachFileToRow(long? sheetId, long? rowId, IFormFile formFile, string accessToken = null);

		Task<Attachment> AttachFileToSheet(long? sheetId, string fileName, long length, Stream stream, string contentType = null, string accessToken = null);
		Task<Attachment> AttachFileToSheet(long? sheetId, IFormFile formFile, string accessToken = null);

		Task<Attachment> AttachUrlToRow(long? sheetId, long? rowId, string url, string name, string description, string attachmentType, string attachmentSubType, string accessToken = null);

		Task<Attachment> AttachUrlToRow(long? sheetId, long? rowId, Attachment attachment, string accessToken = null);

		Task<Attachment> AttachUrlToSheet(long? sheetId, string url, string name, string description, string attachmentType, string attachmentSubType, string accessToken = null);

		Task<Attachment> AttachUrlToSheet(long? sheetId, Attachment attachment, string accessToken = null);
		Task<IEnumerable<Attachment>> ListAttachments(long? sheetId, string accessToken = null);
		Task<Attachment> GetAttachment(long? sheetId, long? attachmentId, string accessToken = null);
		Task<ResultResponse> DeleteAttachment(long? sheetId, long? attachmentId, string accessToken = null);
		
		//	Discussions
		Task<Discussion> CreateDiscussionOnRow(long? sheetId, long? rowId, string commentText,
			string accessToken = null);
		Task<Discussion> CreateDiscussionOnSheet(long? sheetId, string commentText, string accessToken = null);
		Task<ResultResponse> DeleteDiscussion(long? sheetId, long? discussionId, string accessToken = null);
		Task<IEnumerable<Discussion>> ListDiscussions(long? sheetId, bool includeAll = false,
			bool includeComments = false, bool includeAttachments = false, string accessToken = null);
		Task<Discussion> GetDiscussion(long? sheetId, long? discussionId, string accessToken = null);
		Task<IEnumerable<Discussion>> ListRowDiscussions(long? sheetId, long? rowId, bool includeAll = false,
			bool includeComments = false, bool includeAttachments = false, string accessToken = null);
		
		//	Comments
		Task<Comment> AddComment(long? sheetId, long? discussionId, string commentText, string accessToken = null);
		Task<Comment> EditComment(long? sheetId, long? commentId, string commentText, string accessToken = null);
		Task<ResultResponse> DeleteComment(long? sheetId, long? commentId, string accessToken = null);
		Task<Comment> GetComment(long? sheetId, long? commentId, string accessToken = null);
		
		//	Cross sheet refs
		Task<IEnumerable<CrossSheetReference>> ListCrossSheetReferences(long? sheetId, string accessToken = null);
		Task<CrossSheetReference> CreateCrossSheetReference(long? sheetId, CrossSheetReference crossSheetReference, string accessToken = null);
		
		//	Users
		Task<User> GetCurrentUser(string accessToken = null);
		Task<Home> GetHome(string accessToken = null);
		Task<IEnumerable<User>> ListUsers(string accessToken = null, bool includeAll = false);
		Task<User> AddUser(User user, string accessToken = null);
		Task<ResultResponse> RemoveUser(long userID, string transferTo = null, bool transferSheets = false, bool removeFromSharing = false, string accessToken = null);
		Task<User> UpdateUser(long userID, bool admin, bool licensedSheetCreator, string firstName, string lastName, bool groupAdmin, bool resourceViewer, string accessToken = null);
		
		//	Groups
		Task<IEnumerable<Group>> ListOrgGroups(string accessToken = null, bool includeAll = false);
		Task<Group> CreateGroup(string groupName, string description = null, List<GroupMember> members = null, string accessToken = null);
		Task<ResultResponse> DeleteGroup(long groupId, string accessToken = null);
		Task<Group> GetGroup(long? groupId, string accessToken = null);
		Task<Group> UpdateGroup(long groupId, string groupName = null, string description = null, long? ownerId = null, string accessToken = null);
		Task<IEnumerable<GroupMember>> AddGroupMembers(long groupId, List<GroupMember> newMembers = null, string accessToken = null);
		Task<ResultResponse> RemoveGroupMember(long groupId, long userId, string accessToken = null);

	}
}
