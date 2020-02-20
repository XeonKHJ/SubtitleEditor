using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleEditor.Subtitle
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
            Regex beginOrNewLineRegex = new Regex(@"^|((\s*)(\r|\n)+)");
            Regex numberLineRegex = new Regex(@"(?<number>\d+)");
            Regex beginTimeRegex = new Regex(@"(?<beginTime>(\d{2}):(\d{2}):(\d{2}),(\d{3}))");
            Regex endTimeRegex = new Regex(@"(?<endTime>(\d{2}):(\d{2}):(\d{2}),(\d{3}))");
            Regex timeArrowRegex = new Regex("(-->)");
            Regex newLineRegex = new Regex(@"(\n|\r)+");
            Regex dialogueLineRegex = new Regex(@"((?<dialogue>.*?)(?=(((\r|\n)+\d+\s*(\r|\n)+((\d{2}):(\d{2}):(\d{2}),(\d{3}))(\s*)(-->)(\s*)((\d{2}):(\d{2}):(\d{2}),(\d{3})))|\z)))");
            Regex anyEmptyChar = new Regex(@"(\s)*");
            Regex contRegex = new Regex(@"(?<dialogue>(.*))", RegexOptions.Singleline);
            var fullRegex = new Regex(numberLineRegex.ToString() //出现序号
                                    + anyEmptyChar.ToString() //可以有任意空格
                                    + newLineRegex.ToString() //换行
                                    + beginTimeRegex.ToString() //开始时间
                                    + anyEmptyChar.ToString() //任意空格
                                    + timeArrowRegex.ToString() //时间箭头
                                    + anyEmptyChar.ToString() //任意空格
                                    + endTimeRegex.ToString() //结束时间
                                    + anyEmptyChar.ToString() //任意空格
                                    + newLineRegex.ToString() //换行
                                    + dialogueLineRegex.ToString()
                                    , RegexOptions.Singleline
                                    );

            return new Subtitle();
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
