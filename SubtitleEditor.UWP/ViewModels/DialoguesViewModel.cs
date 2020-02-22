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
    public class DialoguesViewModel : ObservableCollection<DialogueViewModel>
    {
        public DialoguesViewModel()
        {
            this.CollectionChanged += DialoguesViewModel_CollectionChanged;
        }

        private void DialoguesViewModel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fuck me!");
        }

        public DialoguesViewModel(Subtitle subtitle)
        {
            LoadSubtitle(subtitle);
        }

        public void LoadSubtitle(Subtitle subtitle)
        {
            Items.Clear();
            foreach (var d in subtitle.Dialogues)
            {
                Items.Add(new DialogueViewModel(d));
            }
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
    }
}
