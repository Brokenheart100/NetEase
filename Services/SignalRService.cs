using Microsoft.AspNetCore.SignalR.Client;
using NetEase.Dtos;
using System.Diagnostics;
using System.Net.Http;

namespace NetEase.Services
{
    public class SignalRService
    {
        private HubConnection _hubConnection;
        private readonly string _hubUrl;
        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public SignalRService(HttpClient httpClient)
        {
            // 从共享的HttpClient获取基地址并拼接Hub路由
            var baseUrl = httpClient.BaseAddress.ToString().TrimEnd('/');
            _hubUrl = $"{baseUrl}/chathub";
        }
        // 连接到Hub，需要提供JWT Token进行认证
        public async Task ConnectAsync(string token)
        {
            // 如果已经连接，或者正在连接中，则不再重复执行
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    // 在每次连接或重连时，都使用最新的Token
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect() // 开启自动重连，非常重要
                .Build();

            // --- 注册所有需要监听的服务器事件 ---

            // 监听 "ReceiveMessage" 事件
            _hubConnection.On<ChatMessageDto>("ReceiveMessage", (message) =>
            {
                Debug.WriteLine($"[SignalR] Message received from SenderId: {message.SenderId}");
                // 当接收到消息时，触发C#事件，通知所有订阅者（ViewModel）
                OnMessageReceived?.Invoke(message);
            });

            _hubConnection.On<int, List<long>>("MessagesRead", (readerId, readMessageIds) =>
            {
                OnMessagesRead?.Invoke(readerId, readMessageIds);
            });

            try
            {
                await _hubConnection.StartAsync();
                Debug.WriteLine("[SignalR] Connection successful.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SignalR] Connection failed: {ex.Message}");
            }
        }
        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                // 取消所有事件监听，防止内存泄漏
                _hubConnection.Remove("ReceiveMessage");

                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                Debug.WriteLine("[SignalR] Disconnected.");
            }
        }
        // 定义一个事件，当接收到新消息时触发
        public event Action<ChatMessageDto> OnMessageReceived;
        public event Action<int, List<long>> OnMessagesRead;
        public event Action<int> OnUserOnline;
        public event Action<int> OnUserOffline;

        // WPF应用可以调用这个方法来通过Hub发送消息
        public async Task SendMessageAsync(SendMessageDto message)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                // 调用后端ChatHub上的SendMessage方法
                await _hubConnection.InvokeAsync("SendMessage", message);
            }
        }


    }
}