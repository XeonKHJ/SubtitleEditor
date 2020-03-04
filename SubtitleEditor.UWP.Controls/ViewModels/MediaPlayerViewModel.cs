using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static SubtitleEditor.UWP.Controls.FramedMediaTransportControls;

namespace SubtitleEditor.UWP.Controls.ViewModels
{
    public class MediaPlayerViewModel : INotifyPropertyChanged
    {
        private FrameMediaPlayer _frameMediaPlayer;
        public MediaPlayerViewModel()
        {
            _frameMediaPlayer = new FrameMediaPlayer();
        }

        public MediaPlayerViewModel(FrameMediaPlayer mediaPlayer)
        {
            _frameMediaPlayer = mediaPlayer;

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
        public void LoadMediaPlayer(FrameMediaPlayer mediaPlayer)
        {
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
            if (_frameMediaPlayer.PlaybackSession.Position - StartTimeOffset <= TimeSpan.Zero)
            {
                Position = TimeSpan.Zero;
            }
            else
            {
                Position = _frameMediaPlayer.PlaybackSession.Position - StartTimeOffset;
            }
        }

        public void UpdateDuration()
        {
            Duration = _frameMediaPlayer.PlaybackSession.NaturalDuration;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MediaTimeSpanFormatter : IValueConverter
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

            if (transportValueType == TransportValueType.Time)
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

    public class MediaSliderValueFormatter : IValueConverter
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

    public class MediaSliderToolTipFormatter : IValueConverter
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

            if (transportValueType == TransportValueType.Frame)
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
