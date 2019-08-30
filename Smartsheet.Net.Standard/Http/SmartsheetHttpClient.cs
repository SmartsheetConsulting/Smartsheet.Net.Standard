using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Smartsheet.Net.Standard.Definitions;
using Smartsheet.Net.Standard.Entities;
using Smartsheet.Net.Standard.Interfaces;
using Smartsheet.Net.Standard.Configuration;
using Smartsheet.Net.Standard.Responses;
using Smartsheet.Net.Standard.Hash;
using Smartsheet.Net.Standard.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Smartsheet.Net.Standard.Http
{
    public class SmartsheetHttpClient : ISmartsheetHttpClient
    {
        private HttpClient _HttpClient = new HttpClient();
        public string _AccessToken { get; private set; }
        public string _ChangeAgent { get; private set; }
        private static int _AttemptLimit = 10;
        private int _WaitTime = 0;
        private int _RetryCount = 0;
        private bool _RetryRequest = true;

        #region Client 
        public SmartsheetHttpClient()
        {
            this.InitializeHttpClient();
        }

        public SmartsheetHttpClient(IOptions<ApplicationSettings> options)
        {
            this._AccessToken = options.Value.SmartsheetCredentials.AccessToken;
            this._ChangeAgent = options.Value.SmartsheetCredentials.ChangeAgent;
            this.InitializeHttpClient();
        }

        public SmartsheetHttpClient(string accessToken, string changeAgent)
        {
            this._AccessToken = accessToken;
            this._ChangeAgent = changeAgent;
            this.InitializeHttpClient();
        }

        /// <summary>
        /// Set the base address, and default request headers
        /// for the Http client prior to sending a request.
        /// </summary>
        private void InitializeHttpClient()
        {
            this._HttpClient.BaseAddress = new Uri("https://api.smartsheet.com/2.0/");
            this._HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Sets the authorization header.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        public void SetAuthorizationHeader(string accessToken)
        {
            if (accessToken != null)
            {
                this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            }
            else
            {
                this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._AccessToken);
            }
        }

        /// <summary>
        /// Executes any, and all requests against the Smartsheet API. Additionally,
        /// handles all retry logic and serialization / deserialization of 
        /// requests / responses.
        /// </summary>
        /// <returns>The request.</returns>
        /// <param name="verb">Verb.</param>
        /// <param name="url">URL.</param>
        /// <param name="data">Data.</param>
        /// <param name="accessToken">Smartsheet API access token.</param>
        /// <param name="content">Form URL Encoded Content.</param>
        /// <param name="readAsStream">Read contents of response as stream. Defaults to false.</param>
        /// <typeparam name="TResult">The 1st type parameter.</typeparam>
        /// <typeparam name="T">The 2nd type parameter.</typeparam>
        public async Task<TResult> ExecuteRequest<TResult, T>(HttpVerb verb, string url, T data, string accessToken = null, FormUrlEncodedContent content = null)
        {
            this.ValidateRequestInjectedResult(typeof(TResult));

            if (content == null)
            {
                this._HttpClient.DefaultRequestHeaders.Remove("Authorization");

                this._HttpClient.DefaultRequestHeaders.Remove("Smartsheet-Change-Agent");

                if (accessToken != null)
                {
                    this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                }
                else
                {
                    this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._AccessToken);
                }

                if (this._ChangeAgent != null)
                {
                    this._HttpClient.DefaultRequestHeaders.Add("Smartsheet-Change-Agent", this._ChangeAgent);
                }

                this.ValidateClientParameters();
            }

            this.InitializeNewRequest();

            while (_RetryRequest && (_RetryCount < _AttemptLimit))
            {
                try
                {
                    if (_WaitTime > 0)
                    {
                        Thread.Sleep(_WaitTime);
                    }

                    HttpResponseMessage response;

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var serializedData = JsonConvert.SerializeObject(data, Formatting.None, serializerSettings);

                    switch (verb)
                    {
                        default:
                        case HttpVerb.GET:
                            response = await this._HttpClient.GetAsync(url);
                            break;
                        case HttpVerb.PUT:
                            if (content == null)
                            {
                                response = await this._HttpClient.PutAsync(url, new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json"));
                            }
                            else
                            {
                                response = await this._HttpClient.PutAsync(url, content);
                            }
                            break;
                        case HttpVerb.POST:
                            if (content == null)
                            {
                                response = await this._HttpClient.PostAsync(url, new StringContent(serializedData, System.Text.Encoding.UTF8, "application/json"));
                            }
                            else
                            {
                                response = await this._HttpClient.PostAsync(url, content);
                            }
                            break;
                        case HttpVerb.DELETE:
                            response = await this._HttpClient.DeleteAsync(url);
                            break;
                    }

                    var statusCode = response.StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();

                            var jsonReponseBody = JsonConvert.DeserializeObject(responseBody).ToString();

                            var resultResponse = JsonConvert.DeserializeObject<TResult>(jsonReponseBody);

                            return resultResponse;
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }

                    if (statusCode.Equals(HttpStatusCode.InternalServerError) || statusCode.Equals(HttpStatusCode.ServiceUnavailable) || statusCode.Equals((HttpStatusCode)429)) // .NET doesn't have a const for this
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        dynamic result = JsonConvert.DeserializeObject(responseJson);

                        // did we hit an error that we should retry?
                        int code = result["errorCode"];

                        if (code == 4001)
                        {
                            // service may be down temporarily
                            _WaitTime = Backoff(_WaitTime, 60 * 1000);
                        }
                        else if (code == 4002 || code == 4004)
                        {
                            // internal error or simultaneous update.
                            _WaitTime = Backoff(_WaitTime, 1 * 1000);
                        }
                        else if (code == 4003)
                        {
                            // rate limit
                            _WaitTime = Backoff(_WaitTime, 2 * 1000);
                        }
                    }
                    else
                    {
                        _RetryRequest = false;
                        dynamic result;
                        try
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();

                            result = JsonConvert.DeserializeObject(responseJson);
                        }
                        catch (Exception)
                        {
                            throw new Exception(string.Format("HTTP Error {0}: url:[{1}]", statusCode, url));
                        }

                        var message = string.Format("HTTP Error {0} - Smartsheet error code {1}: {2} url:[{3}]", statusCode, result["errorCode"], result["message"], url);

                        throw new Exception(message);
                    }
                }
                catch (Exception e)
                {
                    if (!_RetryRequest)
                    {
                        throw e;
                    }
                }

                _RetryCount += 1;
            }

            throw new Exception(string.Format("Retries exceeded.  url:[{0}]", url));
        }

        /// <summary>
        /// Backoff the specified current and minimumWait.
        /// </summary>
        /// <returns>The backoff.</returns>
        /// <param name="current">Current.</param>
        /// <param name="minimumWait">Minimum wait.</param>
        private static int Backoff(int current, int minimumWait)
        {
            if (current > 0)
            {
                return current * 2;
            }

            return minimumWait;
        }

        /// <summary>
        /// Validates the request injected result.
        /// </summary>
        /// <param name="type">Type.</param>
        private void ValidateRequestInjectedResult(Type type)
        {
            if (!type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISmartsheetResult)))
            {
                throw new Exception("Injected type must implement interface ISmartsheetResult");
            }
        }

        /// <summary>
        /// Validates the type of the request injected.
        /// </summary>
        /// <param name="type">Type.</param>
        private void ValidateRequestInjectedType(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.GetGenericArguments()[0] != typeof(ISmartsheetObject))
                {
                    throw new Exception("Injected type must implement interface ISmartsheetObject");
                }
            }
            else
            {
                if (!type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISmartsheetObject)))
                {
                    throw new Exception("Injected type must implement interface ISmartsheetObject");
                }
            }
        }

        /// <summary>
        /// Validates the client parameters.
        /// </summary>
        private void ValidateClientParameters()
        {
            if (this._AccessToken == null || string.IsNullOrWhiteSpace(this._AccessToken))
            {
                throw new ArgumentException("Access Token must be provided");
            }
        }

        /// <summary>
        /// Initiazes the new request.
        /// </summary>
        private void InitializeNewRequest()
        {
            this._WaitTime = 0;
            this._RetryCount = 0;
            this._RetryRequest = true;
        }

        #endregion


        #region Authorization
        public async Task<HttpResponseMessage> RequestAuthorizationFromEndUser(string url, string clientId, string scopes, string state = "")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("Provided Smartsheet Api URL cannot be null");
            }

            var paramaters = new Dictionary<string, string>()
            {
                { "response_type", "code" },
                { "client_id", clientId },
                { "state", state },
                { "scope", scopes }
            };

            var uri = QueryHelpers.AddQueryString(url, paramaters);

            var response = await this._HttpClient.GetAsync(uri);

            return response;
        }

        public async Task<Token> ObtainAccessToken(string url, string code, string clientId, string clientSecret, string redirectUri = "")
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new Exception("Provided Smartsheet Code cannot be null");
            }

            var hash = SHA.GenerateSHA256String(clientSecret + "|" + code);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("hash", hash)
            });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            
            var response = await this.ExecuteRequest<Token, Token>(HttpVerb.POST, url, null, content: content);         

            return response;
        }

        public async Task<Token> RefreshAccessToken(string url, string refreshToken, string clientId, string clientSecret, string redirectUri = "")
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new Exception("Provided Smartsheet Refresh Token cannot be null");
            }

            var hash = SHA.GenerateSHA256String(clientSecret + "|" + refreshToken);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("hash", hash)
            });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");         

            var response = await this.ExecuteRequest<Token, Token>(HttpVerb.POST, url, null, content: content);

            return response;
        }

        public async Task<User> GetCurrentUserInformation(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Provided Smartsheet Access Token cannot be null");
            }

            //this._HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await this.ExecuteRequest<User, User>(HttpVerb.GET, string.Format("users/me"), null, accessToken: accessToken);

            return response;
        }

        #endregion


        #region Workspaces
        public async Task<Workspace> CreateWorkspace(string workspaceName, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(workspaceName))
            {
                throw new Exception("Workspace Name cannot be null or blank");
            }

            var workspace = new Workspace(workspaceName);

            var response = await this.ExecuteRequest<ResultResponse<Workspace>, Workspace>(HttpVerb.POST, string.Format("workspaces"), workspace, accessToken: accessToken);

            return response.Result;
        }

        public async Task<Workspace> GetWorkspaceById(long? workspaceId, string accessToken = null, bool loadAll = false)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}?loadAll={1}", workspaceId, loadAll ? "true" : "false"), null, accessToken: accessToken);

            return response;
        }

        public async Task<IEnumerable<Workspace>> ListWorkspaces(string accessToken = null)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Workspace>, Workspace>(HttpVerb.GET, string.Format("workspaces"), null, accessToken: accessToken);
            return response.Data;
        }

        #endregion


        #region Sheets
        
        public async Task<Sheet> CreateSheet(string sheetName, IEnumerable<Column> columns, string folderId = null, string workspaceId = null, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new Exception("Sheet Name cannot be null or blank");
            }

            var sheet = new Sheet(sheetName, columns.ToList());

            var response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("sheets"), sheet, accessToken: accessToken);

            response.Result._Client = this;

            return response.Result;
        }
        
        public async Task<ResultResponse> DeleteSheet(long? sheetId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<ResultResponse, Sheet>(HttpVerb.DELETE, $"sheets/{sheetId}", null, accessToken: accessToken);
            return response;
        }

        public async Task<Sheet> UpdateSheet(long? sheetId, Sheet sheet, string accessToken = null)
        {
            if (sheet == null)
            {
                throw new Exception("Sheet cannot be null or blank");
            }

            var response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.PUT, string.Format("sheets/{0}", sheetId), sheet, accessToken: accessToken);

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<Sheet> CreateSheetFromTemplate(string sheetName, long? templateId, long? folderId = null, long? workspaceId = null, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                throw new Exception("Sheet Name cannot be null or blank");
            }

            if (templateId == null)
            {
                throw new Exception("Template ID cannot be null or blank");
            }

            var sheet = new Sheet(sheetName, null);
            sheet.FromId = templateId;

            var response = new ResultResponse<Sheet>();

            if (folderId == null && workspaceId == null)
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("sheets"), sheet, accessToken: accessToken);
            }
            else if (folderId != null && workspaceId == null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("folders/{0}/sheets?include=data", folderId), sheet, accessToken: accessToken);
            }
            else if (folderId == null && workspaceId != null) // Folders
            {
                response = await this.ExecuteRequest<ResultResponse<Sheet>, Sheet>(HttpVerb.POST, string.Format("workspaces/{0}/sheets", workspaceId), sheet, accessToken: accessToken);
            }

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<Sheet> CopySheet(string newName, long? sourceSheetId, long? destinationId, DestinationType destinationType, IEnumerable<SheetCopyInclusion> includes, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new Exception("New sheet name cannot be null or blank");
            }

            if (sourceSheetId == null)
            {
                throw new Exception("Source sheet ID cannot be null or blank");
            }

            if (destinationId == null)
            {
                throw new Exception("Destination ID cannot be null or blank");
            }

            var response = new ResultResponse<Sheet>();

            ContainerDestination container = new ContainerDestination()
            {
                DestinationId = destinationId.Value,
                DestinationType = destinationType.ToString(),
                NewName = newName
            };

            string includeString = "";

            if (includes != null && includes.Count() > 0)
            {
                includeString += string.Format("?include={0}", string.Join(",", includes.Select(i => i.ToString().ToCamelCase())));
            }

            response = await this.ExecuteRequest<ResultResponse<Sheet>, ContainerDestination>(HttpVerb.POST, string.Format("sheets/{0}/copy{1}", sourceSheetId, includeString), container, accessToken: accessToken);

            response.Result._Client = this;

            return response.Result;
        }

        public async Task<Sheet> GetSheetById(long? sheetId, string accessToken = null, string[] options = null) 
        {
            if (sheetId == null) 
            {
                throw new Exception("Sheet ID cannot be null");
            }

            string optionsString = String.Empty;

            if(options != null && options.Any())
            {
                optionsString = $"?{String.Join("&", options)}";
            }

            var response = await this.ExecuteRequest<Sheet, Sheet>(HttpVerb.GET, string.Format("sheets/{0}{1}", sheetId, optionsString), null, accessToken: accessToken);

            response._Client = this;

            return response;
        }

        public async Task<IEnumerable<Sheet>> GetSheetsForWorkspace(long? workspaceId, string accessToken = null)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null, accessToken: accessToken);

            response.Sheets.FirstOrDefault()._Client = this;

            return response.Sheets;
        }

        public async Task<IEnumerable<Sheet>> ListSheets(string accessToken = null)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Sheet>, Sheet>(HttpVerb.GET, string.Format("sheets"), null, accessToken: accessToken);
            return response.Data;
        }

        public async Task<IEnumerable<Sheet>> ListAllSheetsAndVersions(string accessToken = null) 
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Sheet>, Sheet>(HttpVerb.GET, "sheets?include=sheetVersion&includeAll=true", null, accessToken: accessToken);
            return response.Data;
        }

        private async Task<Stream> GetSheetAsFile(long? sheetId, PaperSize? paperSize, string contentType, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            var url = $"sheets/{sheetId}";
            if (paperSize.HasValue)
            {
                url += $"?paperSize={paperSize.Value}";
            }

            this._HttpClient.DefaultRequestHeaders.Remove("Authorization");

            this._HttpClient.DefaultRequestHeaders.Remove("Smartsheet-Change-Agent");

            if (accessToken != null)
            {
                this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            }
            else
            {
                this._HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this._AccessToken);
            }

            if (this._ChangeAgent != null)
            {
                this._HttpClient.DefaultRequestHeaders.Add("Smartsheet-Change-Agent", this._ChangeAgent);
            }
            
            this._HttpClient.DefaultRequestHeaders.Accept.Clear();
            this._HttpClient.DefaultRequestHeaders.Add("Accept", contentType);

            this.ValidateClientParameters();

            this.InitializeNewRequest();

            while (_RetryRequest && (_RetryCount < _AttemptLimit))
            {
                try
                {
                    if (_WaitTime > 0)
                    {
                        Thread.Sleep(_WaitTime);
                    }

                    var response = await this._HttpClient.GetAsync(url);

                    var statusCode = response.StatusCode;

                    switch (statusCode)
                    {
                        case HttpStatusCode.OK:
                        {
                            try
                            {
                                return await response.Content.ReadAsStreamAsync();
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }

                        }
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.ServiceUnavailable:
                        // .NET doesn't have a const for this
                        case (HttpStatusCode)429:
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();

                            dynamic result = JsonConvert.DeserializeObject(responseJson);

                            // did we hit an error that we should retry?
                            int code = result["errorCode"];

                            if (code == 4001)
                            {
                                // service may be down temporarily
                                this._WaitTime = Backoff(this._WaitTime, 60 * 1000);
                            }
                            else if (code == 4002 || code == 4004)
                            {
                                // internal error or simultaneous update.
                                this._WaitTime = Backoff(this._WaitTime, 1 * 1000);
                            }
                            else if (code == 4003)
                            {
                                // rate limit
                                this._WaitTime = Backoff(this._WaitTime, 2 * 1000);
                            }

                            break;
                        }
                        default:
                        {
                            this._RetryRequest = false;
                            dynamic result;
                            try
                            {
                                var responseJson = await response.Content.ReadAsStringAsync();

                                result = JsonConvert.DeserializeObject(responseJson);
                            }
                            catch (Exception)
                            {
                                throw new Exception(string.Format("HTTP Error {0}: url:[{1}]", statusCode, url));
                            }

                            var message = string.Format("HTTP Error {0} - Smartsheet error code {1}: {2} url:[{3}]", statusCode, result["errorCode"], result["message"], url);

                            throw new Exception(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!_RetryRequest)
                    {
                        throw e;
                    }
                }

                _RetryCount += 1;
            }

            throw new Exception(string.Format("Retries exceeded.  url:[{0}]", url));
        }
        
        public async Task<Stream> GetSheetAsExcel(long? sheetId)
        {
            return await GetSheetAsFile(sheetId, null, "application/vnd.ms-excel");
        }
        
        public async Task<Stream> GetSheetAsPdf(long? sheetId, PaperSize? paperSize)
        {
            return await GetSheetAsFile(sheetId, paperSize, "application/pdf");
        }

        public async Task<Stream> GetSheetAsCsv(long? sheetId)
        {
            return await GetSheetAsFile(sheetId, null, "text/csv");
        }

        #endregion


        #region Rows
        public async Task<IEnumerable<Row>> CreateRows(long? sheetId, IEnumerable<Row> rows, bool? toTop = null, bool? toBottom = null, long? parentId = null, long? siblingId = null, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rows.Count() > 1)
            {
                foreach (var row in rows)
                {
                    row.ToTop = toTop;
                    row.ToBottom = toBottom;
                    row.ParentId = parentId;
                    row.SiblingId = siblingId;

                    foreach (var cell in row.Cells)
                    {
                        cell.Build();
                    }
                }
            }

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.POST, string.Format("sheets/{0}/rows", sheetId), rows, accessToken: accessToken);

            return response.Result;
        }

        public async Task<CopyOrMoveRowResult> MoveRows(long? sourceSheetId, long? destinationSheetId, IEnumerable<long> rowIds, string accessToken = null)
        {
            if (sourceSheetId == null)
            {
                throw new Exception("Source Sheet ID cannot be null");
            }

            if (destinationSheetId == null)
            {
                throw new Exception("Destination Sheet ID cannot be null");
            }

            var copyOrMoveRowDirective = new CopyOrMoveRowDirective()
            {
                To = new CopyOrMoveRowDestination()
                {
                    SheetId = destinationSheetId
                },
                RowIds = rowIds.ToList()
            };

            var response = await this.ExecuteRequest<CopyOrMoveRowResult, CopyOrMoveRowDirective>(HttpVerb.POST, string.Format("sheets/{0}/rows/move?include=attachments,discussions", sourceSheetId), copyOrMoveRowDirective, accessToken: accessToken);

            return response;
        }

        public async Task<CopyOrMoveRowResult> CopyRows(long? sourceSheetId, long? destinationSheetId, IEnumerable<long> rowIds, string accessToken = null)
        {
            if (sourceSheetId == null)
            {
                throw new Exception("Source Sheet ID cannot be null");
            }

            if (destinationSheetId == null)
            {
                throw new Exception("Destination Sheet ID cannot be null");
            }

            var copyOrMoveRowDirective = new CopyOrMoveRowDirective()
            {
                To = new CopyOrMoveRowDestination()
                {
                    SheetId = destinationSheetId
                },
                RowIds = rowIds.ToList()
            };

            var response = await this.ExecuteRequest<CopyOrMoveRowResult, CopyOrMoveRowDirective>(HttpVerb.POST, string.Format("sheets/{0}/rows/copy?include=attachments,discussions", sourceSheetId), copyOrMoveRowDirective, accessToken: accessToken);

            return response;
        }

        public async Task<IEnumerable<Row>> LockRows(long? sheetId, bool locked, IEnumerable<long?> rowIds, string accessToken = null)
        {
            var rows_to_lock = new List<Dictionary<string, dynamic>>();

            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rowIds.Count() > 0)
            {
                foreach (var rowId in rowIds)
                {
                    var this_row = new Dictionary<string, dynamic>();
                    this_row.Add("locked", locked);
                    this_row.Add("id", rowId);
                    rows_to_lock.Add(this_row);
                }
            }

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Dictionary<string, dynamic>>>(HttpVerb.PUT, string.Format("sheets/{0}/rows", sheetId), rows_to_lock, accessToken: accessToken);

            return response.Result;
        }

        #endregion


        #region Folders
        public async Task<IEnumerable<Folder>> GetFoldersForWorkspace(long? workspaceId, string accessToken = null, bool loadAll = false)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}?loadAll={1}", workspaceId, loadAll ? "true" : "false"), null, accessToken: accessToken);

            return response.Folders;
        }

        public async Task<Folder> GetFolderById(long? folderId, string accessToken = null)
        {
            if (folderId == null)
            {
                throw new Exception("Folder ID cannot be null");
            }

            var response = await this.ExecuteRequest<Folder, Folder>(HttpVerb.GET, string.Format("folders/{0}", folderId), null, accessToken: accessToken);

            return response;
        }

        public async Task<Folder> CopyFolder(long? folderId, long? destinationId, string newName, string accessToken = null)
        {
            if (folderId == null)
            {
                throw new Exception("Folder ID cannot be null");
            }

            var containerDestinationObject = new ContainerDestinationObject()
            {
                DestinationId = destinationId.Value,
                DestinationType = "folder",
                NewName = newName
            };

            var response = await this.ExecuteRequest<ResultResponse<Folder>, ContainerDestinationObject>(HttpVerb.POST, string.Format("folders/{0}/copy?include=data", folderId), containerDestinationObject, accessToken: accessToken);

            return response.Result;
        }

        #endregion


        #region Reports
        public async Task<IEnumerable<ISmartsheetObject>> GetReportsForWorkspace(long? workspaceId, string accessToken = null)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null, accessToken: accessToken);

            return response.Reports;
        }

        public async Task<IEnumerable<Report>> ListReports(string accessToken = null)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Report>, Report>(HttpVerb.GET, string.Format("reports"), null, accessToken: accessToken);

            return response.Data;
        }

        #endregion


        #region Templates
        public async Task<IEnumerable<ISmartsheetObject>> GetTemplatesForWorkspace(long? workspaceId, string accessToken = null)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }

            var response = await this.ExecuteRequest<Workspace, Workspace>(HttpVerb.GET, string.Format("workspaces/{0}", workspaceId), null, accessToken: accessToken);

            return response.Templates;
        }

        public async Task<IEnumerable<Template>> ListTemplates(string accessToken = null)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Template>, Template>(HttpVerb.GET, string.Format("templates"), null, accessToken: accessToken);
            return response.Data;
        }

        #endregion


        #region Sights
        public async Task<IEnumerable<Sight>> ListSights(string accessToken = null)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<Sight>, Sight>(HttpVerb.GET, string.Format("sights"), null, accessToken: accessToken);
            return response.Data;
        }

        #endregion


        #region Update Requests
        public async Task<UpdateRequest> CreateUpdateRequest(long? sheetId, IEnumerable<long> rowIds, IEnumerable<Recipient> sendTo, IEnumerable<long> columnIds, string subject = null, string message = null, bool ccMe = false, bool includeDiscussions = true, bool includeAttachments = true, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rowIds.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more rows to update");
            }

            if (sendTo.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more recipients");
            }

            var request = new UpdateRequest()
            {
                SendTo = sendTo.ToList(),
                Subject = subject,
                Message = message,
                CcMe = ccMe,
                RowIds = rowIds.ToList(),
                ColumnIds = columnIds.ToList(),
                IncludeAttachments = includeAttachments,
                IncludeDiscussions = includeDiscussions
            };

            var result = await this.ExecuteRequest<ResultResponse<UpdateRequest>, UpdateRequest>(HttpVerb.POST, string.Format("sheets/{0}/updaterequests", sheetId), request, accessToken: accessToken);

            return result.Result;
        }

        #endregion


        #region Send Rows
        public async Task<MultiRowEmail> CreateSendRow(long? sheetId, MultiRowEmail email, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (email.RowIds.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more rows to update");
            }

            if (email.SendTo.Count() == 0)
            {
                throw new Exception("Must specifiy 1 or more recipients");
            }

            var result = await this.ExecuteRequest<ResultResponse<MultiRowEmail>, MultiRowEmail>(HttpVerb.POST, string.Format("sheets/{0}/rows/emails", sheetId), email, accessToken: accessToken);

            return result.Result;
        }

        #endregion


        #region Webhooks
        public async Task<IEnumerable<Webhook>> GetWebhooksForUser(string accessToken = null, bool includeAll = false)
        {
            var result = await this.ExecuteRequest<IndexResultResponse<Webhook>, Webhook>(HttpVerb.GET, string.Format("webhooks?includeAll={0}", includeAll ? "true" : "false"), null, accessToken: accessToken);

            return result.Data;
        }

        public async Task<Webhook> GetWebhook(long? webhookId, string accessToken = null)
        {
            if (webhookId == null)
            {
                throw new Exception("Webhook ID cannot be null");
            }

            var result = await this.ExecuteRequest<Webhook, Webhook>(HttpVerb.GET, string.Format("webhooks/{0}", webhookId), null, accessToken: accessToken);

            return result;
        }

        public async Task<Webhook> CreateWebhook(Webhook model, string accessToken = null)
        {
            var result = await this.ExecuteRequest<ResultResponse<Webhook>, Webhook>(HttpVerb.POST, "webhooks", model, accessToken: accessToken);

            return result.Result;
        }

        public async Task<Webhook> UpdateWebhook(long? webhookId, Webhook model, string accessToken = null)
        {
            var result = await this.ExecuteRequest<ResultResponse<Webhook>, Webhook>(HttpVerb.PUT, string.Format("webhooks/{0}", webhookId), model, accessToken: accessToken);

            return result.Result;
        }
        
        public async Task<Webhook> DeleteWebhook(long? webhookId, string accessToken = null)
        {
            var result = await this.ExecuteRequest<ResultResponse<Webhook>, Webhook>(HttpVerb.DELETE, string.Format("webhooks/{0}", webhookId), null, accessToken: accessToken);

            return result.Result;
        }

        #endregion


        #region Columns
        public async Task<Column> EditColumn(long? sheetId, long? columnId, Column model, string accessToken = null)
        {
            if (columnId == null)
            {
                throw new Exception("Column ID cannot be null");
            }

            var result = await this.ExecuteRequest<ResultResponse<Column>, Column>(HttpVerb.PUT, string.Format("sheets/{0}/columns/{1}", sheetId, columnId), model, accessToken: accessToken);

            return result.Result;
        }

        public async Task<Column> CreateColumn(long? sheetId, Column model, string accessToken = null) 
        {
            var result = await this.ExecuteRequest<ResultResponse<Column>, Column>(HttpVerb.POST, string.Format("sheets/{0}/columns/", sheetId), model, accessToken: accessToken);

            return result.Result;
        }

        public async Task<ResultResponse> DeleteColumn(long? sheetId, long? columnId, string accessToken = null) 
        {
            var result = await this.ExecuteRequest<ResultResponse, Column>(HttpVerb.DELETE, string.Format("sheets/{0}/columns/{1}", sheetId, columnId), null, accessToken: accessToken);

            return result;
        }

        #endregion


        #region Attachments
        [Obsolete("UploadAttachmentToRow is deprecated. Use AttachFileToRow.")]
        public async Task<Attachment> UploadAttachmentToRow(long? sheetId, long? rowId, string fileName, long length, Stream stream, string contentType = null, string accessToken = null)
        {
            return await AttachFileToRow(sheetId, rowId, fileName, length, stream, contentType, accessToken);
        }

        [Obsolete("UploadAttachmentToRow is deprecated. Use AttachFileToRow.")]
        public async Task<Attachment> UploadAttachmentToRow(long? sheetId, long? rowId, IFormFile formFile, string accessToken = null)
        {
            return await AttachFileToRow(sheetId, rowId, formFile, accessToken);
        }

        public async Task<Attachment> AttachFileToRow(long? sheetId, long? rowId, string fileName, long length,
            Stream stream, string contentType = null, string accessToken = null)
        {
            var url = $"https://api.smartsheet.com/2.0/sheets/{sheetId}/rows/{rowId}/attachments";

            return await UploadFileAttachment(url, fileName, length, stream, contentType, accessToken);
        }
        
        public async Task<Attachment> AttachFileToRow(long? sheetId, long? rowId, IFormFile formFile, string accessToken = null)
        {
            using (var stream = formFile.OpenReadStream())
            {
                var response = await this.AttachFileToRow(sheetId, rowId, formFile.FileName, formFile.Length, stream, formFile.ContentType, accessToken);
                return response;
            }

        }
        
        public async Task<Attachment> AttachFileToSheet(long? sheetId, string fileName, long length, Stream stream, string contentType = null, string accessToken = null)
        {
            var url = $"https://api.smartsheet.com/2.0/sheets/{sheetId}/attachments";

            return await UploadFileAttachment(url, fileName, length, stream, contentType, accessToken);
        }

        public async Task<Attachment> AttachFileToSheet(long? sheetId, IFormFile formFile, string accessToken = null)
        {
            using (var stream = formFile.OpenReadStream())
            {
                var response = await this.AttachFileToSheet(sheetId, formFile.FileName, formFile.Length, stream, formFile.ContentType, accessToken);
                return response;
            }

        }
        
        private async Task<Attachment> UploadFileAttachment(string requestUrl, string fileName, long length, Stream stream,
            string contentType = null, string accessToken = null)
        {
            this._HttpClient.DefaultRequestHeaders.Remove("Authorization");
            this.SetAuthorizationHeader(accessToken);
            this._HttpClient.DefaultRequestHeaders.Accept.Clear();
            this._HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            byte[] data;
            using (var br = new BinaryReader(stream))
            {
                data = br.ReadBytes((int)stream.Length);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Content = new ByteArrayContent(data);
            request.Content.Headers.ContentLength = length;
            request.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            if (contentType != null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            
            this.InitializeNewRequest();
            
            while (_RetryRequest && (_RetryCount < _AttemptLimit))
            {
                try
                {
                    if (_WaitTime > 0)
                    {
                        Thread.Sleep(_WaitTime);
                    }

                    var serializerSettings = new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var serializedData = JsonConvert.SerializeObject(data, Formatting.None, serializerSettings);

                    var response = await this._HttpClient.SendAsync(request);
                    
                    var statusCode = response.StatusCode;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        try
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var jsonResponseBody = JsonConvert.DeserializeObject(responseBody).ToString();
                            var resultResponse = JsonConvert.DeserializeObject<ResultResponse<Attachment>>(jsonResponseBody);
                            return resultResponse.Result;
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }

                    if (statusCode.Equals(HttpStatusCode.InternalServerError) || statusCode.Equals(HttpStatusCode.ServiceUnavailable) || statusCode.Equals((HttpStatusCode)429)) // .NET doesn't have a const for this
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();

                        dynamic result = JsonConvert.DeserializeObject(responseJson);

                        // did we hit an error that we should retry?
                        int code = result["errorCode"];

                        if (code == 4001)
                        {
                            // service may be down temporarily
                            _WaitTime = Backoff(_WaitTime, 60 * 1000);
                        }
                        else if (code == 4002 || code == 4004)
                        {
                            // internal error or simultaneous update.
                            _WaitTime = Backoff(_WaitTime, 1 * 1000);
                        }
                        else if (code == 4003)
                        {
                            // rate limit
                            _WaitTime = Backoff(_WaitTime, 2 * 1000);
                        }
                    }
                    else
                    {
                        _RetryRequest = false;
                        dynamic result;
                        try
                        {
                            var responseJson = await response.Content.ReadAsStringAsync();

                            result = JsonConvert.DeserializeObject(responseJson);
                        }
                        catch (Exception)
                        {
                            throw new Exception(string.Format("HTTP Error {0}: url:[{1}]", statusCode, requestUrl));
                        }

                        var message = string.Format("HTTP Error {0} - Smartsheet error code {1}: {2} url:[{3}]", statusCode, result["errorCode"], result["message"], requestUrl);

                        throw new Exception(message);
                    }
                }
                catch (Exception e)
                {
                    if (!_RetryRequest)
                    {
                        throw e;
                    }
                }

                _RetryCount += 1;
            }
            
            throw new Exception(string.Format("Retries exceeded.  url:[{0}]", requestUrl));
        }

        public async Task<Attachment> AttachUrlToRow(long? sheetId, long? rowId, string url, string name, string description, string attachmentType, string attachmentSubType, string accessToken = null)
        {
            var attachment = new Attachment
            {
                Url = url,
                Name = name,
                Description = description,
                AttachmentType = attachmentType,
                AttachmentSubType = attachmentSubType
            };

            return await AttachUrlToRow(sheetId, rowId, attachment, accessToken);
        }

        public async Task<Attachment> AttachUrlToRow(long? sheetId, long? rowId, Attachment attachment, string accessToken = null)
        {
            var requestUrl = $"https://api.smartsheet.com/2.0/sheets/{sheetId}/rows/{rowId}/attachments";
            
            var attachmentCopy = new Attachment
            {
                AttachmentSubType = attachment.AttachmentSubType,
                AttachmentType = attachment.AttachmentType,
                Description = attachment.Description,
                Name = attachment.Name,
                Url = attachment.Url
            };

            return await UploadUrlAttachment(requestUrl, attachmentCopy, accessToken);
        }
        
        public async Task<Attachment> AttachUrlToSheet(long? sheetId, string url, string name, string description, string attachmentType, string attachmentSubType, string accessToken = null)
        {
            var attachment = new Attachment
            {
                Url = url,
                Name = name,
                Description = description,
                AttachmentType = attachmentType,
                AttachmentSubType = attachmentSubType
            };

            return await AttachUrlToSheet(sheetId, attachment, accessToken);
        }
        
        public async Task<Attachment> AttachUrlToSheet(long? sheetId, Attachment attachment, string accessToken = null)
        {
            var requestUrl = $"https://api.smartsheet.com/2.0/sheets/{sheetId}/attachments";
            
            var attachmentCopy = new Attachment
            {
                AttachmentSubType = attachment.AttachmentSubType,
                AttachmentType = attachment.AttachmentType,
                Description = attachment.Description,
                Name = attachment.Name,
                Url = attachment.Url
            };

            return await UploadUrlAttachment(requestUrl, attachmentCopy, accessToken);
        }
        
        private async Task<Attachment> UploadUrlAttachment(string requestUrl, Attachment attachment, string accessToken = null)
        {
            attachment.Build();
            
            var response = await this.ExecuteRequest<ResultResponse<Attachment>, Attachment>(HttpVerb.POST, requestUrl, attachment, accessToken: accessToken);

            return response.Result;
        }

        public async Task<IEnumerable<Attachment>> ListAttachments(long? sheetId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<IndexResultResponse<Attachment>, Attachment>(HttpVerb.GET,$"sheets/{sheetId}/attachments", null, accessToken: accessToken);
            return response.Data;
        }

        public async Task<Attachment> GetAttachment(long? sheetId, long? attachmentId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (attachmentId == null)
            {
                throw new Exception("Attachment ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<Attachment, Attachment>(HttpVerb.GET,$"sheets/{sheetId}/attachments/{attachmentId}", null, accessToken: accessToken);
            return response;
        }
        
        public async Task<ResultResponse> DeleteAttachment(long? sheetId, long? attachmentId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            if (attachmentId == null)
            {
                throw new Exception("Attachment ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<ResultResponse, Discussion>(HttpVerb.DELETE, $"sheets/{sheetId}/attachments/{attachmentId}", null, accessToken: accessToken);
            return response;
        }

        public async Task<Attachment> AttachNewFileVersion(long? sheetId, long? attachmentId, string fileName, long length, Stream stream, string contentType = null, string accessToken = null)
        {
            var url = $"https://api.smartsheet.com/2.0/sheets/{sheetId}/attachments/{attachmentId}/versions";

            return await UploadFileAttachment(url, fileName, length, stream, contentType, accessToken);
        }

        public async Task<Attachment> AttachNewFileVersion(long? sheetId, long? attachmentId, IFormFile formFile, string accessToken = null)
        {
            using (var stream = formFile.OpenReadStream())
            {
                var response = await this.AttachNewFileVersion(sheetId, attachmentId, formFile.FileName, formFile.Length, stream, formFile.ContentType, accessToken);
                return response;
            }
        }

        #endregion


        #region Discussions
        public async Task<Discussion> CreateDiscussionOnRow(long? sheetId, long? rowId, string commentText, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            if (rowId == null)
            {
                throw new Exception("Row ID cannot be null");
            }

            var discussion = new Discussion
            {
                Comment = new Comment
                {
                    Text = commentText
                }
            };
            
            var response = await this.ExecuteRequest<ResultResponse<Discussion>, Discussion>(HttpVerb.POST, $"sheets/{sheetId}/rows/{rowId}/discussions", discussion, accessToken: accessToken);

            return response.Result;
        }
        
        public async Task<Discussion> CreateDiscussionOnSheet(long? sheetId, string commentText, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            var discussion = new Discussion
            {
                Comment = new Comment
                {
                    Text = commentText
                }
            };
            
            var response = await this.ExecuteRequest<ResultResponse<Discussion>, Discussion>(HttpVerb.POST, $"sheets/{sheetId}/discussions", discussion, accessToken: accessToken);

            return response.Result;
        }
        
        public async Task<ResultResponse> DeleteDiscussion(long? sheetId, long? discussionId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            if (discussionId == null)
            {
                throw new Exception("Discussion ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<ResultResponse, Discussion>(HttpVerb.DELETE, $"sheets/{sheetId}/discussions/{discussionId}", null, accessToken: accessToken);
            return response;
        }
        
        public async Task<IEnumerable<Discussion>> ListDiscussions(long? sheetId, bool includeAll = false, bool includeComments = false, bool includeAttachments = false, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            var includeString = includeComments ? "?include=comments" + (includeAttachments ? ",attachments" : "") : "";

            if (includeAll)
            {
                includeString += string.IsNullOrEmpty(includeString) ? "?includeAll=true" : "&includeAll=true";
            }
            
            var response = await this.ExecuteRequest<IndexResultResponse<Discussion>, Discussion>(HttpVerb.GET,$"sheets/{sheetId}/discussions{includeString}", null, accessToken: accessToken);
            return response.Data;
        }
        
        public async Task<Discussion> GetDiscussion(long? sheetId, long? discussionId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (discussionId == null)
            {
                throw new Exception("Attachment ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<Discussion, Discussion>(HttpVerb.GET,$"sheets/{sheetId}/discussions/{discussionId}", null, accessToken: accessToken);
            return response;
        }
        
        public async Task<IEnumerable<Discussion>> ListRowDiscussions(long? sheetId, long? rowId, bool includeAll = false, bool includeComments = false, bool includeAttachments = false, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            if (rowId == null)
            {
                throw new Exception("Row ID cannot be null");
            }

            var includeString = includeComments ? "?include=comments" + (includeAttachments ? ",attachments" : "") : "";

            if (includeAll)
            {
                includeString += string.IsNullOrEmpty(includeString) ? "?includeAll=true" : "&includeAll=true";
            }
            
            var response = await this.ExecuteRequest<IndexResultResponse<Discussion>, Discussion>(HttpVerb.GET,$"sheets/{sheetId}/rows/{rowId}/discussions{includeString}", null, accessToken: accessToken);
            return response.Data;
        }
        
        #endregion


        #region Comments
        public async Task<Comment> AddComment(long? sheetId, long? discussionId, string commentText, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            if (discussionId == null)
            {
                throw new Exception("Discussion ID cannot be null");
            }

            var comment = new Comment
            {
                Text = commentText
            };
            
            var response = await this.ExecuteRequest<ResultResponse<Comment>, Comment>(HttpVerb.POST, $"sheets/{sheetId}/discussions/{discussionId}/comments", comment, accessToken: accessToken);

            return response.Result;
        }
        
        public async Task<Comment> EditComment(long? sheetId, long? commentId, string commentText, string accessToken = null)
        {
            var comment = new Comment
            {
                Text = commentText
            };
            
            var result = await this.ExecuteRequest<ResultResponse<Comment>, Comment>(HttpVerb.PUT, $"sheets/{sheetId}/comments/{commentId}", comment, accessToken: accessToken);

            return result.Result;
        }
        
        public async Task<ResultResponse> DeleteComment(long? sheetId, long? commentId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }
            
            if (commentId == null)
            {
                throw new Exception("Comment ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<ResultResponse, Comment>(HttpVerb.DELETE, $"sheets/{sheetId}/comments/{commentId}", null, accessToken: accessToken);
            return response;
        }
        
        public async Task<Comment> GetComment(long? sheetId, long? commentId, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (commentId == null)
            {
                throw new Exception("Comment ID cannot be null");
            }
            
            var response = await this.ExecuteRequest<Comment, Comment>(HttpVerb.GET,$"sheets/{sheetId}/comments/{commentId}", null, accessToken: accessToken);
            return response;
        }

        #endregion


        #region Users
        public async Task<User> GetCurrentUser(string accessToken = null)
        {
            var response = await this.ExecuteRequest<User, User>(HttpVerb.GET, string.Format("users/me"), null, accessToken: accessToken);
            return response;
        }

        public async Task<Home> GetHome(string accessToken = null)
        {
            var response = await this.ExecuteRequest<Home, Home>(HttpVerb.GET, string.Format("home"), null, accessToken: accessToken);
            return response;
        }

        public async Task<IEnumerable<User>> ListUsers(string accessToken = null, bool includeAll = false)
        {
            if (includeAll)
            {
                var response = await this.ExecuteRequest<IndexResultResponse<User>, User>(HttpVerb.GET, string.Format("users?includeAll=true"), null, accessToken: accessToken);
                return response.Data;

            }
            else
            {
                var response = await this.ExecuteRequest<IndexResultResponse<User>, User>(HttpVerb.GET, string.Format("users"), null, accessToken: accessToken);
                return response.Data;
            }

        }

        public async Task<User> AddUser(User user, string accessToken = null)
        {
            this._HttpClient.DefaultRequestHeaders.Add("sendEmail", "false");

            var response = await this.ExecuteRequest<ResultResponse<User>, User>(HttpVerb.POST, string.Format("users"), data: user, accessToken: accessToken);

            return response.Result;
        }

        public async Task<ResultResponse> RemoveUser(long userID, string transferTo = null, bool transferSheets = false, bool removeFromSharing = false, string accessToken = null)
        {
            if (transferTo == null)
            {
                var thisURL = string.Format("users/{0}?removeFromSharing={1}", userID, removeFromSharing);

                var response = await this.ExecuteRequest<ResultResponse, User>(HttpVerb.DELETE, string.Format(thisURL), null, accessToken: accessToken);

                return response;
            }
            else
            {
                var thisURL = string.Format("users/{0}?transferTo={1}&removeFromSharing={2}&transferSheets={3}", userID, transferTo, removeFromSharing, transferSheets);

                var response = await this.ExecuteRequest<ResultResponse, User>(HttpVerb.DELETE, string.Format(thisURL), null, accessToken: accessToken);

                return response;
            }

        }

        public async Task<User> UpdateUser(long userID, bool admin, bool licensedSheetCreator, string firstName, string lastName, bool groupAdmin, bool resourceViewer, string accessToken = null)
        {
            var thisUser = new User();
            thisUser.Admin = admin;
            thisUser.LicensedSheetCreator = licensedSheetCreator;
            thisUser.FirstName = firstName;
            thisUser.LastName = lastName;
            thisUser.GroupAdmin = groupAdmin;
            thisUser.ResourceViewer = resourceViewer;

            var thisURL = string.Format("users/{0}", userID);
            var response = await this.ExecuteRequest<ResultResponse<User>, User>(HttpVerb.PUT, string.Format(thisURL), thisUser, accessToken: accessToken);
            return response.Result;
        }

        #endregion


        #region Groups
        public async Task<IEnumerable<Group>> ListOrgGroups(string accessToken = null, bool includeAll = false)
        {
            if (includeAll)
            {
                var response = await this.ExecuteRequest<IndexResultResponse<Group>, Group>(HttpVerb.GET, string.Format("groups?includeAll=true"), null, accessToken: accessToken);
                return response.Data;

            }
            else
            {
                var response = await this.ExecuteRequest<IndexResultResponse<Group>, Group>(HttpVerb.GET, string.Format("groups"), null, accessToken: accessToken);
                return response.Data;
            }

        }

        public async Task<Group> CreateGroup(string groupName, string description = null, List<GroupMember> members = null, string accessToken = null)
        {

            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new Exception("Group Name cannot be null or blank");
            }

            var group = new Group(groupName, description, members);
            var response = await this.ExecuteRequest<ResultResponse<Group>, Group>(HttpVerb.POST, string.Format("groups"), group, accessToken: accessToken);
            return response.Result;

        }

        public async Task<ResultResponse> DeleteGroup(long groupId, string accessToken = null)
        {
            var response = await this.ExecuteRequest<ResultResponse, Group>(HttpVerb.DELETE, string.Format("groups/{0}", groupId), null, accessToken: accessToken);
            return response;
        }

        public async Task<Group> GetGroup(long? groupId, string accessToken = null)
        {
            var response = await this.ExecuteRequest<Group, Group>(HttpVerb.GET, string.Format("groups/{0}", groupId), null, accessToken: accessToken);
            return response;
        }

        public async Task<Group> UpdateGroup(long groupId, string groupName = null, string description = null, long? ownerId = null, string accessToken = null)
        {
            var this_group = new Group();
            this_group.Description = description;
            this_group.Name = groupName;
            this_group.OwnerId = ownerId;
            var response = await this.ExecuteRequest<ResultResponse<Group>, Group>(HttpVerb.PUT, string.Format("groups/{0}", groupId), this_group, accessToken: accessToken);
            return response.Result;
        }

        public async Task<IEnumerable<GroupMember>> AddGroupMembers(long groupId, List<GroupMember> newMembers = null, string accessToken = null)
        {
            var response = await this.ExecuteRequest<IndexResultResponse<GroupMember>, List<GroupMember>>(HttpVerb.POST, string.Format("groups/{0}/members", groupId), newMembers, accessToken: accessToken);
            return response.Data;
        }

        public async Task<ResultResponse> RemoveGroupMember(long groupId, long userId, string accessToken = null)
        {
            var response = await this.ExecuteRequest<ResultResponse, List<GroupMember>>(HttpVerb.DELETE, string.Format("groups/{0}/members/{1}", groupId, userId), null, accessToken: accessToken);
            return response;
        }
        #endregion


        #region Cross Sheet Refs 

        public async Task<IEnumerable<CrossSheetReference>> ListCrossSheetReferences(long? sheetId, string accessToken = null) {
            if (sheetId == null) {
                throw new Exception("Sheet ID cannot be null");
            }

            var response = await this.ExecuteRequest<IndexResultResponse<CrossSheetReference>, User>(HttpVerb.GET, string.Format("sheets/{0}/crosssheetreferences", sheetId), null, accessToken: accessToken);
            return response.Data;
        }

        public async Task<CrossSheetReference> CreateCrossSheetReference(long? sheetId, CrossSheetReference crossSheetReference, string accessToken = null)
        {
            if (sheetId == null) 
            {
                throw new Exception("Sheet ID cannot be null");
            }

            var response = await this.ExecuteRequest<ResultResponse<CrossSheetReference>, CrossSheetReference>(HttpVerb.POST, $"sheets/{sheetId}/crosssheetreferences", crossSheetReference, accessToken: accessToken);
            return response.Result;
        }

        #endregion

        #region Shares
        
        public async Task<IEnumerable<Share>> ShareWorkspace(long? workspaceId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null)
        {
            if (workspaceId == null)
            {
                throw new Exception("Workspace ID cannot be null");
            }
            
            shares.ToList().ForEach(s => s.Build());

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Share>>, IEnumerable<Share>>(HttpVerb.POST,
                string.Format("workspaces/{0}/shares?sendEmail={1}", workspaceId, sendEmail), shares, accessToken);

            return response.Result;
        }

        public async Task<IEnumerable<Share>> ShareReport(long? reportId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null)
        {
            if (reportId == null)
            {
                throw new Exception("Report ID cannot be null");
            }
            
            shares.ToList().ForEach(s => s.Build());

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Share>>, IEnumerable<Share>>(
                HttpVerb.POST, string.Format("reports/{0}/shares?sendEmail={1}", reportId, sendEmail), shares,
                accessToken);

            return response.Result;
        }

        public async Task<IEnumerable<Share>> ShareSheet(long? sheetId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            shares.ToList().ForEach(s => s.Build());
            
            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Share>>, IEnumerable<Share>>(
                HttpVerb.POST, string.Format("sheets/{0}/shares?sendEmail={1}", sheetId, sendEmail), shares,
                accessToken);

            return response.Result;
        }
        
        public async Task<IEnumerable<Share>> ShareSight(long? sightId, IEnumerable<Share> shares, bool sendEmail = false, string accessToken = null)
        {
            if (sightId == null)
            {
                throw new Exception("Sight ID cannot be null");
            }
            
            shares.ToList().ForEach(s => s.Build());

            var response = await this.ExecuteRequest<ResultResponse<IEnumerable<Share>>, IEnumerable<Share>>(
                HttpVerb.POST, string.Format("sights/{0}/shares?sendEmail={1}", sightId, sendEmail), shares,
                accessToken);

            return response.Result;
        }
        
        #endregion

        #region Cell History

        public async Task<IEnumerable<CellHistory>> GetCellHistory(long? sheetId, long? rowId, long? columnId, IEnumerable<CellInclusions> includes = null, string accessToken = null)
        {
            if (sheetId == null)
            {
                throw new Exception("Sheet ID cannot be null");
            }

            if (rowId == null)
            {
                throw new Exception("Row ID cannot be null");
            }

            if (columnId == null)
            {
                throw new Exception("Column ID cannot be null");
            }
            
            string includeString = "";

            if (includes != null && includes.Count() > 0)
            {
                includeString += string.Format("?include={0}", string.Join(",", includes.Select(i => i.ToString().ToCamelCase())));
            }
            
            var response = await this.ExecuteRequest<IndexResultResponse<CellHistory>, CellHistory>(HttpVerb.GET,
                string.Format("sheets/{0}/rows/{1}/columns/{2}/history" + includeString, sheetId, rowId, columnId), null,
                accessToken: accessToken);

            return response.Data;

        }

        #endregion
    }
}