using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleEditor.Subtitles
{
    public class SubRipParser : ISubtitleParser
    {
        public Subtitle LoadFromFile(string filePath)
        {
            var subString = File.ReadAllText(filePath);
            var subtitle = LoadFromString(subString);

            return subtitle;
        }

        public Subtitle LoadFromString(string subString)
        {
            Regex numberLineRegex = new Regex(@"(?<number>\d+)");
            Regex beginTimeRegex = new Regex(@"(?<beginTime>(\d{2}):(\d{2}):(\d{2}),(\d{3}))");
            Regex endTimeRegex = new Regex(@"(?<endTime>(\d{2}):(\d{2}):(\d{2}),(\d{3}))");
            Regex timeArrowRegex = new Regex("(-->)");
            Regex newLineRegex = new Regex(@"(\n|\r)+");
            Regex dialogueLineRegex = new Regex(@"((?<dialogue>.*?)(?=(((\s*)(\r|\n)+\d+\s*(\r|\n)+((\d{2}):(\d{2}):(\d{2}),(\d{3}))(\s*)(-->)(\s*)((\d{2}):(\d{2}):(\d{2}),(\d{3})))|(\s*(\r|\n)*)\z)))");
            Regex anyEmptyChar = new Regex(@"(\s)*");
            var srtRegex = new Regex(numberLineRegex.ToString()     //出现序号
                                    + anyEmptyChar.ToString()       //可以有任意空格
                                    + newLineRegex.ToString()       //换行
                                    + beginTimeRegex.ToString()     //开始时间
                                    + anyEmptyChar.ToString()       //任意空格
                                    + timeArrowRegex.ToString()     //时间箭头
                                    + anyEmptyChar.ToString()       //任意空格
                                    + endTimeRegex.ToString()       //结束时间
                                    + anyEmptyChar.ToString()       //任意空格
                                    + newLineRegex.ToString()       //换行
                                    + dialogueLineRegex.ToString()  //台词
                                    , RegexOptions.Singleline
                                    );

            var matches = srtRegex.Matches(subString);

            Subtitle subtitle = new Subtitle();
            foreach(Match match in matches)
            {
                var groups = match.Groups;
                var no = int.Parse(groups["number"].Value);
                var beginTime = groups["beginTime"].Value;
                var endTime = groups["endTime"].Value;
                var line = groups["dialogue"].Value;
                Dialogue dialogue = new Dialogue(no, DateTime.ParseExact(beginTime, "HH:mm:ss,fff", null), DateTime.ParseExact(endTime, "HH:mm:ss,fff", null), line, match.Value);
                subtitle.Dialogues.Add(dialogue);
            }

            return subtitle;
        }
        public void SaveToFile(Subtitle subtitle, string filePath)
        {
            throw new NotImplementedException();
        }

        public Task SaveToFileAsync(Subtitle subtitle, string filePath)
        {
            throw new NotImplementedException();
        }

        public string SaveToString(Subtitle subtitle)
        {
            throw new NotImplementedException();
        }
    }
}
