using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SubtitleEditor.Subtitles;
using SubtitleEditor.UWP.ViewModels;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Imaging;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Graphics.Display;
using Windows.Media.MediaProperties;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SubtitleEditor.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public Subtitle Subtitle;
        public DialoguesViewModel DialoguesViewModel = new DialoguesViewModel();
        private async void OpenButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".srt");

            StorageFile file = await picker.PickSingleFileAsync();

            OpenSubFile(file);
        }

        private MediaPlayer mediaPlayer;
        private double FrameRate;
        private async void OpenVideoFile(StorageFile file)
        {
        
            if(file != null)
            {
                //关掉前一个视频
                CloseVideo();
                
                List<string> encodingPropertiesToRetrieve = new List<string>();

                MediaEncodingProfile mediaEncodingProfile = await MediaEncodingProfile.CreateFromFileAsync(file);
                FrameRate = (double)mediaEncodingProfile.Video.FrameRate.Numerator / (double)mediaEncodingProfile.Video.FrameRate.Denominator;

                var path = file.Path;

                App.RunOnUIThread(()=>
                {
                    EnableVideoControls();
                    mediaPlayer = new MediaPlayer
                    {
                        IsVideoFrameServerEnabled = true
                    };
                    mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                    mediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
                    VideoElement.SetMediaPlayer(mediaPlayer);
                    //VideoStandAloneControls.MediaPlayer = mediaPlayer;
                    //VideoStandAloneControls.FrameRate = FrameRate;
                });
            }
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            int width = (int)sender.PlaybackSession.NaturalVideoWidth;
            int height = (int)sender.PlaybackSession.NaturalVideoHeight;
            frameServerDest = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Ignore);
            Debug.WriteLine(String.Format("Initial Position - TimeSpan: {0}, Frame: {1}", sender.PlaybackSession.Position, sender.PlaybackSession.Position.TotalSeconds * FrameRate));
            
            var actualTime = sender.PlaybackSession.NaturalDuration.Ticks * (30 / 29.97);

            App.RunOnUIThread(() =>
            {
                canvasImageSource = new CanvasImageSource(canvasDevice, width, height, DisplayInformation.GetForCurrentView().LogicalDpi);
                VideoFrameServer.Source = canvasImageSource;
                mediaPlayer.VideoFrameAvailable += MediaPlayer_VideoFrameAvailableAsync;
            });
        }

        private CanvasImageSource canvasImageSource;
        private readonly CanvasDevice canvasDevice = CanvasDevice.GetSharedDevice();
        private SoftwareBitmap frameServerDest;
        private async void MediaPlayer_VideoFrameAvailableAsync(MediaPlayer sender, object args)
        {
            var deltaTimeSpan = sender.PlaybackSession.Position;
            
            if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Opening)
            {
                return;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameServerDest))
                using (CanvasDrawingSession ds = canvasImageSource.CreateDrawingSession(Windows.UI.Colors.Black))
                {
                    mediaPlayer.CopyFrameToVideoSurface(inputBitmap);
                    ds.DrawImage(inputBitmap);
                }
            });

        }

        private void CloseVideo()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Dispose();
                DisableVideoControls();
            }
        }

        private void EnableVideoControls()
        {
            VideoElement.Visibility = Visibility.Visible;
            //VideoFrameServer.Visibility = Visibility.Visible;
            VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Visible;
            VideoTransportControls.Visibility = Visibility.Visible;
        }

        private void DisableVideoControls()
        {
            VideoElement.Visibility = Visibility.Collapsed;
            //VideoFrameServer.Visibility = Visibility.Collapsed;
            VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Collapsed;
            VideoTransportControls.Visibility = Visibility.Collapsed;
        }

        private async void OpenSubFile(StorageFile file)
        {
            if (file != null)
            {
                using (var stream = await file.OpenReadAsync())
                {
                    StreamReader streamReader = new StreamReader(stream.AsStreamForRead());
                    var content = await streamReader.ReadToEndAsync();
                    SubRipParser subRipParser = new SubRipParser();
                    Subtitle = subRipParser.LoadFromString(content);
                    DialoguesViewModel.LoadSubtitle(Subtitle);
                }
            }
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            DialoguesViewModel.Add(new DialogueViewModel(new Dialogue(DateTime.Now, DateTime.Now, "shit")));
        }

        private void DialoguesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count != 0)
            {
                DialogueBox.Text = ((DialogueViewModel)e.AddedItems.First()).Line;
            }
        }

        private async void OpenVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".mp4");

            StorageFile file = await picker.PickSingleFileAsync();

            OpenVideoFile(file);
        }

        private void CloseVideoButton_Click(object sender, RoutedEventArgs e)
        {
            CloseVideo();
        }

        private void GoToButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var frame = Convert.ToInt32(PositionBox.Text);

            }
            catch(Exception)
            {

            }
        }
    }
}
