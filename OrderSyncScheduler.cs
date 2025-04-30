namespace OrderSyncScheduler
{
    public class OrderSyncScheduler
    {
        private readonly OrderService _orderService;
        private Timer _timer;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

        public OrderSyncScheduler(OrderService orderService)
        {
            _orderService = orderService;
        }

        public void Start()
        {
            _timer = new Timer(async _ => await SyncOrdersAsync(), null, TimeSpan.Zero, _syncInterval);
        }

        public void Stop()
        {
            _timer?.Dispose();
        }

        private async Task SyncOrdersAsync()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync();
                foreach (var order in orders)
                {
                    Console.WriteLine($"Sipariş alındı: {order.Id} - {order.CustomerName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş senkronizasyonu sırasında hata oluştu: {ex.Message}");
            }
        }
    }
} 