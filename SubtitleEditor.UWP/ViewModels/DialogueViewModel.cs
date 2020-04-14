using SubtitleEditor.Subtitles;
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
        public DialogueViewModel(Dialogue dialogue)
        {
            if(dialogue != null)
            {
                No = dialogue.No;
                From = dialogue.From;
                To = dialogue.To;
                Line = dialogue.Line;
                isLoaded = true;
            }
            else
            {
                throw new ArgumentNullException(nameof(dialogue));
            }
        }

        public DialogueViewModel() 
        {
            isLoaded = true;
        }

        private bool isLoaded = false;
        public int No { set; get; }
        public TimeSpan From { set; get; }
        public TimeSpan To { set; get; }

        private string _line;
        public string Line
        {
            set
            {
                _line = value;
                OnPropertyChanged();
            }
            get
            {
                return _line;
            }
        }

        public Dialogue ToDialogue()
        {
            return new Dialogue(From, To, Line) { No = No };
        }

        public TimeSpan Span
        {
            get
            {
                return To - From;
            }
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(isLoaded)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
