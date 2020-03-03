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


        public MediaPlayer MediaPlayer { get; private set; } = new MediaPlayer();

        public bool IsOpened { get; private set; }
        public double FrameRate { get; private set; }
        public TimeSpan StartOffset { set; private get; }

        private FrameMediaPlayer(IMediaPlaybackSource source)
        {
            RegsiterEvent();
            MediaPlayer.Source = source;
        }

        private void RegsiterEvent()
        {
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }
        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            StartOffset = sender.PlaybackSession.Position;
            Opened?.Invoke(this, null);
        }
        static async Task<FrameMediaPlayer> CreateFromStorageFileAsync(IStorageFile mediaFile)
        {
            var source = MediaSource.CreateFromStorageFile(mediaFile);
            FrameMediaPlayer frameMediaPlayer = new FrameMediaPlayer(source);
            await frameMediaPlayer.CalculateFrameRateAsync(mediaFile);
            return frameMediaPlayer;
        }

        private async Task CalculateFrameRateAsync(IStorageFile file)
        {
            MediaEncodingProfile profile = await MediaEncodingProfile.CreateFromFileAsync(file);
            FrameRate = (double)profile.Video.FrameRate.Numerator / (double)profile.Video.FrameRate.Denominator;

        }

        public event TypedEventHandler<FrameMediaPlayer, object> Opened;
    }
}
