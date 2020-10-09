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
using System.Runtime.CompilerServices;

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
                        item.DialogueModified += DialogueViewModel_DialogueModified;
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

            //创建对话逻辑模型。
            Dialogue dialogue = new Dialogue(this.Count + 1, beginTime, beginTime, line);

            //通过对话逻辑模型创建视图模型。
            DialogueViewModel dialogueViewModel = new DialogueViewModel(dialogue);
            RegisterEventsForDialogueViewModel(dialogueViewModel);

            //添加到字幕视图模型
            this.Add(dialogueViewModel);

            //添加到字幕逻辑模型
            _subtitle.AddDialogue(dialogue);

            //添加到历史
            Operation operation = new Operation("Items", this, OperationType.Add, null, dialogueViewModel);
            OperationRecorder.Record(operation);
        }
        
        public void AddDialogue(DialogueViewModel dialogueViewModel)
        {
            if(dialogueViewModel != null)
            {
                Dialogue dialogue = dialogueViewModel.ToDialogue();
                _subtitle.AddDialogue(dialogue);

                this.Add(dialogueViewModel);

                //添加到历史
                Operation operation = new Operation("Items", this, OperationType.Add, null, dialogueViewModel);
                OperationRecorder.Record(operation);
            }
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
            dialogueViewModel.DialogueModified += DialogueViewModel_DialogueModified;
        }

        private void DialogueViewModel_DialogueModified(DialogueViewModel sender, string propertyName, object oldValue, object newValue)
        {
            Operation operation = new Operation(propertyName, sender,OperationType.Modify, oldValue, newValue);
            OperationRecorder.Record(operation);
        }

        /// <summary>
        /// 当用户添加字幕时，字幕是添加到Subtitle中的，然后通过出发事件来添加到视图模型中。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Subtitle_DialogueAdded(object sender, Dialogue e)
        {

        }


        private void RemoveDialogue(DialogueViewModel viewModel)
        {
            this.RemoveAt(viewModel.No - 1);
            _subtitle.DeleteDialogueByIndex(viewModel.No - 1);

            //所有后面的对话的
            for(int i = viewModel.No -1; i < Count; ++i)
            {
                Items[i].No = i;
            }

            Operation operation = new Operation("Items", this, OperationType.Delete, viewModel, null);
            OperationRecorder.Record(operation);
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
        /// 定义撤销操作。
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="recordTime"></param>
        private static void Recorder_Redoing(OperationStack operations, DateTime recordTime)
        {
            if(operations != null)
            {
                var clonedOps = new Stack<Operation>(operations);

                while (clonedOps.Count != 0)
                {
                    var operation = clonedOps.Pop();

                    if (operation.ChangeeObject is DialogueViewModel)
                    {
                        var dialogueViewModel = operation.ChangeeObject as DialogueViewModel;
                        switch (operation.Type)
                        {
                            case OperationType.Modify:
                                {
                                    switch (operation.PropertyName)
                                    {
                                        case "Line":
                                            dialogueViewModel.Line = (string)operation.NewValue;
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    else if (operation.ChangeeObject is SubtitleViewModel)
                    {
                        var subtitleViewModel = operation.ChangeeObject as SubtitleViewModel;
                        switch (operation.Type)
                        {
                            case OperationType.Add:
                                {
                                    switch (operation.PropertyName)
                                    {
                                        case "Items":
                                            subtitleViewModel.AddDialogue(operation.NewValue as DialogueViewModel);
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// 定义重做操作
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="recordTime"></param>
        private static void Recorder_Undoing(OperationStack operations, DateTime recordTime)
        {
            
            if(operations != null)
            {
                var clonedOps = new Stack<Operation>(operations);
                while (clonedOps.Count != 0)
            {
                var operation = clonedOps.Pop();

                if (operation.ChangeeObject is DialogueViewModel)
                {
                    var dialogueViewModel = operation.ChangeeObject as DialogueViewModel;
                    switch (operation.Type)
                    {
                        case OperationType.Modify:
                            {
                                switch (operation.PropertyName)
                                {
                                    case "Line":
                                        dialogueViewModel.Line = (string)operation.OldValue;
                                        break;
                                }
                            }
                            break;
                    }
                }
                else if(operation.ChangeeObject is SubtitleViewModel)
                {
                    var subtitleViewModel = operation.ChangeeObject as SubtitleViewModel;
                    switch(operation.Type)
                    {
                        case OperationType.Add:
                            {
                                switch(operation.PropertyName)
                                {
                                    case "Items":
                                        subtitleViewModel.RemoveDialogue(operation.NewValue as DialogueViewModel);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            }
            
        }
        #endregion

    }
}
