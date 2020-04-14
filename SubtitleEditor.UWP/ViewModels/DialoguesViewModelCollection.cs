using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SubtitleEditor.Subtitles;
using System.Collections.Specialized;

namespace SubtitleEditor.UWP.ViewModels
{
    public class DialoguesViewModelCollection : ObservableCollection<DialogueViewModel>
    {
        public DialoguesViewModelCollection()
        {
            this.CollectionChanged += DialoguesViewModel_CollectionChanged;
        }

        private void DialoguesViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (DialogueViewModel item in e.NewItems)
                    {
                        item.PropertyChanged += DialogueViewModel_PropertyChanged;
                    }
                    //如果是添加字幕
                    DialoguesAddedOrDeleted(sender, e);
                    break;
            }

        }

        Subtitle _subtitle;
        public DialoguesViewModelCollection(Subtitle subtitle)
        {
            LoadSubtitle(subtitle);
        }

        public void LoadSubtitle(Subtitle subtitle)
        {
            Items.Clear();
            _subtitle = subtitle;
            if(subtitle != null)
            {
                foreach (var d in subtitle.Dialogues)
                {
                    var dialogueViewModel = new DialogueViewModel(d);
                    dialogueViewModel.PropertyChanged += DialogueViewModel_PropertyChanged;
                    Items.Add(dialogueViewModel);
                }

                //AddNewBlankDialogueViewModel();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                throw new ArgumentNullException(nameof(subtitle));
            }
        }
        private void AddNewBlankDialogueViewModel()
        {
            var blankViewModel = new DialogueViewModel { No = 0, Line = "" };
            blankViewModel.PropertyChanged += DialogueViewModel_PropertyChanged;
            Items.Add(blankViewModel);
        }

        public void AddBlankDialogue()
        {
            AddNewBlankDialogueViewModel();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event PropertyChangedEventHandler SubtitleEdited;
        public event NotifyCollectionChangedEventHandler DialoguesAddedOrDeleted;
        private async void DialogueViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                SubtitleEdited.Invoke(sender, e);
            }).ConfigureAwait(false);
        }
    }
}
