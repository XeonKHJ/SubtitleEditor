using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.Subtitles
{
    public class AssParser : ISubtitleParser
    {
        public Subtitle LoadFromFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public Subtitle LoadFromString(string subString)
        {
            throw new NotImplementedException();
        }

        public void SaveToFile(Subtitle subtitle, string filePath, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public Task SaveToFileAsync(Subtitle subtitle, string filePath, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public string SaveToString(Subtitle subtitle)
        {
            throw new NotImplementedException();
        }
    }
}
