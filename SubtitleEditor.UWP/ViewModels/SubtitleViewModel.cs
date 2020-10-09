﻿using System;
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
        public OperationRecorder OperationRecorder { get; private set; } = new OperationRecorder(string.Empty);
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

        public void AddDialogue(string line)
        {
            TimeSpan beginTime;

            if (this.Count > 0)
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
                    RegisterEventsForDialogueViewModel(dialogueViewModel);
                    Items.Add(dialogueViewModel);
                }

                //AddNewBlankDialogueViewModel();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                OperationRecorder = new OperationRecorder();
                RegisterEventsForOperationRecorder(OperationRecorder);
            }
            else
            {
                throw new ArgumentNullException(nameof(subtitle));
            }
        }


        /// <summary>
        /// 为新建的对话视图模型注册所需要的事件。
        /// </summary>
        /// <param name="dialogueViewModel"></param>
        private void RegisterEventsForDialogueViewModel(DialogueViewModel dialogueViewModel)
        {
            dialogueViewModel.PropertyChanged += DialogueViewModel_PropertyChanged;
            dialogueViewModel.DialogueEdited += DialogueViewModel_SubtitleEdited;
        }

        /// <summary>
        /// 当用户添加字幕时，字幕是添加到Subtitle中的，然后通过出发事件来添加到视图模型中。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Subtitle_DialogueAdded(object sender, Dialogue e)
        {
            DialogueViewModel dialogueViewModel = new DialogueViewModel(e);
            RegisterEventsForDialogueViewModel(dialogueViewModel);

            //添加到历史
            Operation operation = new Operation("Dialogue", OperationType.Add, null, e);
            OperationRecorder.Record(operation);

            this.Add(dialogueViewModel);
        }


        #region 与历史记录操作有关
        /// <summary>
        /// 为操作记录器注册事件。
        /// </summary>
        /// <param name="recorder"></param>
        private void RegisterEventsForOperationRecorder(OperationRecorder recorder)
        {
            recorder.Undoing += Recorder_Undoing;
            recorder.Redoing += Recorder_Redoing;
        }

        /// <summary>
        /// 字幕经过编辑时会触发该函数。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <param name="descrption"></param>
        private void DialogueViewModel_SubtitleEdited(string propertyName, object oldItem, object newItem, string descrption)
        {
            Operation operation = new Operation(propertyName, OperationType.Modify, oldItem, newItem);
            OperationRecorder.Record(operation);
        }

        /// <summary>
        /// 定义撤销操作。
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="recordTime"></param>
        private void Recorder_Redoing(OperationStack operations, DateTime recordTime)
        {
            while(operations.Count != 0)
            {
                var operation = operations.Pop();
                switch(operation.Position)
                {
                    case "Dialogue":
                        {
                            switch (operation.Type)
                            {
                                case OperationType.Add:
                                    {
                                        var dialogue = operation.NewValue as Dialogue;
                                        _subtitle.DeleteDialogue(dialogue);
                                    }
                                    break;
                                case OperationType.Delete:
                                    {
                                        var dialogue = operation.OldValue as Dialogue;
                                        _subtitle.AddDialogue(dialogue);
                                    }
                                    break;
                                case OperationType.Modify:
                                    {
                                        var newDialogue = operation.NewValue as Dialogue;
                                        var oldDialogue = operation.OldValue as Dialogue;
                                        _subtitle.DeleteDialogue(newDialogue);
                                        _subtitle.AddDialogue(oldDialogue);
                                    }
                                    break;
                            }
                                
                        }
                        break;
                    case "Subtitle":
                        break;
                }
            }
        }

        /// <summary>
        /// 定义重做操作
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="recordTime"></param>
        private void Recorder_Undoing(OperationStack operations, DateTime recordTime)
        {
            while (operations.Count != 0)
            {
                var operation = operations.Pop();
                switch (operation.Position)
                {
                    case "Dialogue":
                        {
                            switch (operation.Type)
                            {
                                case OperationType.Add:
                                    this.Remove(operation.NewValue as Dialogue);
                                    break;
                            }
                        }
                        break;
                    case "Subtitle":
                        break;
                }
            }
        }
        #endregion

    }
}
