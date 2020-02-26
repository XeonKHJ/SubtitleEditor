using SubtitleEditor.UWP.Controls.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

        DispatcherTimer dispatcherTimer;
        DateTimeOffset startTime;
        DateTimeOffset lastTime;
        DateTimeOffset stopTime;
        int timesTicked = 1;
        int timesToTick = 10;
        public void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick; ;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            if(mediaPlayer != null)
            {
                UpdateCurrentPosition();
            }
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

            BindingViewModel();

            DispatcherTimerSetup();
            //ValueUpdater();
            _visualized = true;
        }

        private void RenderControl()
        {
            _endPostionBlock.Visibility = IsEndTextShown ? Visibility.Visible : Visibility.Collapsed;
            _startPostionBlock.Visibility = IsStartTextShown ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BindingViewModel()
        {
            Binding binding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("StringPosition"),
            };

            _startPostionBlock.SetBinding(TextBlock.TextProperty, binding);
        }
        private MediaPlayerViewModel MediaPlayerViewModel { set; get; } = new MediaPlayerViewModel();

        private MediaPlayer mediaPlayer;
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

            transportControl.mediaPlayer = newMediaPlayer;

            transportControl.UnRegeisterMediaPlayerEvent(oldMediaPlayer);

            if (transportControl._visualized)
            {
                transportControl.UpdateControls();
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
            if (mediaPlayer != null)
            {
                MediaPlayerViewModel.StringPosition = StringCurrentValue;
                _mediaSlider.Value = SliderCurrentValue;
                //_startPostionBlock.Text = StringCurrentValue;
            }
        }
        private void UpdateDuration()
        {
            if (MediaPlayer != null)
            {
                //_mediaSlider.Maximum = SliderMaximum;
                //_endPostionBlock.Text = StringMaximum;
            }
        }
        private double SliderMaximum
        {
            get
            {
                var totalFrame = CalculateFrame(mediaPlayer.PlaybackSession.NaturalDuration, frameRate);
                return Convert.ToDouble(totalFrame);
            }
        }
        private double SliderCurrentValue
        {
            get
            {
                return CalculateFrame(mediaPlayer.PlaybackSession.Position, frameRate);
            }
        }

        private string StringMaximum
        {
            get
            {
                if (valueType == TransportValueType.Time)
                {
                    return MediaPlayer.PlaybackSession.NaturalDuration.ToString(@"hh\:mm\:ss\,fff");
                }
                else
                {
                    return CalculateFrame(mediaPlayer.PlaybackSession.NaturalDuration, frameRate).ToString();
                }
            }
        }

        private string StringCurrentValue
        {
            get
            {
                if (valueType == TransportValueType.Time)
                {
                    return mediaPlayer.PlaybackSession.Position.ToString(@"hh\:mm\:ss\,fff");
                }
                else
                {
                    return CalculateFrame(mediaPlayer.PlaybackSession.Position, frameRate).ToString();
                }
            }
        }

        private static int CalculateFrame(TimeSpan span, double frameRate)
        {
            return Convert.ToInt32(span.TotalSeconds * frameRate);
        }
        public enum TransportValueType { Time, Frame }
        private TransportValueType valueType;
        public TransportValueType ValueType
        {
            get { return (TransportValueType)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(TransportValueType), typeof(StandAloneMediaTransportControls), new PropertyMetadata(TransportValueType.Time, OnValueTypeChanged));

        private static void OnValueTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as StandAloneMediaTransportControls;
            TransportValueType valueType = (TransportValueType)e.NewValue;
            transportControl.valueType = valueType;
        }

        private double frameRate;
        public double FrameRate
        {
            get { return (double)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FrameRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(double), typeof(StandAloneMediaTransportControls), new PropertyMetadata(25.0, OnFrameRateChanged));

        private static void OnFrameRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as StandAloneMediaTransportControls;
            double frameRate = (double)e.NewValue;
            transportControl.frameRate = frameRate;
        }

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
}
