using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetEase.Models; // 假设 Comment 模型在这里
using NetEase.Services; // 假设 CommentService 在这里
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NetEase.ViewModels;

public partial class CommentViewModel : BaseViewModel
{
    private readonly CommentService _commentService;
    private string _subjectId; // 当前评论区的主题ID (e.g., "playlist_123")

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _newCommentText;

    [ObservableProperty]
    private int _characterCount = 1000;

    public ObservableCollection<Comment> Comments { get; } = new();

    public CommentViewModel(CommentService commentService)
    {
        _commentService = commentService;

        LoadDesignTimeData();

    }
    private void LoadDesignTimeData()
    {
        IsLoading = false;
        Comments.Add(new Comment
        {
            Id = "1",
            UserName = "亚历克思",
            AvatarUrl = "/CoverImage/25.jpg", // 假设你有示例头像图片
            Content = "若说高中对我意味着什么？三个词，恋爱，火影加高考。以至于听到熟悉的曲子甚至是见到人物的笑脸都会把我带回高中的午后，永远的旧的阳光，永远蒸腾热气的操场，宽松校服里羞涩笑着的姑娘，永远知道目标，永远看不到头的高中生活。你问我忍道是什么？做到更好，这就是在下的忍道",
            LikeCount = 1109,
            CreatedAt = new DateTime(2016, 5, 5),
            IsVip = true // 新增 IsVip 属性
        });
        Comments.Add(new Comment
        {
            Id = "2",
            UserName = "悠扬以外",
            AvatarUrl = "/CoverImage/28.jpg",
            Content = "每一首都深入人心，看过火影的同志们都知道，每一首歌后面深藏的故事",
            LikeCount = 650,
            CreatedAt = new DateTime(2016, 3, 12),
            IsVip = false
        });
    }
    // 一个简单的辅助方法来检测设计模式
  
    /// <summary>
    /// 初始化并加载评论区
    /// </summary>
    public async Task LoadCommentsAsync(string subjectId)
    {
        _subjectId = subjectId;
        IsLoading = true;
        Comments.Clear();

        try
        {
            var comments = await _commentService.GetCommentsAsync(subjectId);
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    Comments.Add(comment);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }

        // TODO: 在这里连接 SignalR 并加入评论组
        // await _signalRService.JoinCommentGroup(subjectId);
    }

    [RelayCommand]
    private async Task PostCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCommentText) || string.IsNullOrEmpty(_subjectId))
        {
            return;
        }

        var newComment = await _commentService.PostCommentAsync(_subjectId, NewCommentText);
        if (newComment != null)
        {
            // 乐观更新：直接将新评论添加到列表顶部
            // 如果使用SignalR，可以等待服务器推送，避免重复添加
            Comments.Insert(0, newComment);
            NewCommentText = string.Empty; // 清空输入框
        }
        else
        {
            // 显示错误提示
        }
    }
}
