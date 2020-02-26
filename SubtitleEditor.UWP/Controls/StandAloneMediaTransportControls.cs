using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace SubtitleEditor.UWP.Controls
{
    public sealed class StandAloneMediaTransportControls : Control
    {
        public StandAloneMediaTransportControls()
        {
            this.DefaultStyleKey = typeof(StandAloneMediaTransportControls);
        }

        private Slider _mediaSlider;
        private TextBlock _endPostionBlock;
        private TextBlock _startPostionBlock;
        private bool _visualized = false;
        protected override void OnBringIntoViewRequested(BringIntoViewRequestedEventArgs e)
        {
            base.OnBringIntoViewRequested(e);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _endPostionBlock = GetTemplateChild("EndPositionBlock") as TextBlock;
            _startPostionBlock = GetTemplateChild("StartPositionBlock") as TextBlock;
            _mediaSlider = GetTemplateChild("MediaSlider") as Slider;

            //_mediaSlider.ThumbToolTipValueConverter = PositionConverter;

            RenderControl();

            UpdateControls();

            _visualized = true;
        }

        private void RenderControl()
        {
            _endPostionBlock.Visibility = IsEndTextShown ? Visibility.Visible : Visibility.Collapsed;
            _startPostionBlock.Visibility = IsStartTextShown ? Visibility.Visible : Visibility.Collapsed;
        }

        private IValueConverter TimelineConverter
        {
            get
            {
                if (ValueType == TransportValueType.Frame)
                {
                    return new TimeSpanToFrameFormatter(this.FrameRate);
                }
                else
                {
                    return new TimeSpanToTicksFormatter();
                }
            }
        }

        private IValueConverter PositionConverter
        {
            get
            {
                if (ValueType == TransportValueType.Frame)
                {
                    return new TimeSpanToFrameFormatter(this.FrameRate);
                }
                else
                {
                    return new TimeSpanFormatter();
                }
            }
        }
        public MediaPlayer MediaPlayer
        {
            get { return (MediaPlayer)GetValue(MediaSourceProperty); }
            set { SetValue(MediaSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaSourceProperty =
            DependencyProperty.Register("MediaPlayer", typeof(MediaPlayer), typeof(StandAloneMediaTransportControls), new PropertyMetadata(null, OnMediaPlayerChanged));

        private static void OnMediaPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as StandAloneMediaTransportControls;
            MediaPlayer newMediaPlayer = e.NewValue as MediaPlayer;
            MediaPlayer oldMediaPlayer = e.OldValue as MediaPlayer;

            transportControl.UnRegeisterMediaPlayerEvent(oldMediaPlayer);

            if (transportControl._visualized)
            {
                transportControl.UpdateControls();
            }

            transportControl.RegeisterMediaPlayerEvent(newMediaPlayer);
        }

        private void RegeisterMediaPlayerEvent(MediaPlayer newMediaPlayer)
        {
            if(MediaPlayer != null)
            {
                MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            }
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            UpdateCurrentPosition();
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            UpdateControls();
        }

        private void UnRegeisterMediaPlayerEvent(MediaPlayer newMediaPlayer)
        {
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            MediaPlayer.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
        }

        private void UpdateControls()
        {
            UpdateCurrentPosition();
            UpdateDuration();
        }

        private void UpdateCurrentPosition()
        {
            if(MediaPlayer != null)
            {
                _mediaSlider.Value = SliderCurrentValue;
                _startPostionBlock.Text = StringCurrentValue;
            }
        }
        private void UpdateDuration()
        {
            if(MediaPlayer != null)
            {
                _mediaSlider.Maximum = SliderMaximum;
                _endPostionBlock.Text = StringMaximum;
            }
        }
        private double SliderMaximum
        {
            get
            {
                var totalFrame = CalculateFrame(MediaPlayer.PlaybackSession.NaturalDuration, FrameRate);
                return Convert.ToDouble(totalFrame);
            }
        }
        private double SliderCurrentValue
        {
            get
            {
                return CalculateFrame(MediaPlayer.PlaybackSession.Position, FrameRate);
            }
        }

        private string StringMaximum
        {
            get
            {
                if (ValueType == TransportValueType.Time)
                {
                    return MediaPlayer.PlaybackSession.NaturalDuration.ToString(@"hh\:mm\:ss\,fff");
                }
                else
                {
                    return CalculateFrame(MediaPlayer.PlaybackSession.NaturalDuration, FrameRate).ToString();
                }
            }
        }

        private string StringCurrentValue
        {
            get
            {
                if (ValueType == TransportValueType.Time)
                {
                    return MediaPlayer.PlaybackSession.Position.ToString(@"hh\:mm\:ss\,fff");
                }
                else
                {
                    return CalculateFrame(MediaPlayer.PlaybackSession.Position, FrameRate).ToString();
                }
            }
        }

        private static int CalculateFrame(TimeSpan span, double frameRate)
        {
            return Convert.ToInt32(span.TotalSeconds * frameRate);
        }
        public enum TransportValueType { Time, Frame }
        public TransportValueType ValueType
        {
            get { return (TransportValueType)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(TransportValueType), typeof(StandAloneMediaTransportControls), new PropertyMetadata(TransportValueType.Time));

        public double FrameRate
        {
            get { return (double)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FrameRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(double), typeof(StandAloneMediaTransportControls), new PropertyMetadata(25.0));

        /// <summary>
        /// 末尾标记是否显示。
        /// </summary>
        public bool IsEndTextShown
        {
            get { return (bool)GetValue(IsEndTextShownProperty); }
            set { SetValue(IsEndTextShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEndTextShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEndTextShownProperty =
            DependencyProperty.Register("IsEndTextShown", typeof(bool), typeof(StandAloneMediaTransportControls), new PropertyMetadata(true, OnIsEndTextShownChanged));

        private static void OnIsEndTextShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isEndTextShown = (bool)e.NewValue;
            var transportControls = d as StandAloneMediaTransportControls;
            if (transportControls._endPostionBlock != null)
            {
                transportControls._endPostionBlock.Visibility = isEndTextShown ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 开始标记是否显示。
        /// </summary>
        public bool IsStartTextShown
        {
            get { return (bool)GetValue(IsStartTextShownProperty); }
            set { SetValue(IsStartTextShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsStartTextShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsStartTextShownProperty =
            DependencyProperty.Register("IsStartTextShown", typeof(bool), typeof(StandAloneMediaTransportControls), new PropertyMetadata(true, OnIsStartTextShownChanged));

        private static void OnIsStartTextShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isStartTextShown = (bool)e.NewValue;
            var transportControls = d as StandAloneMediaTransportControls;

            if (transportControls._startPostionBlock != null)
            {
                transportControls._startPostionBlock.Visibility = isStartTextShown ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public class SliderThumTooltipFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanToFrameFormatter : IValueConverter
    {
        double Frame { set; get; } = 25;
        public TimeSpanToFrameFormatter(double frame)
        {
            Frame = frame;
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)(Frame * ((TimeSpan)value).TotalSeconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new TimeSpan((long)((int.Parse((string)value)) * 1000000 * Frame));
        }
    }

    public class TimeSpanToTicksFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timeSpan = (TimeSpan)value;

            return timeSpan.Ticks;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new TimeSpan(System.Convert.ToInt64(value));
        }
    }

    public class TimeSpanFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timeSpan = (TimeSpan)value;

            return timeSpan.ToString(@"hh\:mm\:ss\,fff"); ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.ParseExact(((string)value), @"hh\:mm\:ss\,fff", null);
        }
    }

    public class MediaPlayerViewModel : INotifyPropertyChanged
    {
        private MediaPlayer mediaPlayer;
        public MediaPlayerViewModel()
        { }

        public MediaPlayerViewModel(MediaPlayer mediaPlayery)
        {
            this.mediaPlayer = mediaPlayer;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string stringPosition;
        public string StringPosition 
        { 
            set
            {

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StringPosition"));
            }
            get
            {

            }
        }
        public string StringDuration { set; get; }
        public double TimelinePosition { set; get; } 
        public double TimelineDuration { set; get; }
    }
}
