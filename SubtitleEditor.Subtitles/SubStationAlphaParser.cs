using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.Subtitles
{
    public class SubStationAlphaParser : ISubtitleParser
    {
        public Subtitle LoadFromFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public Subtitle LoadFromStream(Stream stream, Encoding encoding)
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
