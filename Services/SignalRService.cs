using Microsoft.AspNetCore.SignalR.Client;
using NetEase.Dtos;
using System;
using System.Threading.Tasks;

namespace NetEase.Services
{
    public class SignalRService
    {
        private HubConnection _hubConnection;

        // 定义一个事件，当接收到新消息时触发
        public event Action<ChatMessageDto> OnMessageReceived;

        public async Task ConnectAsync(string token)
        {
            if (_hubConnection?.State == HubConnectionState.Connected) return;
            // http://localhost:5215
            // 后端Hub的地址
            var hubUrl = "http://localhost:5215/chathub"; // <-- 【重要】替换为您的后端地址和端口

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    // 在连接时，将JWT Token附加到请求头中进行身份验证
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect() // 开启自动重连
                .Build();

            // 【核心】注册一个方法来监听从服务器推送过来的"ReceiveMessage"事件
            _hubConnection.On<ChatMessageDto>("ReceiveMessage", (message) =>
            {
                // 当接收到消息时，触发我们自己的C#事件
                OnMessageReceived?.Invoke(message);
            });

            try
            {
                await _hubConnection.StartAsync();
                System.Diagnostics.Debug.WriteLine("SignalR Connected.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR Connection failed: {ex.Message}");
            }
        }

        // WPF应用可以调用这个方法来通过Hub发送消息
        public async Task SendMessageAsync(SendMessageDto message)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                // 调用后端ChatHub上的SendMessage方法
                await _hubConnection.InvokeAsync("SendMessage", message);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}