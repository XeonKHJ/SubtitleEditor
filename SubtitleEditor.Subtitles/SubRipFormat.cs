using System;
using System.Collections.Generic;
using System.Text;

namespace SubtitleEditor.Subtitles
{
    public class SubRipFormat : IFormatProvider, ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        public object GetFormat(Type formatType)
        {
            if(formatType == typeof(ICustomFormatter))
            {
                return this;
            }
            else
            {
                return null;
            }
        }
    }
}
