using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderSyncScheduler
{
    public class TokenManager
    {
        private static TokenManager _instance;
        private static readonly object _lock = new object();
        private string _accessToken;
        private DateTime _tokenExpiration;
        private int _tokenRequestCount;
        private readonly HttpClient _httpClient;
        private readonly string _tokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private TokenManager(string tokenEndpoint, string clientId, string clientSecret)
        {
            _httpClient = new HttpClient();
            _tokenEndpoint = tokenEndpoint;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tokenRequestCount = 0;
        }

        public static TokenManager GetInstance(string tokenEndpoint, string clientId, string clientSecret)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TokenManager(tokenEndpoint, clientId, clientSecret);
                    }
                }
            }
            return _instance;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // Token geçerli ve saatlik limit aşılmamışsa mevcut token'ı döndür
            if (!string.IsNullOrEmpty(_accessToken) && 
                DateTime.UtcNow < _tokenExpiration && 
                _tokenRequestCount < 5)
            {
                return _accessToken;
            }

            // Token süresi dolmuş veya limit aşılmışsa yeni token al
            if (DateTime.UtcNow >= _tokenExpiration || _tokenRequestCount >= 5)
            {
                _tokenRequestCount = 0;
                await RequestNewTokenAsync();
            }

            _tokenRequestCount++;
            return _accessToken;
        }

        private async Task RequestNewTokenAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                });

                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                _accessToken = tokenResponse.AccessToken;
                _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            }
            catch (Exception ex)
            {
                throw new Exception("Token alınamadı", ex);
            }
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
} 