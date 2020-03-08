using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Globalization;

namespace SubtitleEditor.UWP.Controls
{
    public class FrameMediaPlayer : IDisposable
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
            await frameMediaPlayer.CalculateFrameRateFromFileAsync(mediaFile).ConfigureAwait(false);
            return frameMediaPlayer;
        }

        public FrameMediaPlayer()
        {
            MediaPlayer = new MediaPlayer()
            {
                IsVideoFrameServerEnabled = true
            };
            RegisterMediaPlayerEvent();
        }
        private FrameMediaPlayer(IMediaPlaybackSource source)
        {
            RegisterMediaPlayerEvent();
            MediaPlayer.Source = source;
        }

        /// <summary>
        /// 从文件加载媒体，如之前有加载媒体，则会将其关闭。
        /// </summary>
        /// <param name="mediaFile"></param>
        /// <returns></returns>
        public async Task LoadFileAsync(IStorageFile mediaFile)
        {
            await CalculateFrameRateFromFileAsync(mediaFile).ConfigureAwait(false);
            Source = MediaSource.CreateFromStorageFile(mediaFile);
        }

        public async Task LoadStreamAsync(IRandomAccessStream stream, string contentType)
        {
            await CalculateFrameRateFromStreamAsync(stream).ConfigureAwait(false);
            Source = MediaSource.CreateFromStream(stream, contentType);
        }

        private MediaSource _mediaSource;
        public IMediaPlaybackSource Source
        {
            private set
            {
                IsOpened = false;
                MediaPlayer.Source = value;
                _mediaSource = value as MediaSource;
            }
            get
            {
                return MediaPlayer.Source;
            }
        }

        private void MediaPlayer_VideoFrameAvailable(MediaPlayer sender, object args)
        {
            VideoFrameAvailable?.Invoke(this, args);

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
                return TimeSpan.FromSeconds(FrameCounts / FrameRate);
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
            set
            {
                if (value + StartOffset < Duration)
                {
                    PlaybackSession.Position = value + StartOffset;
                }
            }
        }
        public MediaPlayer MediaPlayer { get; private set; } = new MediaPlayer();
        public MediaPlaybackSession PlaybackSession { get { return MediaPlayer.PlaybackSession; } }
        public bool IsOpened { get; private set; }
        public double FrameRate { get; private set; } = 30;
        public TimeSpan StartOffset { private set; get; }
        public long CurrentFrame
        {
            get
            {
                return (long)Math.Round(Position.TotalSeconds / FrameRate);
            }
            set
            {
                var frameOffset = 0.0004 * FrameRate;

                double offsetSeconds = (value + frameOffset) / FrameRate;
                double seconds = value / FrameRate;

                var offsetPosition = new TimeSpan((long)(offsetSeconds * 10000000));
                //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Offset Position: {0}, OffsetFrame: {1}", TimeSpan.FromSeconds(offsetSeconds), value + frameOffset));
                //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "NoOffset Position: {0}, Frame: {1}", TimeSpan.FromSeconds(seconds), value));
                //System.Diagnostics.Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Offset Position: {0}", offsetPosition));

                //计算时间
                Position = offsetPosition;
            }
        }

        public long FrameCounts
        {
            get
            {
                return (long)(Duration.TotalSeconds * FrameRate);
            }
        }

        /// <summary>
        /// 播放媒体
        /// </summary>
        public void Play()
        {
            MediaPlayer.Play();
        }

        /// <summary>
        /// 暂停媒体
        /// </summary>
        public void Pause()
        {
            MediaPlayer.Pause();
        }

        public void Close()
        {
            if (_mediaSource != null)
            {
                MediaStatus = FrameMediaStatus.Closing;

                var oldMediaPlayer = MediaPlayer;
                oldMediaPlayer.VideoFrameAvailable -= MediaPlayer_VideoFrameAvailable;

                //关闭旧播放器
                oldMediaPlayer.Dispose();

                //关掉视频源
                _mediaSource.Dispose();
                _mediaSource = null;

                //新建MediaPlayer来取代之前的MediaPlayer
                MediaPlayer = new MediaPlayer()
                {
                    IsVideoFrameServerEnabled = true
                };
                RegisterMediaPlayerEvent();

                CurrentFrame = 0;

                MediaStatus = FrameMediaStatus.Closed;
            }
        }
        /// <summary>
        /// 将当前视频帧复制到Direct3DSurface目标。
        /// </summary>
        /// <param name="destination"></param>
        [HandleProcessCorruptedStateExceptions]
        public void CopyFrameToVideoSurface(IDirect3DSurface destination)
        {
            if(destination != null)
            {
                try
                {
                    MediaPlayer.CopyFrameToVideoSurface(destination);
                }
                catch(Exception exception)
                {
                    switch(exception.HResult)
                    {
                        //GPU 设备实例已经暂停
                        case -2005270523:
                            break;
                        default:
                            throw new Exception(exception.Message);
                    }
                }
            }
        }

        private void RegisterMediaPlayerEvent()
        {
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            MediaEnded?.Invoke(this, args);
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            CurrentStateChanged?.Invoke(this, args);
        }

        public enum FrameMediaStatus { Openning, Opened, Closing, Closed }
        public FrameMediaStatus MediaStatus { private set; get; } = FrameMediaStatus.Closed;
        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            MediaPlayer.VideoFrameAvailable -= MediaPlayer_VideoFrameAvailable;
            MediaPlayer.VideoFrameAvailable += CountingStartTimeOffset;

            renderedCounts = 0;
            isInitialReady = false;
            //等待起始位移时间计算完成
            StartCountingOffset();

            System.Diagnostics.Debug.WriteLine(string.Format("Frame Counts: {0}", FrameCounts));
            System.Diagnostics.Debug.WriteLine(string.Format("Fixed Duration: {0}", CorrectedDuration));
        }


        private void StartCountingOffset()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("StartCountingOffset: {0}", StartOffset));
        }

        int renderedCounts = 0;
        bool isInitialReady = false;
        private void CountingStartTimeOffset(MediaPlayer sender, object args)
        {

            if (!isInitialReady)
            {
                ++renderedCounts;
                if (renderedCounts > 2)
                {
                    StartOffset = sender.PlaybackSession.Position;
                    renderedCounts = 0;
                    isInitialReady = true;
                    MediaPlayer.VideoFrameAvailable -= CountingStartTimeOffset;
                    MediaPlayer.VideoFrameAvailable += MediaPlayer_VideoFrameAvailable;
                    MediaPlayer.IsVideoFrameServerEnabled = false;
                    MediaStatus = FrameMediaStatus.Opened;
                    MediaOpened?.Invoke(this, null);
                }
            }
        }

        private async Task CalculateFrameRateFromFileAsync(IStorageFile file)
        {
            MediaEncodingProfile profile = await MediaEncodingProfile.CreateFromFileAsync(file);
            FrameRate = (double)profile.Video.FrameRate.Numerator / profile.Video.FrameRate.Denominator;
        }

        private async Task CalculateFrameRateFromStreamAsync(IRandomAccessStream stream)
        {
            MediaEncodingProfile profile = await MediaEncodingProfile.CreateFromStreamAsync(stream);
            FrameRate = (double)profile.Video.FrameRate.Numerator / profile.Video.FrameRate.Denominator;
        }

        /// <summary>
        /// 释放该媒体播放器，包括绑定在该播放器中的媒体流。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (MediaPlayer.Source != null)
                {
                    MediaPlayer.Dispose();
                    MediaPlayer = null;
                }
                if (_mediaSource != null)
                {
                    _mediaSource.Dispose();
                    _mediaSource = null;
                }

            }
        }

        public event TypedEventHandler<FrameMediaPlayer, object> CurrentStateChanged;
        public event TypedEventHandler<FrameMediaPlayer, object> MediaOpened;
        public event TypedEventHandler<FrameMediaPlayer, object> MediaEnded;
        public event TypedEventHandler<FrameMediaPlayer, object> VideoFrameAvailable;
    }
}
