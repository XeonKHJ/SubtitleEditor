using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;

namespace SubtitleEditor.UWP.Controls
{
    public class FrameMediaPlayer
    {
        /// <summary>
        /// 从文件构造该类
        /// </summary>
        /// <param name="mediaFile"></param>
        /// <returns></returns>
        static async Task<FrameMediaPlayer> CreateFromStorageFileAsync(IStorageFile mediaFile)
        {
            var source = MediaSource.CreateFromStorageFile(mediaFile);
            FrameMediaPlayer frameMediaPlayer = new FrameMediaPlayer(source);
            await frameMediaPlayer.CalculateFrameRateAsync(mediaFile);
            return frameMediaPlayer;
        }

        public FrameMediaPlayer()
        {
            MediaPlayer = new MediaPlayer();
        }
        private FrameMediaPlayer(IMediaPlaybackSource source)
        {
            RegisterMediaPlayerEvent();
            MediaPlayer.Source = source;
        }

        public IMediaPlaybackSource Source
        {
            set
            {
                IsOpened = false;   
                MediaPlayer.Source = value;
            }
            get
            {
                return MediaPlayer.Source;
            }
        }

        private void UnregisterMediaPlayerEvent()
        {
            MediaPlayer.VideoFrameAvailable -= MediaPlayer_VideoFrameAvailable;
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
        }

        private void MediaPlayer_VideoFrameAvailable(MediaPlayer sender, object args)
        {
            throw new NotImplementedException();
        }

        public TimeSpan CorrectedPosition
        {
            get
            {
                return TimeSpan.FromSeconds(CurrentFrame / FrameRate);
            }
        }

        /// <summary>
        /// 媒体的校正后时长（自然时长×帧率）
        /// </summary>
        public TimeSpan CorrectedDuration
        {
            get
            {
                return TimeSpan.FromSeconds(CurrentFrame / FrameRate);
            }
        }

        /// <summary>
        /// 媒体的自然时长
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return PlaybackSession.NaturalDuration;
            }
        }

        /// <summary>
        /// 经过起始偏移后的当前位置
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                return PlaybackSession.Position - StartOffset;
            }
        }
        public MediaPlayer MediaPlayer { get; private set; } = new MediaPlayer();
        public MediaPlaybackSession PlaybackSession { get { return MediaPlayer.PlaybackSession; } }
        public bool IsOpened { get; private set; }
        public double FrameRate { get; private set; }
        public TimeSpan StartOffset { private set; get; }
        public long CurrentFrame
        {
            get
            {
                return (long)(Position.Seconds / FrameRate);
            }
            set
            {
                ;
            }
        }

        public void Play()
        {
            MediaPlayer.Play();
        }
        public void Pause()
        {
            MediaPlayer.Pause();
        }

        public void StepBackwardOneFrame()
        {
            var currentPosition = CurrentFrame;
        }

        private void RegisterMediaPlayerEvent()
        {
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            CurrentStateChanged?.Invoke(this, args);
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            StartOffset = sender.PlaybackSession.Position;
            //等待起始位移时间计算完成
            StartCountingOffset();

            MediaOpened?.Invoke(this, null);
        }


        private void StartCountingOffset()
        {
            MediaPlayer.VideoFrameAvailable += CountingStartTimeOffset;

            while (isInitialReady) ;
            renderedCounts = 0;
            isInitialReady = false;
            System.Diagnostics.Debug.WriteLine(string.Format("StartCountingOffset: {0}", StartOffset));

            MediaPlayer.VideoFrameAvailable -= CountingStartTimeOffset;
        }

        int renderedCounts = 0;
        bool isInitialReady = false;
        private void CountingStartTimeOffset(MediaPlayer sender, object args)
        {
            ++renderedCounts;
            if (renderedCounts > 2)
            {
                StartOffset = sender.PlaybackSession.Position;
                MediaPlayer.VideoFrameAvailable -= MediaPlayer_VideoFrameAvailable;
                renderedCounts = 0;
            }
        }

        private async Task CalculateFrameRateAsync(IStorageFile file)
        {
            MediaEncodingProfile profile = await MediaEncodingProfile.CreateFromFileAsync(file);
            FrameRate = (double)profile.Video.FrameRate.Numerator / (double)profile.Video.FrameRate.Denominator;
        }

        public event TypedEventHandler<FrameMediaPlayer, object> CurrentStateChanged;
        public event TypedEventHandler<FrameMediaPlayer, object> MediaOpened;
    }
}
