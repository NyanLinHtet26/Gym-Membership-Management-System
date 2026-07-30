using System.Net.Http.Headers;

namespace GMMS.App.Services
{
    public class HttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;
        private readonly AuthTokenStore _authTokenStore;

        public string BaseUrl => _baseUrl;

        public HttpClientService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            AuthTokenStore authTokenStore)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = configuration["BackendApiUrl"]!;
            _authTokenStore = authTokenStore;
        }

        public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            return await Execute<object, TResponse>(endpoint, HttpMethodType.Get);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            return await Execute<TRequest, TResponse>(endpoint, HttpMethodType.Post, request);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            return await Execute<TRequest, TResponse>(endpoint, HttpMethodType.Put, request);
        }

        public async Task<TResponse?> DeleteAsync<TResponse>(string endpoint)
        {
            return await Execute<object, TResponse>(endpoint, HttpMethodType.Delete);
        }

        private enum HttpMethodType
        {
            Get,
            Post,
            Put,
            Delete
        }

        private async Task<TResponse?> Execute<TRequest, TResponse>(
            string endpoint,
            HttpMethodType method,
            TRequest? request = default)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            if (!string.IsNullOrEmpty(_authTokenStore.AccessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authTokenStore.AccessToken);
            }

            HttpResponseMessage response;
            switch (method)
            {
                case HttpMethodType.Get:
                    response = await httpClient.GetAsync(endpoint);
                    break;

                case HttpMethodType.Post:
                    response = await httpClient.PostAsJsonAsync(endpoint, request);
                    break;

                case HttpMethodType.Put:
                    response = await httpClient.PutAsJsonAsync(endpoint, request);
                    break;

                case HttpMethodType.Delete:
                    response = await httpClient.DeleteAsync(endpoint);
                    break;

                default:
                    throw new Exception("Invalid HTTP method.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error ({response.StatusCode}): {body}");
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}
