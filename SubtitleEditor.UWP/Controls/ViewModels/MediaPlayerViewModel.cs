using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Data;
using SubtitleEditor.UWP.Controls;
using Windows.Foundation;
using static SubtitleEditor.UWP.Controls.StandAloneMediaTransportControls;

namespace SubtitleEditor.UWP.Controls.Old.ViewModels
{
    public class MediaPlayerViewModel : INotifyPropertyChanged
    {
        private MediaPlayer _mediaPlayer;
        public MediaPlayerViewModel()
        {
            _mediaPlayer = new MediaPlayer();
        }

        public MediaPlayerViewModel(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private TimeSpan position;
        public TimeSpan Position
        {
            set
            {
                position = value;
                OnPropertyChanged();
            }
            get
            {
                return position;
            }
        }

        private TimeSpan duration;
        public TimeSpan Duration
        {
            set
            {
                duration = value;
                OnPropertyChanged();
            }
            get
            {
                return duration;
            }
        }

        public TimeSpan StartTimeOffset { set; get; }
        public void LoadMediaPlayer(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            StartTimeOffset = mediaPlayer.PlaybackSession.Position;
            System.Diagnostics.Debug.WriteLine(string.Format("Start Time Offset - {0}", StartTimeOffset));
            UpdateAllProperty();
        }

        public void UpdateAllProperty()
        {
            UpdatePosition();
            UpdateDuration();
        }

        public void UpdatePosition()
        {
            if(_mediaPlayer.PlaybackSession.Position - StartTimeOffset <= TimeSpan.Zero)
            {
                Position = TimeSpan.Zero;
            }
            else
            {
                Position = _mediaPlayer.PlaybackSession.Position - StartTimeOffset;
            }
        }
        
        public void UpdateDuration()
        {
            Duration = _mediaPlayer.PlaybackSession.NaturalDuration;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class MediaTimeSpanFormatter : IValueConverter
    {
        public double FrameRate { set; get; }
        public MediaTimeSpanFormatter(double fps)
        {
            FrameRate = fps;
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TransportValueType transportValueType = (TransportValueType)parameter;
            TimeSpan timeSpan = (TimeSpan)value;

            if(transportValueType == TransportValueType.Time)
            {
                return timeSpan.ToString(@"hh\:mm\:ss\,fff");
            }
            else
            {
                return System.Convert.ToInt32(timeSpan.TotalSeconds * FrameRate);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    internal class MediaSliderValueFormatter : IValueConverter
    {
        public double FrameRate { set; get; }
        public TimeSpan InitialOffset { set; get; }
        public MediaSliderValueFormatter(double fps, TimeSpan initialOffset)
        {
            InitialOffset = initialOffset;
            FrameRate = fps;
        }

        /// <summary>
        /// 从timeSpan转换到帧数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            return timeSpan.TotalSeconds * FrameRate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double sliderValue = (double)value;
            TimeSpan timeSpan = new TimeSpan(System.Convert.ToInt64(sliderValue / FrameRate * 10000000)) + InitialOffset;
            OnConvertBacked?.Invoke(this, timeSpan);
            return timeSpan;
        }

        public event TypedEventHandler<MediaSliderValueFormatter, TimeSpan> OnConvertBacked;
    }

    internal class MediaSliderToolTipFormatter : IValueConverter
    {
        private double FrameRate { set; get; }
        public MediaSliderToolTipFormatter(double fps)
        {
            FrameRate = fps;
        }

        /// <summary>
        /// 把进度条传来的值进行转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TransportValueType transportValueType = (TransportValueType)parameter;
            double sliderValue = (double)transportValueType;

            if(transportValueType == TransportValueType.Frame)
            {
                return System.Convert.ToInt32(sliderValue).ToString();
            }
            else
            {
                var totalSeconds = sliderValue / FrameRate;
                TimeSpan timeSpan = new TimeSpan(System.Convert.ToInt64(totalSeconds * 1000000));
                return timeSpan.ToString(@"hh\:mm\:ss\,fff");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
