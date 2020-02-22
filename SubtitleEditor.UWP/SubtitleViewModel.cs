using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP
{
    class SubtitleViewModel
    {
        public DateTime BeginTime { set; get; }
        public DateTime EndTime { set; get; }
        public string Dialog { set; get; }
        public TimeSpan Span
        {
            get
            {
                return EndTime - BeginTime;
            }
        }
    }
}
