using SubtitleEditor.UWP.Controls.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace SubtitleEditor.UWP.Controls
{
    public sealed class FramedMediaTransportControls : Control
    {
        public FramedMediaTransportControls()
        {
            this.DefaultStyleKey = typeof(FramedMediaTransportControls);
        }
        private readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public void DispatcherTimerSetup()
        {
            dispatcherTimer.Tick += DispatcherTimer_Tick; ;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            MediaPlayerViewModel.UpdatePosition();
            System.Diagnostics.Debug.WriteLine(MediaPlayerViewModel.Position);
        }

        private Slider _mediaSlider;
        private AppBarButton _playButton;
        private TextBlock _endPostionBlock;
        private TextBlock _startPostionBlock;

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
            _playButton = GetTemplateChild("PlayButton") as AppBarButton;
            _playButton.Click += ((sender, args) =>
            {
                if (FrameMediaPlayer != null)
                {
                    if (FrameMediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                    {
                        FrameMediaPlayer.Play();
                    }
                    else
                    {
                        if (FrameMediaPlayer.PlaybackSession.CanPause)
                        {
                            FrameMediaPlayer.Pause();
                        }

                    }
                }
            });

            //_mediaSlider.ThumbToolTipValueConverter = PositionConverter;

            RenderControl();

            BindingViewModel();

            DispatcherTimerSetup();
        }

        private void RenderControl()
        {
            _endPostionBlock.Visibility = IsEndTextShown ? Visibility.Visible : Visibility.Collapsed;
            _startPostionBlock.Visibility = IsStartTextShown ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BindingViewModel()
        {
            Binding startBlockBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Position"),
                Converter = new MediaTimeSpanFormatter(FrameRate),
                ConverterParameter = ValueType
            };

            Binding endBlockBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Duration"),
                Converter = new MediaTimeSpanFormatter(FrameRate),
                ConverterParameter = ValueType
            };

            Binding sliderMaxValueBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Duration"),
                Converter = new MediaSliderValueFormatter(FrameRate, MediaPlayerViewModel.StartTimeOffset),
                ConverterParameter = ValueType
            };

            System.Diagnostics.Debug.WriteLine(string.Format("Slider Max Value - {0}", _mediaSlider.Maximum));

            MediaSliderValueFormatter mediaSliderValueFormatter = new MediaSliderValueFormatter(FrameRate, MediaPlayerViewModel.StartTimeOffset);
            mediaSliderValueFormatter.OnConvertBacked += ((formatter, timeSpan) =>
            {
                if (MediaPlayer != null)
                {
                    FrameMediaPlayer.PlaybackSession.Position = timeSpan;
                }
            });
            Binding sliderValueBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Position"),
                Converter = mediaSliderValueFormatter,
                Mode = BindingMode.TwoWay,
                ConverterParameter = ValueType
            };

            _startPostionBlock.SetBinding(TextBlock.TextProperty, startBlockBinding);
            _endPostionBlock.SetBinding(TextBlock.TextProperty, endBlockBinding);
            _mediaSlider.SetBinding(Slider.MaximumProperty, sliderMaxValueBinding);
            _mediaSlider.SetBinding(Slider.ValueProperty, sliderValueBinding);
        }

        private MediaPlayerViewModel MediaPlayerViewModel { set; get; } = new MediaPlayerViewModel();
        private MediaPlayer MediaPlayer { get { return FrameMediaPlayer.MediaPlayer; } }
        public FrameMediaPlayer FrameMediaPlayer
        {
            get { return (FrameMediaPlayer)GetValue(MediaSourceProperty); }
            set { SetValue(MediaSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaSourceProperty =
            DependencyProperty.Register("FrameMediaPlayer", typeof(FrameMediaPlayer), typeof(FramedMediaTransportControls), new PropertyMetadata(null, OnMediaPlayerChanged));

        private static void OnMediaPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as FramedMediaTransportControls;
            FrameMediaPlayer newMediaPlayer = e.NewValue as FrameMediaPlayer;
            FrameMediaPlayer oldMediaPlayer = e.OldValue as FrameMediaPlayer;

            transportControl.FrameMediaPlayer = newMediaPlayer;

            if (oldMediaPlayer != null)
            {
                transportControl.UnRegeisterMediaPlayerEvent(oldMediaPlayer);
            }

            if (newMediaPlayer != null)
            {
                transportControl.RegisterMediaPlayerEvent(newMediaPlayer);
            }
        }

        private void RegisterMediaPlayerEvent(FrameMediaPlayer mediaPlayer)
        {
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private async void MediaPlayer_CurrentStateChanged(FrameMediaPlayer sender, object args)
        {
            switch (sender.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _playButton.Icon = new SymbolIcon(Symbol.Pause);
                        dispatcherTimer.Start();
                    });
                    break;
                case MediaPlaybackState.Paused:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _playButton.Icon = new SymbolIcon(Symbol.Play);
                        dispatcherTimer.Stop();
                    });
                    break;
                case MediaPlaybackState.None:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _playButton.Icon = new SymbolIcon(Symbol.Play);
                        dispatcherTimer.Stop();
                    });
                    break;
            }
        }

        private async void MediaPlayer_MediaOpened(FrameMediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal ,() =>
            {
                MediaPlayerViewModel.LoadMediaPlayer(sender);
                BindingViewModel();
            });
        }

        private void UnRegeisterMediaPlayerEvent(FrameMediaPlayer mediaPlayer)
        {
            mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
        }

        public enum TransportValueType { Time, Frame }
        private TransportValueType valueType;
        public TransportValueType ValueType
        {
            get { return (TransportValueType)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(TransportValueType), typeof(FramedMediaTransportControls), new PropertyMetadata(TransportValueType.Time, OnValueTypeChanged));

        private static void OnValueTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as FramedMediaTransportControls;
            TransportValueType valueType = (TransportValueType)e.NewValue;
            transportControl.valueType = valueType;
        }
        public double FrameRate
        {
            get { return (double)GetValue(FrameRateProperty); }
            set { SetValue(FrameRateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FrameRate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrameRateProperty =
            DependencyProperty.Register("FrameRate", typeof(double), typeof(FramedMediaTransportControls), new PropertyMetadata(29.97, OnFrameRateChanged));

        private static void OnFrameRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as FramedMediaTransportControls;
            double frameRate = (double)e.NewValue;
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
            DependencyProperty.Register("IsEndTextShown", typeof(bool), typeof(FramedMediaTransportControls), new PropertyMetadata(true, OnIsEndTextShownChanged));

        private static void OnIsEndTextShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isEndTextShown = (bool)e.NewValue;
            var transportControls = d as FramedMediaTransportControls;
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
            DependencyProperty.Register("IsStartTextShown", typeof(bool), typeof(FramedMediaTransportControls), new PropertyMetadata(true, OnIsStartTextShownChanged));

        private static void OnIsStartTextShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isStartTextShown = (bool)e.NewValue;
            var transportControls = d as FramedMediaTransportControls;

            if (transportControls._startPostionBlock != null)
            {
                transportControls._startPostionBlock.Visibility = isStartTextShown ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
}
