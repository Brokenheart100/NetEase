using System.Text.Json.Serialization;

namespace NetEase.Dtos
{
    // 这个类对应后端返回的整个JSON对象
    public class SearchResponseDto
    {
        // 使用 [JsonPropertyName] 特性来确保能正确映射 JSON 中的 camelCase 命名
        [JsonPropertyName("tookMilliseconds")]
        public long TookMilliseconds { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }

        //[JsonPropertyName("results")]
        //public Dictionary<string, SearchResultCategoryDto> Results { get; set; }
        [JsonPropertyName("results")]
        public SearchResultsDto Results { get; set; }
    }

    // 这个类对应 JSON 中的 "results" 对象
    public class SearchResultsDto
    {
        // 使用字典来灵活处理不同类型的搜索结果 (songs, artists, etc.)
        // [JsonExtensionData] 是一种高级用法，更简单的方式是直接定义属性
        [JsonPropertyName("songs")]
        public SearchResultCategoryDto Songs { get; set; }

        // 你可以继续添加其他类型
        // [JsonPropertyName("artists")]
        // public SearchResultCategoryDto Artists { get; set; }
    }

    // 这个类对应 JSON 中 "songs" 或 "artists" 这样的分类对象
    public class SearchResultCategoryDto
    {
        [JsonPropertyName("total")]
        public long Total { get; set; }

        // Items 是一个 SongDto 对象的列表
        [JsonPropertyName("items")]
        public List<SongDto> Items { get; set; }
    }

    // SongDto 也应该在这里定义，或者在一个共享的 Dtos 文件中
    // 确保它的属性与后端返回的歌曲对象一致

}
