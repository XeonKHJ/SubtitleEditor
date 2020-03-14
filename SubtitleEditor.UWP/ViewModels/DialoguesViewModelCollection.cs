using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SubtitleEditor.Subtitles;

namespace SubtitleEditor.UWP.ViewModels
{
    public class DialoguesViewModelCollection : ObservableCollection<DialogueViewModel>
    {
        public DialoguesViewModelCollection()
        {
            this.CollectionChanged += DialoguesViewModel_CollectionChanged;
        }

        private void DialoguesViewModel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fuck me!");
        }

        public DialoguesViewModelCollection(Subtitle subtitle)
        {
            LoadSubtitle(subtitle);
        }

        public void LoadSubtitle(Subtitle subtitle)
        {
            Items.Clear();
            if(subtitle != null)
            {
                foreach (var d in subtitle.Dialogues)
                {
                    var dialogueViewModel = new DialogueViewModel(d);
                    dialogueViewModel.PropertyChanged += DialogueViewModel_PropertyChanged;
                    Items.Add(dialogueViewModel);
                }

                var blankViewModel = new DialogueViewModel { No = 0, Line = "" };
                blankViewModel.PropertyChanged += DialogueViewModel_PropertyChanged;
                Items.Add(blankViewModel);

                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
            }
            else
            {
                throw new ArgumentNullException(nameof(subtitle));
            }
        }

        public event PropertyChangedEventHandler SubtitleEdited;
        private async void DialogueViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                SubtitleEdited.Invoke(sender, e);
            }).ConfigureAwait(false);
        }
    }
}
