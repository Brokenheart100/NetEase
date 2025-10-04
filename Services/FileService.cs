using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace NetEase.Services
{
    public class FileService
    {
        private readonly HttpClient _httpClient;

        public FileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadFileAsync(string localFilePath)
        {
            if (string.IsNullOrEmpty(localFilePath) || !File.Exists(localFilePath))
            {
                return null;
            }

            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(localFilePath);

                // 将文件流添加到表单数据中
                // "file" 这个名字必须和后端API的参数名 IFormFile file 一致
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(localFilePath));

                // 发送POST请求到上传API
                var response = await _httpClient.PostAsync("api/files/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    // 解析返回的JSON: { "url": "/images/chat/..." }
                    var responseJson = await response.Content.ReadFromJsonAsync<JsonObject>();
                    return responseJson?["url"]?.GetValue<string>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"File upload failed: {ex.Message}");
                return null;
            }
        }
    }
}
