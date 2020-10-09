using SubtitleEditor.Subtitles;
using SubtitleEditor.UWP.History;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.ViewModels
{
    public class DialogueViewModel : INotifyPropertyChanged
    {
        private Dialogue _dialogue;
        public DialogueViewModel(Dialogue dialogue)
        {
            if(dialogue != null)
            {
                No = dialogue.No;
                From = dialogue.From;
                To = dialogue.To;
                Line = dialogue.Line;
                _isLoaded = true;
                _dialogue = dialogue;
            }
            else
            {
                throw new ArgumentNullException(nameof(dialogue));
            }
        }

        public DialogueViewModel() 
        {
            _isLoaded = true;
        }

        private bool _isLoaded = false;
        public int No { set; get; }
        public TimeSpan From { set; get; }
        public TimeSpan To { set; get; }

        private string _line;
        public string Line
        {
            set
            {
                var oldValue = _line;
                _line = value;
                OnPropertyChanged(oldValue, value);
            }
            get
            {
                return _line;
            }
        }

        public Dialogue ToDialogue()
        {
            return new Dialogue(No, From, To, Line);
        }

        public TimeSpan Span
        {
            get
            {
                return To - From;
            }
        }
        public void OnPropertyChanged(object oldValue, object newValue, [CallerMemberName] string propertyName = null)
        {
            if(_isLoaded)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

                switch(propertyName)
                {
                    case "Line":
                        _dialogue.Line = this.Line;
                        break;
                }

                DialogueModified?.Invoke(this, propertyName, oldValue, newValue);
            }
        }

        /// <summary>
        /// 用于通知界面字幕更新的事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void DialogueModifiedEventHandler(DialogueViewModel sender, string propertyName, object oldValue, object newValue);
        public event DialogueModifiedEventHandler DialogueModified;
    }
}
