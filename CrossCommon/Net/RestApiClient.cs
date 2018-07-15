using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossCommon
{
    public class RestApiClient
    {
        private readonly JsonSerializer _serializer = new JsonSerializer();

        public RestApiClient() : this(CreateDefaultClient())
        {
        }

        public RestApiClient(string baseUrl) : this(CreateDefaultClient())
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                Uri baseUri = ParseUri(baseUrl);
                if (baseUri != null)
                {
                    Client.BaseAddress = baseUri;
                }
            }
        }

        public RestApiClient(HttpClient httpClient)
        {
            Client = httpClient;
        }

        private static HttpClient CreateDefaultClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        /// <summary>
        /// Underlying HttpClient
        /// </summary>
        /// <value>The client.</value>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// GET request
        /// </summary>
        /// <param name="url"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public Task<ApiResult<TResult>> GetAsync<TResult>(string url)
        {
            HttpRequestMessage requestMessage = CreateGetRequest(url);
            return SendRequestAsync<TResult>(requestMessage);
        }

        /// <summary>
        /// POST request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dto"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public Task<ApiResult<TResult>> PostAsync<T, TResult>(string url, T dto)
        {
            string json = JsonConvert.SerializeObject(dto);
            Log.Debug(json);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return PostContentAsync<TResult>(url, content);
        }

        /// <summary>
        /// POST request with a specific content
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public Task<ApiResult<TResult>> PostContentAsync<TResult>(string url, HttpContent content)
        {
            HttpRequestMessage requestMessage = CreatePostRequest(url, content);
            return SendRequestAsync<TResult>(requestMessage);
        }

        /// <summary>
        /// Send HTTP Request without Cancellation token
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public Task<ApiResult<TResult>> SendRequestAsync<TResult>(HttpRequestMessage requestMessage)
        {
            return SendRequestAsync<TResult>(requestMessage, CancellationToken.None);
        }

        /// <summary>
        /// Send HTTP Request and parrse result
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public async Task<ApiResult<TResult>> SendRequestAsync<TResult>(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            try
            {
                Debug($"{requestMessage.Method} {requestMessage.RequestUri}");
                Debug($"Headers: {requestMessage.Headers}");
                HttpResponseMessage response = await Client.SendAsync(requestMessage, cancellationToken);
                return await ParseResponse<TResult>(_serializer, response);
            }
            catch (Exception ex)
            {
                Debug(ex);
                return ParseException<TResult>(ex);
            }
        }

        public HttpRequestMessage CreateGetRequest(string url) => new HttpRequestMessage(HttpMethod.Get, CreateUri(url));

        public HttpRequestMessage CreatePostRequest(string url, HttpContent content) => new HttpRequestMessage(HttpMethod.Post, CreateUri(url)) { Content = content };

        private Uri CreateUri(string url)
        {
            Uri uri = null;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                uri = new Uri(url);
            }
            else if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                uri = new Uri(Client.BaseAddress, url);
            }

            return uri;
        }

        private static ApiResult<TResult> ParseException<TResult>(Exception ex)
        {
            Debug(ex);
            if (ex is WebException)
            {
                return new ApiResult<TResult>(ApiResultStatus.NoInternetConnection);
            }
            else
            {
                return new ApiResult<TResult>(ApiResultStatus.InternalProblem);
            }
        }

        private static async Task<ApiResult<TResult>> ParseResponse<TResult>(JsonSerializer serializer, HttpResponseMessage response)
        {
            Debug(response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                TResult item = default(TResult);
                if (typeof(TResult) == typeof(string))
                {
                    string str = await response.Content.ReadAsStringAsync();
                    item = (TResult)((object)str);
                }
                else
                {
#if DEBUG
                    var str = await response.Content.ReadAsStringAsync();
                    Debug(str);
#endif
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var textReader = new StreamReader(stream))
                        {
                            using (JsonReader reader = new JsonTextReader(textReader))
                            {
                                item = serializer.Deserialize<TResult>(reader);
                            }
                        }
                    }
                }
                return new ApiResult<TResult>(ToNetworkState(response), item);
            }
            else
            {
#if DEBUG
                var str = await response.Content.ReadAsStringAsync();
                Debug(str);
#endif
                return new ApiResult<TResult>(ToNetworkState(response));
            }
        }

        private static ApiResultStatus ToNetworkState(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return ApiResultStatus.Success;

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return ApiResultStatus.Unauthorized;
            }

            return ApiResultStatus.InternalProblem;
        }

        private static void Debug(object obj)
        {
            Debug(obj?.ToString());
        }

        private static void Debug(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }

        protected static Uri ParseUri(string baseUrl)
        {
            if (!(baseUrl?.Trim()?.EndsWith("/") ?? false))
            {
                baseUrl = baseUrl?.Trim() + "/";
            }

            Uri res = null;
            Uri.TryCreate(baseUrl, UriKind.Absolute, out res);
            return res;
        }
    }
}