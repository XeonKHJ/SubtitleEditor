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
            DeadLoop();
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
        
        private void OpenVideoFile(StorageFile file)
        {
            if(file != null)
            {
                //关掉前一个视频
                CloseVideo();

                var path = file.Path;
                mediaPlayer = new MediaPlayer
                {
                    Source = MediaSource.CreateFromStorageFile(file)
                };
                VideoElement.SetMediaPlayer(mediaPlayer);
                VideoElement.Visibility = Visibility.Visible;
                VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Visible;
                VideoTransportControls.Visibility = Visibility.Visible;
                //VideoElement.TransportControls = VideoTransportControls;
            }
        }

        private void CloseVideo()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Dispose();
                VideoElement.Visibility = Visibility.Collapsed;
                VideoElementAndDialogueBoxSplitter.Visibility = Visibility.Collapsed;
            }
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

        private async void DeadLoop()
        {
            await Task.Run(()=>
            { 
                while (true) ; 
            }) ;
        }
    }
}
