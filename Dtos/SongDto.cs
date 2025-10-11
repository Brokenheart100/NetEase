using System.Text.Json.Serialization; // 1. 引入这个命名空间

    namespace NetEase.Dtos
    {
        public class SongDto
        {
            // 2. 为每个属性添加 JsonPropertyName 特性
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("duration")]
            public string Duration { get; set; }

            [JsonPropertyName("albumTitle")]
            public string AlbumTitle { get; set; } // 3. 保留这个

            [JsonPropertyName("artistName")]
            public string ArtistName { get; set; } // 3. 保留这个

            [JsonPropertyName("coverImageUrl")]
            public string? CoverImageUrl { get; set; }

            [JsonPropertyName("filePath")]
            public string FilePath { get; set; }
            [JsonPropertyName("isliked")]
            public bool IsLiked { get; set; } = false;

    }
}

