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
using SubtitleEditor.UWP.Controls;

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
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpenedAsync;
            mediaPlayer.VideoFrameAvailable += MediaPlayer_VideoFrameAvailableAsync;
        }

        private StorageFile _file;
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

        private readonly FrameMediaPlayer mediaPlayer = new FrameMediaPlayer();
        private async void OpenVideoFile(StorageFile file)
        {
            
            if (file != null)
            {
                //关掉前一个视频
                CloseVideo();

                _file = file;

                await mediaPlayer.LoadFileAsync(file);
                //using (var stream = await file.OpenStreamForReadAsync())
                //{
                //    await mediaPlayer.LoadStreamAsync(stream.AsRandomAccessStream(), file.ContentType);
                //}

                var path = file.Path;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    FramedTransportControls.FrameMediaPlayer = mediaPlayer;
                    EnableVideoControls();
                });
            }
        }

        private async void MediaPlayer_MediaOpenedAsync(FrameMediaPlayer sender, object args)
        {
            if(inputBitmap != null)
            {
                var oldBitmap = inputBitmap;
                inputBitmap = null;
                oldBitmap.Dispose();
            }
            int width = (int)sender.PlaybackSession.NaturalVideoWidth;
            int height = (int)sender.PlaybackSession.NaturalVideoHeight;

            frameServerDest = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Ignore);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                canvasImageSource = new CanvasImageSource(canvasDevice, width, height, DisplayInformation.GetForCurrentView().LogicalDpi);
                VideoFrameServer.Source = canvasImageSource;
                inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameServerDest);
            });
        }

        private CanvasImageSource canvasImageSource;
        private readonly CanvasDevice canvasDevice = CanvasDevice.GetSharedDevice();
        private SoftwareBitmap frameServerDest;
        private CanvasBitmap inputBitmap;
        private async void MediaPlayer_VideoFrameAvailableAsync(FrameMediaPlayer sender, object args)
        {
            if(inputBitmap != null)
            {
                Debug.WriteLine(canvasImageSource.AlphaMode);
                Debug.WriteLine(Windows.UI.Colors.Black);
                mediaPlayer.CopyFrameToVideoSurface(inputBitmap);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    using (CanvasDrawingSession ds = canvasImageSource.CreateDrawingSession(Windows.UI.Colors.Black))
                    {
                        try
                        {
                            ds.DrawImage(inputBitmap);
                        }
                        catch (ArgumentException exception)
                        {
                            System.Diagnostics.Debug.WriteLine(exception);
                        }
                    }
                });
            }

        }

        private void CloseVideo()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Close();
                DisableVideoControls();
            }
        }

        private void EnableVideoControls()
        {
            //VideoElement.Visibility = Visibility.Visible;
            VideoFrameServer.Visibility = Visibility.Visible;
            VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Visible;
            FramedTransportControls.Visibility = Visibility.Visible;
        }

        private void DisableVideoControls()
        {
            VideoElement.Visibility = Visibility.Collapsed;
            VideoFrameServer.Visibility = Visibility.Collapsed;
            VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Collapsed;
            FramedTransportControls.Visibility = Visibility.Collapsed;
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
            if (e.AddedItems.Count != 0)
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
                mediaPlayer.Dispose();
            }
            catch (Exception)
            {

            }
        }
    }
}
