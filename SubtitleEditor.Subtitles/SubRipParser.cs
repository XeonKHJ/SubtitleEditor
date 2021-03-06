﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using SubtitlesParser.Classes.Parsers;
using System.ComponentModel;

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

        public Subtitle LoadFromStream(Stream stream, Encoding encoding)
        {
            SrtParser parser = new SrtParser();
            Subtitle subtitle = new Subtitle();
            var sub = parser.ParseStream(stream, encoding);

            int no = 0;
            foreach(var dia in sub)
            {
                
                string line = string.Empty;

                for(int i = 0; i < dia.Lines.Count; ++i)
                {
                    if(i == dia.Lines.Count - 1)
                    {
                        line += dia.Lines[i];
                    }
                    else
                    {
                        line += (dia.Lines[i] + System.Environment.NewLine);
                    }
                }

                Dialogue dialogue = new Dialogue(++no, new TimeSpan(0, 0, 0, 0, dia.StartTime), new TimeSpan(0, 0, 0, 0, dia.EndTime), line);

                subtitle.AddDialogue(dialogue);
            }
            return subtitle;
        }

        public Subtitle LoadFromString(string subString)
        {

            Regex numberLineRegex = new Regex(@"(?<number>\d+)");
            Regex beginTimeRegex = new Regex(@"(?<beginTime>(\d{2}):(\d{2}):(\d{2}),(\d{3}))");
            Regex endTimeRegex = new Regex(@"(?<endTime>(\d{2}):(\d{2}):(\d{2}),(\d{3}))");
            Regex timeArrowRegex = new Regex("(-->)");
            Regex newLineRegex = new Regex(@"(\n|\r)");
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
                Dialogue dialogue = new Dialogue(no, TimeSpan.ParseExact(beginTime, @"hh\:mm\:ss\,fff", null), TimeSpan.ParseExact(endTime, @"hh\:mm\:ss\,fff", null), line, match.Value);
                subtitle.AddDialogue(dialogue);
            }

            return subtitle;
        }
        public async void SaveToFile(Subtitle subtitle, string filePath, Encoding encoding = null)
        {
            await SaveToFileAsync(subtitle, filePath, encoding).ConfigureAwait(false);
        }

        public async Task SaveToFileAsync(Subtitle subtitle, string filePath, Encoding encoding = null)
        {
            using (var stream = File.OpenWrite(filePath))
            {
                var subtitleString = SaveToString(subtitle);
                byte[] encodedBytes;
                if(encoding != null)
                {
                    encodedBytes = encoding.GetBytes(subtitleString);
                }
                else
                {
                    encodedBytes = Encoding.UTF8.GetBytes(subtitleString);
                }

                await stream.WriteAsync(encodedBytes, 0, encodedBytes.Length).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public string SaveToString(Subtitle subtitle)
        {
            string subtitleString = "";

            if(subtitle != null)
            {
                for (int i = 0; i < subtitle.Dialogues.Count; ++i)
                {
                    var dialogue = subtitle.Dialogues[i];
                    string from = dialogue.From.ToString(@"hh\:mm\:ss\,fff", CultureInfo.CurrentCulture);
                    string to = dialogue.To.ToString(@"hh\:mm\:ss\,fff", CultureInfo.CurrentCulture);
                    string arrowWithTwoSpace = " --> ";
                    string no = (i + 1).ToString(CultureInfo.CurrentCulture);
                    string line = dialogue.Line;

                    string dialogueString = no + Environment.NewLine +
                                            to + arrowWithTwoSpace + from + Environment.NewLine +
                                            line + Environment.NewLine +
                                            Environment.NewLine;
                    subtitleString += dialogueString;
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(subtitle));
            }


            return subtitleString;
        }
    }
}
