using NetEase.ViewModels.ChatViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetEase.Dtos;

namespace NetEase.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ChatMessage>> GetHistoryAsync(int friendId)
        {
            Debug.WriteLine($"ChatService Enter {nameof(GetHistoryAsync)}({friendId})");
            try
            {
                // GET api/chat/history/{friendId}
                return await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/chat/history/{friendId}");
            }
            catch (HttpRequestException ex)
            {
             
                    Debug.WriteLine($"HTTP请求失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"内部异常: {ex.InnerException.Message}");
                    }
                
            return new List<ChatMessage>();
            }
        }
            
        public async Task<ChatMessage> SendMessageAsync(int receiverId, string content, string mimeType = null)
        {
            //var messageDto = new { ReceiverId = receiverId, Content = content };
            var messageDto = new { ReceiverId = receiverId, Content = content, MimeType = mimeType };
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/chat/send", messageDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ChatMessage>();
                }
                return null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP请求失败: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"内部异常: {ex.InnerException.Message}");
                }
                return null;
            }
        }
       
        public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto messageDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/chat/send", messageDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChatMessageDto>();
            }
            return null;
        }
        public async Task<List<ChatSessionDto>> GetSessionsAsync()
        {
            try
            {
                // DTO需要先在前端定义
                return await _httpClient.GetFromJsonAsync<List<ChatSessionDto>>("api/chat/sessions");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get sessions: {ex.Message}");
                return new List<ChatSessionDto>();
            }
        }
        public async Task MarkMessagesAsReadAsync(int contactId)
        {
            try
            {
                await _httpClient.PostAsync($"api/chat/read/{contactId}", null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to mark messages as read: {ex.Message}");
            }
        }
    }
}
