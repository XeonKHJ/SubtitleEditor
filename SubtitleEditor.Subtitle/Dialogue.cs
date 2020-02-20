using System;
using System.Collections.Generic;
using System.Text;

namespace SubtitleEditor.Subtitle
{
    public class Dialogue
    {
        public string Narrator { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Line { set; get; } = "";

        public string OriginalText { set; get; }
    }
}
