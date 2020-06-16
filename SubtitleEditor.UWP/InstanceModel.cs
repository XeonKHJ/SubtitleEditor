using SubtitleEditor.Subtitles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SubtitleEditor.UWP
{
    internal class InstanceModel
    {
        public StorageFile StorageFile { set; get; }

        public Subtitle Subtitle { set; get; }
    }
}
