using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.Subtitles
{
    interface ISubtitleParser
    {
        Subtitle LoadFromFile(string filePath);

        Subtitle LoadFromString(string subString);
        string SaveToString(Subtitle subtitle);

        void SaveToFile(Subtitle subtitle, string filePath, Encoding encoding);

        Task SaveToFileAsync(Subtitle subtitle, string filePath, Encoding encoding);

        Subtitle LoadFromStream(Stream stream, Encoding encoding);
    }
}
