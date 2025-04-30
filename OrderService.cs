using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderSyncScheduler
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenManager _tokenManager;
        private readonly string _ordersEndpoint;

        public OrderService(string tokenEndpoint, string ordersEndpoint, string clientId, string clientSecret)
        {
            _httpClient = new HttpClient();
            _tokenManager = TokenManager.GetInstance(tokenEndpoint, clientId, clientSecret);
            _ordersEndpoint = ordersEndpoint;
        }

        public async Task<Order[]> GetOrdersAsync()
        {
            try
            {
                var token = await _tokenManager.GetAccessTokenAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Get, _ordersEndpoint);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Order[]>(content);
            }
            catch (Exception ex)
            {
                throw new Exception("Siparişler alınamadı", ex);
            }
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
    }
} 