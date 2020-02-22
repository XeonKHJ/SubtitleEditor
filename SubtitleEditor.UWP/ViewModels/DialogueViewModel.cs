using SubtitleEditor.Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.ViewModels
{
    public class DialogueViewModel
    {
        public DialogueViewModel(Dialogue dialogue)
        {
            No = dialogue.No;
            From = dialogue.From;
            To = dialogue.To;
            Line = dialogue.Line;
        }

        public int No { set; get; }
        public DateTime From { set; get; }
        public DateTime To { set; get; }
        public string Line { set; get; }

        public TimeSpan Span
        {
            get
            {
                return To - From;
            }
        }
    }
}
