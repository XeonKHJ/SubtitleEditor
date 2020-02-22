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

        public List<string> Dialogues = new List<string> { "sdfsdf", "sdfsdf" };
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
    }
}
