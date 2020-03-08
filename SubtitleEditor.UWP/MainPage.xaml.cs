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
using Windows.Storage.Pickers;
using SubtitleEditor.UWP.Controls;
using System.Text;

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
            //mediaPlayer.VideoFrameAvailable += MediaPlayer_VideoFrameAvailableAsync;
        }

        public Subtitle Subtitle { set; get; }
        public DialoguesViewModelCollection DialoguesViewModel { get; } = new DialoguesViewModelCollection();
        private async void OpenButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
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

                try
                {
                    await mediaPlayer.LoadFileAsync(file).ConfigureAwait(true);


                    var path = file.Path;

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        FramedTransportControls.FrameMediaPlayer = mediaPlayer;
                        EnableVideoControls();
                    });
                }
                catch (Exception exception)
                {
                    switch ((uint)exception.HResult)
                    {
                        case 0xC00D36C4:
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
        }

        private async void MediaPlayer_MediaOpenedAsync(FrameMediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    VideoElement.SetMediaPlayer(mediaPlayer.MediaPlayer);
                });

            if (inputBitmap != null)
            {
                var oldBitmap = inputBitmap;
                inputBitmap = null;
                oldBitmap.Dispose();
            }
            int width = (int)sender.PlaybackSession.NaturalVideoWidth;
            int height = (int)sender.PlaybackSession.NaturalVideoHeight;

            Debug.WriteLine("MediaPlayer_MediaOpenedAsync - " + sender.StartOffset);
            //FrameServerSetup(width, height);
        }

        private async void FrameServerSetup(int width, int height)
        {
            isReadtyToDraw = false;
            frameServerDest = new SoftwareBitmap(BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Ignore);

            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    canvasImageSource = new CanvasImageSource(canvasDevice, width, height, DisplayInformation.GetForCurrentView().LogicalDpi);
                    VideoFrameServer.Source = canvasImageSource;
                    inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameServerDest);
                });
            }
            catch (Exception exception)
            {
                switch ((uint)exception.HResult)
                {
                    case 0x887a0005:
                        CloseVideo();
                        break;
                    default:
                        throw new Exception();
                }
            }

            isReadtyToDraw = true;
        }

        private CanvasImageSource canvasImageSource;
        private readonly CanvasDevice canvasDevice = CanvasDevice.GetSharedDevice();
        private SoftwareBitmap frameServerDest;
        private CanvasBitmap inputBitmap;
        private bool isReadtyToDraw = false;
        private async void MediaPlayer_VideoFrameAvailableAsync(FrameMediaPlayer sender, object args)
        {
            if (isReadtyToDraw)
            {
                mediaPlayer.CopyFrameToVideoSurface(inputBitmap);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        using (CanvasDrawingSession ds = canvasImageSource.CreateDrawingSession(Windows.UI.Colors.Black))
                        {
                            try
                            {
                                ds.DrawImage(inputBitmap);
                            }
                            catch (ArgumentException exception)
                            {
                                Debug.WriteLine(exception);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        switch ((uint)exception.HResult)
                        {
                            case 0x802B0020:
                                FrameServerSetup((int)sender.PlaybackSession.NaturalVideoWidth, (int)sender.PlaybackSession.NaturalVideoHeight);
                                break;
                            default:
                                throw new Exception();
                        }
                        Debug.WriteLine(exception.Message);
                    }
                });
            }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e != null)
            {
                if (e.Parameter is StorageFile file)
                {
                    OpenSubFile(file);
                }
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
            VideoElement.Visibility = Visibility.Visible;
            //VideoFrameServer.Visibility = Visibility.Visible;
            VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Visible;
            FramedTransportControls.Visibility = Visibility.Visible;
        }

        private void DisableVideoControls()
        {
            VideoColumn.Width = GridLength.Auto;
            VideoElement.Visibility = Visibility.Collapsed;
            //VideoFrameServer.Visibility = Visibility.Collapsed;
            VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Collapsed;
            FramedTransportControls.Visibility = Visibility.Collapsed;
        }

        private StorageFile openedFile;
        private async void OpenSubFile(StorageFile file)
        {
            if (file != null)
            {
                using (var stream = await file.OpenReadAsync())
                using (var streamReader = new StreamReader(stream.AsStreamForRead(), true))
                {
                    openedFile = file;

                    var content = await streamReader.ReadToEndAsync().ConfigureAwait(true);
                    SubRipParser subRipParser = new SubRipParser();
                    Subtitle = subRipParser.LoadFromString(content);
                    DialoguesViewModel.LoadSubtitle(Subtitle);

                    //修改选中编码
                    EncodingsBox.SelectedItem = streamReader.CurrentEncoding.EncodingName;
                }
            }
        }

        private List<string> Encodings
        {
            get
            {
                var encodingInfos = Encoding.GetEncodings().ToList();
                return (from encoding in encodingInfos
                        select encoding.DisplayName).ToList();
            }
        }
        private void DialoguesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                var firstItem = (DialogueViewModel)e.AddedItems.First();
                DialogueBox.Text = firstItem.Line;
                mediaPlayer.Position = firstItem.From;
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
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Subtitle != null)
            {
                var picker = new FileSavePicker()
                {
                    SuggestedStartLocation = PickerLocationId.VideosLibrary,
                    SuggestedFileName = "新建字幕文件"
                };
                if (openedFile != null)
                {
                    picker.SuggestedFileName = openedFile.DisplayName;
                }

                picker.FileTypeChoices.Add("SubRip", new List<string>() { ".srt" });
                picker.FileTypeChoices.Add("Advanced SubStation Alpha", new List<string>() { ".ass", ".ssa" });
                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    SubRipParser subRipParser = new SubRipParser();
                    var subtitleString = subRipParser.SaveToString(Subtitle);
                    await FileIO.WriteTextAsync(file, subtitleString, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                }
            }
        }

        private void TimeButton_Checked(object sender, RoutedEventArgs e)
        {
            if (FramedTransportControls != null)
            {
                FramedTransportControls.ValueType = FramedMediaTransportControls.TransportValueType.Time;
            }
        }

        private void FrameButton_Checked(object sender, RoutedEventArgs e)
        {
            if (FramedTransportControls != null)
            {
                FramedTransportControls.ValueType = FramedMediaTransportControls.TransportValueType.Frame;
            }

        }
    }
}
