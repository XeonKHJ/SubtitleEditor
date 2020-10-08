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
using System.ComponentModel;
using Windows.System;
using System.Collections.Specialized;
using SubtitleEditor.UWP.History;

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
            DialoguesViewModel.LoadSubtitle(Subtitle);
        }
        public Subtitle Subtitle { set; get; } = new Subtitle();
        public SubtitleViewModel DialoguesViewModel { get; } = new SubtitleViewModel();

        /// <summary>
        /// 历史记录器。
        /// 每次新建或打开一个新的字幕文件时需要重新创建。
        /// </summary>
        internal OperationRecorder HistoryRecorder { get; set; } = new OperationRecorder();
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

        private StorageFile _openedFile;
        private async void OpenSubFile(StorageFile file)
        {
            if (file != null)
            {
                using (var stream = await file.OpenReadAsync())
                //using (var streamReader = new StreamReader(stream.AsStreamForRead(), true))
                //{
                //    _openedFile = file;

                //    var content = await streamReader.ReadToEndAsync().ConfigureAwait(true);
                //    SubRipParser subRipParser = new SubRipParser();
                //    Subtitle = subRipParser.LoadFromString(content);
                //    DialoguesViewModel.LoadSubtitle(Subtitle);
                //    //修改选中编码
                //    EncodingsBox.SelectedItem = streamReader.CurrentEncoding.EncodingName;
                //}
                {
                    _openedFile = file;
                    SubRipParser subRipParser = new SubRipParser();
                    Subtitle = subRipParser.LoadFromStream(stream.AsStream(), Encoding.UTF8);
                    DialoguesViewModel.LoadSubtitle(Subtitle);
                    //修改选中编码
                    //EncodingsBox.SelectedItem = streamReader.CurrentEncoding.EncodingName;
                }

                HistoryRecorder = new OperationRecorder(file.FolderRelativeId);
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
            DialogueBox_LostFocus(null, null);
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
                if (_openedFile != null)
                {
                    picker.SuggestedFileName = _openedFile.DisplayName;
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

        private void AddLineButton_Click(object sender, RoutedEventArgs e)
        {
            //DialoguesViewModel.AddBlankDialogue();
            DialoguesViewModel.AddDialogue("");
        }

        private DialogueViewModel editingDialogue;

        /// <summary>
        /// 当对话框失去焦点时，说明已经编辑完成。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogueBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(editingDialogue != null)
            {
                editingDialogue.Line = DialogueBox.Text;
                editingDialogue = null;
            }
        }

        /// <summary>
        /// 当选中一条或多条对话时的操作，第一条选中的对话出现在主框上。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogueBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(DialoguesGrid.SelectedItems.Count > 0)
            {
                editingDialogue = DialoguesGrid.SelectedItems[0] as DialogueViewModel;
            }
        }

        private void SaveLocalBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
