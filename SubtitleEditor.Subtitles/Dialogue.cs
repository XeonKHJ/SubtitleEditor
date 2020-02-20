using System;
using System.Collections.Generic;
using System.Text;

namespace SubtitleEditor.Subtitles
{
    public class Dialogue
    {
        public Dialogue(DateTime from, DateTime to, string line)
        {
            From = from;
            To = to;
            Line = line;
        }

        public Dialogue(int no, DateTime from, DateTime to, string line, string originalText)
        {
            No = no;
            From = from;
            To = to;
            Line = line;
            OriginalText = originalText;
        }
        public int No { set; get; }
        public string Narrator { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Line { set; get; } = "";
        public string OriginalText { set; get; }
    }
}
