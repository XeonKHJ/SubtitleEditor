using System;
using System.Collections.Generic;
using System.Text;

namespace SubtitleEditor.Subtitles
{
    public class Dialogue
    {
        public Dialogue(TimeSpan from, TimeSpan to, string line)
        {
            From = from;
            To = to;
            Line = line;
        }

        public Dialogue(int no, TimeSpan from, TimeSpan to, string line, string originalText)
        {
            No = no;
            From = from;
            To = to;
            Line = line;
            OriginalText = originalText;
        }
        public int No { set; get; }
        public string Narrator { get; set; }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public string Line { set; get; } = "";
        public string OriginalText { set; get; }
    }
}
