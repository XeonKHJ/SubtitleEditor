using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.Subtitle
{
    interface ISubtitleParser
    {
        Subtitle LoadFromFile(string filePath);

        Subtitle LoadFromString(string subString);
        string SaveToString(Subtitle subtitle);

        void SaveToFile(Subtitle subtitle, string filePath);

        Task SaveToFileAsync(Subtitle subtitle, string filePath);
    }
}
