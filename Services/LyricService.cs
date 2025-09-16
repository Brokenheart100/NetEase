    using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NetEase.ViewModels;

namespace NetEase.Services
{
    public class LyricService
    {
        /// <summary>
        /// 尝试在与音频文件相同的目录下，查找并加载同名的.lrc外挂歌词文件。
        /// </summary>
        /// <param name="audioFilePath">音频文件的完整路径。</param>
        /// <returns>如果找到并成功解析，则返回歌词行列表；否则返回null。</returns>
        public List<LyricLine> GetLyricsFromLrcFile(string audioFilePath)
        {
            if (string.IsNullOrEmpty(audioFilePath))
            {
                return null;
            }

            try
            {
                // 1. 构建.lrc文件的预期路径
                // Path.ChangeExtension 会智能地将 "C:\music\song.mp3" 变为 "C:\music\song.lrc"
                string lrcFilePath = Path.ChangeExtension(audioFilePath, ".lrc");

                // 2. 检查.lrc文件是否存在
                if (File.Exists(lrcFilePath))
                {
                    Debug.WriteLine($"找到外挂歌词文件: '{lrcFilePath}'");
                    // 3. 如果存在，读取其所有文本内容
                    string lrcContent = File.ReadAllText(lrcFilePath);

                    // 4. 调用我们已有的解析方法来解析它
                    return ParseLrc(lrcContent);
                }
                else
                {
                    Debug.WriteLine($"在 '{Path.GetDirectoryName(audioFilePath)}' 中未找到对应的.lrc文件。");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"读取外挂歌词文件时出错: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 从指定的音频文件路径中尝试读取内嵌歌词。
        /// </summary>
        /// <param name="filePath">音频文件的完整路径。</param>
        /// <returns>如果找到并成功解析，则返回歌词行列表；否则返回null。</returns>
        public List<LyricLine> GetLyricsFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return null;
            }

            try
            {
                // 1. 使用 TagLib# 创建文件对象
                //    using 语句确保文件句柄在使用后被正确释放
                using (var tagFile = TagLib.File.Create(filePath))
                {
                    // 2. 访问文件的标签 (Tag)
                    //    TagLib.Tag 对象包含了如标题、歌手、专辑等所有元数据
                    var tag = tagFile.Tag;

                    // 3. 【核心】获取歌词
                    //    内嵌歌词通常存储在 Lyrics 属性中
                    string embeddedLyrics = tag.Lyrics;

                    if (string.IsNullOrEmpty(embeddedLyrics))
                    {
                        Debug.WriteLine($"文件 '{filePath}' 中没有找到内嵌歌词。");
                        return null; // 文件中没有歌词
                    }

                    // 4. 解析 LRC 格式的歌词文本
                    return ParseLrc(embeddedLyrics);
                }
            }
            catch (Exception ex)
            {
                // 捕获可能发生的异常，如文件损坏、不支持的格式等
                Debug.WriteLine($"读取或解析文件 '{filePath}' 的歌词时出错: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析 LRC 格式的歌词字符串。
        /// </summary>
        /// <param name="lrcContent">包含LRC标签的歌词文本。</param>
        /// <returns>一个 LyricLine 对象列表。</returns>
        private List<LyricLine> ParseLrc(string lrcContent)
        {
            var lyrics = new List<LyricLine>();
            // 正则表达式，用于匹配时间标签 [mm:ss.xx]
            var regex = new Regex(@"\[(\d{2}):(\d{2})\.(\d{2,3})\](.*)");

            var lines = lrcContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    int minutes = int.Parse(match.Groups[1].Value);
                    int seconds = int.Parse(match.Groups[2].Value);
                    int milliseconds = int.Parse(match.Groups[3].Value.PadRight(3, '0')); // 保证是3位数

                    var time = new TimeSpan(0, 0, minutes, seconds, milliseconds);
                    var text = match.Groups[4].Value.Trim();

                    // TODO: 这里可以进一步处理翻译歌词等复杂情况
                    lyrics.Add(new LyricLine { Time = time, OriginalText = text });
                }
            }

            // 按时间排序
            lyrics.Sort((a, b) => a.Time.CompareTo(b.Time));
            return lyrics;
        }
    }
}
