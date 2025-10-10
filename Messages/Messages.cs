using CommunityToolkit.Mvvm.Messaging.Messages;
using NetEase.Dtos;
using NetEase.Models;
using NetEase.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetEase.Messages
{
    public class LoginSuccessMessage(LoginResponse loginResponse) : ValueChangedMessage<LoginResponse>(loginResponse)
    {
    }
    public class PlaybackStatusChangedMessage(PlaybackStatus status) : ValueChangedMessage<PlaybackStatus>(status)
    {
    }

    // 用于替换 CurrentSongChanged event
    public class CurrentSongChangedMessage(Song song) : ValueChangedMessage<Song>(song)
    {
    }

    public record ProgressUpdatedPayload(TimeSpan CurrentTime, TimeSpan TotalTime);
    public class ProgressUpdatedMessage(TimeSpan currentTime, TimeSpan totalTime) : ValueChangedMessage<ProgressUpdatedPayload>(new ProgressUpdatedPayload(currentTime, totalTime))
    {
    }

    // --- Messages for PlayerService -> MediaPlayerService ---

    // 用于替换 PlayRequested event
    public class PlayRequestedMessage : ValueChangedMessage<Song>
    {
        public PlayRequestedMessage(Song song) : base(song) { }
    }

    // 用于替换 SeekRequested event
    public class SeekRequestedMessage : ValueChangedMessage<double>
    {
        public SeekRequestedMessage(double percentage) : base(percentage) { }
    }

    // 用于替换 VolumeChanged event
    public class VolumeChangedMessage : ValueChangedMessage<double>
    {
        public VolumeChangedMessage(double volume) : base(volume) { }
    }

    // --- Message for PlayerControlViewModel -> MainViewModel ---

    // 用于替换 ShowSongDetailRequested event
    public class ShowSongDetailMessage : ValueChangedMessage<Song>
    {
        public ShowSongDetailMessage(Song song) : base(song) { }
    }
}
