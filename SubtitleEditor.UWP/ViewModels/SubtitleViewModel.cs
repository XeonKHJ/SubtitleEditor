using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SubtitleEditor.Subtitles;
using System.Collections.Specialized;
using Newtonsoft.Json;
using SubtitleEditor.UWP.History;

namespace SubtitleEditor.UWP.ViewModels
{
    public class SubtitleViewModel : ObservableCollection<DialogueViewModel>
    {
        internal OperationRecorder HistoryRecorder { get; } = new OperationRecorder("NewFile");
        private Subtitle _subtitle;

        public SubtitleViewModel()
        {
            //CollectionChanged += DialoguesViewModel_CollectionChanged;
        }

        /// <summary>
        /// 因为我们在GridView里显示，因此当单条字幕经过修改时，需要通知界面（不能通过默认通知因为绑定的数据不是里面的对话，而是对话合集的字幕）。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialoguesViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (DialogueViewModel item in e.NewItems)
                    {
                        item.PropertyChanged += DialogueViewModel_PropertyChanged;
                    }
                    //如果是添加字幕
                    //DialoguesAddedOrDeleted(sender, e);
                    break;
            }
        }
        public SubtitleViewModel(Subtitle subtitle)
        {
            LoadSubtitle(subtitle);
        }

        /// <summary>
        /// 将字幕加载进该视图模型
        /// </summary>
        /// <param name="subtitle">字幕实例</param>
        public void LoadSubtitle(Subtitle subtitle)
        {
            //清楚视图模型中的字幕集合。
            Items.Clear();

            if (subtitle != null)
            {
                _subtitle = subtitle;

                //为字幕注册添加对话事件。
                _subtitle.DialogueAdded += Subtitle_DialogueAdded;

                //为视图模型重新添加新字幕中的对话。
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

        private void Subtitle_DialogueAdded(object sender, Dialogue e)
        {
            DialogueViewModel dialogueViewModel = new DialogueViewModel(e);
            dialogueViewModel.PropertyChanged += DialogueViewModel_PropertyChanged;
            dialogueViewModel.DialogueEdited += DialogueViewModel_SubtitleEdited;
            this.Add(dialogueViewModel);
        }

        private void DialogueViewModel_SubtitleEdited(string propertyName, object oldItem, object newItem, string descrption)
        {

        }

        public void AddDialogue(string line)
        {
            TimeSpan beginTime;
            
            if(this.Count > 0)
            {
                beginTime = this.Last().To;
            }

            Dialogue dialogue = new Dialogue(beginTime, beginTime, line);

            _subtitle.AddDialogue(dialogue);
        }

        private async void DialogueViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                
                System.Diagnostics.Debug.WriteLine("编辑了字幕。");

                //SubtitleEdited?.Invoke(sender, e);
            }).ConfigureAwait(false);
        }
    }
}
