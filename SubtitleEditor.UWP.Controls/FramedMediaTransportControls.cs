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
        }

        private Slider _mediaSlider;
        private AppBarButton _playButton;
        private TextBlock _endPostionBlock;
        private TextBlock _startPostionBlock;
        
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
            if(FrameMediaPlayer == null)
            {
                return;
            }

            Binding startBlockBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Position"),
                Converter = new MediaTimeSpanFormatter(FrameMediaPlayer.FrameRate),
                ConverterParameter = ValueType
            };

            Binding endBlockBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Duration"),
                Converter = new MediaTimeSpanFormatter(FrameMediaPlayer.FrameRate),
                ConverterParameter = ValueType
            };

            Binding sliderMaxValueBinding = new Binding
            {
                Source = MediaPlayerViewModel,
                Path = new PropertyPath("Duration"),
                Converter = new MediaSliderValueFormatter(FrameMediaPlayer.FrameRate),
                ConverterParameter = ValueType
            };

            MediaSliderValueFormatter mediaSliderValueFormatter = new MediaSliderValueFormatter(FrameMediaPlayer.FrameRate);
            mediaSliderValueFormatter.OnConvertBacked += ((formatter, timeSpan) =>
            {
                if (MediaPlayer != null)
                {
                    FrameMediaPlayer.CurrentFrame = timeSpan;
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

            if(_startPostionBlock != null)
            {
                _startPostionBlock.SetBinding(TextBlock.TextProperty, startBlockBinding);
            }
            if(_endPostionBlock != null)
            {
                _endPostionBlock.SetBinding(TextBlock.TextProperty, endBlockBinding);
            }
            if(_mediaSlider != null)
            {
                _mediaSlider.SetBinding(Slider.MaximumProperty, sliderMaxValueBinding);
                _mediaSlider.SetBinding(Slider.ValueProperty, sliderValueBinding);
            }
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
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private async void MediaPlayer_MediaEnded(FrameMediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal ,() =>
            {
                MediaPlayerViewModel.Position = MediaPlayerViewModel.Duration;
            });
            
            System.Diagnostics.Debug.WriteLine("Media is Ended.");
        }

        private async void MediaPlayer_CurrentStateChanged(FrameMediaPlayer sender, object args)
        {
            switch (sender.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if(_playButton != null)
                        {
                            _playButton.Icon = new SymbolIcon(Symbol.Pause);
                        }
                        sender.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
                        dispatcherTimer.Start();
                    });
                    break;
                case MediaPlaybackState.Paused:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if(_playButton != null)
                        {
                            _playButton.Icon = new SymbolIcon(Symbol.Play);
                        }
                        dispatcherTimer.Stop();
                        sender.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
                        sender.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
                    });
                    break;
                case MediaPlaybackState.None:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if(_playButton != null)
                        {
                            _playButton.Icon = new SymbolIcon(Symbol.Play);
                        }
                        dispatcherTimer.Stop();
                        sender.PlaybackSession.PositionChanged -= PlaybackSession_PositionChanged;
                        sender.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
                    });
                    break;
            }
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayerViewModel.UpdatePosition();
                System.Diagnostics.Debug.WriteLine("PlaybackSession_PositionChanged");
            });
        }

        private async void MediaPlayer_MediaOpened(FrameMediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal ,() =>
            {
                BindingViewModel();
                MediaPlayerViewModel.LoadMediaPlayer(sender);
                System.Diagnostics.Debug.WriteLine("Media Opened");
            });
        }

        private void UnRegeisterMediaPlayerEvent(FrameMediaPlayer mediaPlayer)
        {
            mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
        }

        public enum TransportValueType { Time, Frame }
        public TransportValueType ValueType
        {
            get { System.Diagnostics.Debug.WriteLine("GetValueTYpe"); return (TransportValueType)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(TransportValueType), typeof(FramedMediaTransportControls), new PropertyMetadata(TransportValueType.Time, OnValueTypeChanged));

        private static void OnValueTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transportControl = d as FramedMediaTransportControls;
            TransportValueType valueType = (TransportValueType)e.NewValue;
            transportControl.BindingViewModel();
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
