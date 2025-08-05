using NetEase.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NetEase.Views;

namespace NetEase.Views.Pages
{
    // 数据模型：用于精选推荐的大卡片
    public class RecommendationCard
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; } // e.g., "免费听"
        public List<string> Tracklist { get; set; }
    }
    /// <summary>
    /// FeaturedView.xaml 的交互逻辑
    /// </summary>
    public partial class FeaturedView : UserControl
    {// 新增：榜单精选数据集合
        public ObservableCollection<RankingList> RankingLists { get; set; }
        // 新增：轮播图数据属性
        // 新增：猜你喜欢卡片的数据集合
        public ObservableCollection<SuggestionCard> Suggestions { get; set; }
        public CarouselItem CurrentCarouselItem { get; set; }
        public ObservableCollection<RecommendationCard> Recommendations { get; set; }
        public FeaturedView()
        {
            InitializeComponent();
            // 初始化示例数据
            Recommendations = new ObservableCollection<RecommendationCard>
            {
                new RecommendationCard
                {
                    ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\10.jpg",
                    Title = "每日推荐",
                    Description = "从「stand still」...",
                    Tag = "免费听",
                    Tracklist = new List<string>
                    {
                        "1 ユキトキ (TV Si...",
                        "2 Avid",
                        "3 願い〜あ..."
                    }
                },
                new RecommendationCard
                {
                    ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\12.jpg",
                    Title = "私人漫游",
                    Description = "从「コメットルシファー」...",
                    Tag = null, // 没有标签
                    Tracklist = new List<string>
                    {
                        "1 ユキトキ (TV Si...",
                        "2 Avid",
                        "3 願い〜あ..."
                    }
                },
                new RecommendationCard
                {
                    ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\13.jpg",
                    Title = "私人雷达",
                    Description = "今天从《灼け落ちない翼》听起",
                    Tag = null,
                    Tracklist = new List<string>
                    {
                        "1 ユキトキ (TV Si...",
                        "2 Avid",
                        "3 願い〜あ..."
                    }
                },
                new RecommendationCard
                {
                    ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\10.jpg",
                    Title = "华语流行日推",
                    Description = "着魔、很久很久、颜色",
                    Tag = null,
                    Tracklist = new List<string>
                    {
                        "1 ユキトキ (TV Si...",
                        "2 Avid",
                        "3 願い〜あ..."
                    }
                },
                new RecommendationCard
                {
                    ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\10.jpg",
                    Title = "心情氛围歌单",
                    Description = "Downtempo灵性觉醒 | 属于夜晚...",
                    Tag = null,
                    Tracklist = new List<string>
                    {
                        "1 ユキトキ (TV Si...",
                        "2 Avid",
                        "3 願い〜あ..."
                    }
                },
            };


            // 初始化轮播图的示例数据
            CurrentCarouselItem = new CarouselItem
            {
                Title = "人气女子组合 TWICE",
                SubTitle = "最新单曲",
                MainText = "ENEMY",
                Description = "第六张日语专辑先行曲上线",
                ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\18.jpg",
                BackgroundImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\26.jpg", // 背景大图
                Tag = "新歌首发"
            };

            // 新增：初始化“猜你喜欢”的示例数据
            Suggestions = new ObservableCollection<SuggestionCard>
            {
                new SuggestionCard { ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\18.jpg", Title = "浪里个浪", Tag = "播客" },
                new SuggestionCard { ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\9.jpg", Title = "凹凸电波", Tag = "播客" },
                new SuggestionCard { ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\25.jpg.jpg", Title = "呼叫仙贝", Tag = "播客" },
                new SuggestionCard { ImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\19.jpg.jpg", Title = "<画面感极强的BGM>", Tag = "歌单" },
            };
            this.DataContext = this;
            // 新增：初始化榜单数据
            RankingLists = new ObservableCollection<RankingList>
            {
                new RankingList
                {
                    Title = "飙升榜", UpdateInfo = "刚刚更新", CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\16.jpg",
                    TopSongs = new List<RankedSong>
                    {
                        new RankedSong { Rank = 1, Title = "大城小爱", Artist = "梓渝", Status = RankStatus.New },
                        new RankedSong { Rank = 2, Title = "萤火星球", Artist = "梓渝", Status = RankStatus.New },
                        new RankedSong { Rank = 3, Title = "暗流", Artist = "石凯/万妮达", Status = RankStatus.Up },
                    }
                },
                new RankingList
                {
                    Title = "新歌榜", UpdateInfo = "更新12首", CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\36.jpg",
                    TopSongs = new List<RankedSong>
                    {
                        new RankedSong { Rank = 1, Title = "大城小爱", Artist = "梓渝", Status = RankStatus.New },
                        new RankedSong { Rank = 2, Title = "萤火星球", Artist = "梓渝", Status = RankStatus.Down },
                        new RankedSong { Rank = 3, Title = "洗牌", Artist = "SHarK/Rapeter", Status = RankStatus.Down },
                    }
                },
                new RankingList
                {
                    Title = "飙升榜", UpdateInfo = "刚刚更新", CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\26.jpg",
                    TopSongs = new List<RankedSong>
                    {
                        new RankedSong { Rank = 1, Title = "大城小爱", Artist = "梓渝", Status = RankStatus.New },
                        new RankedSong { Rank = 2, Title = "萤火星球", Artist = "梓渝", Status = RankStatus.New },
                        new RankedSong { Rank = 3, Title = "暗流", Artist = "石凯/万妮达", Status = RankStatus.Up },
                    }
                },
                new RankingList
                {
                    Title = "新歌榜", UpdateInfo = "更新12首", CoverImageUrl = "E:\\Computer\\VS\\NetEase\\CoverImage\\5.jpg",
                    TopSongs = new List<RankedSong>
                    {
                        new RankedSong { Rank = 1, Title = "大城小爱", Artist = "梓渝", Status = RankStatus.New },
                        new RankedSong { Rank = 2, Title = "萤火星球", Artist = "梓渝", Status = RankStatus.Down },
                        new RankedSong { Rank = 3, Title = "洗牌", Artist = "SHarK/Rapeter", Status = RankStatus.Down },
                    }
                }

            };

        }

    }
}
