using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SubtitleEditor.UWP
{
    public class DialogueViewModelFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Retrieve the format string and use it to format the value.

            string formatString = parameter as string;
            string valueString = value.ToString();
            if (value is DateTime dateTime)
            {
                valueString = dateTime.ToString("HH:mm:ss,fff");
            }
            else if(value is TimeSpan span)
            {
                valueString = span.ToString(@"m\:ss\,fff");
            }
            else if(value is string dialogue)
            {
                if(parameter is string option)
                {
                    switch (option)
                    {
                        case "SingleLine":
                            valueString = valueString.Replace("\n", "\\n", StringComparison.Ordinal)
                                                     .Replace("\r\\n", "\\n", StringComparison.Ordinal)
                                                     .Replace("\r", "\\n", StringComparison.Ordinal);
                            break;
                    }
                }
            }

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Object target = value;
            if(parameter is string paramString)
            {
                switch (parameter)
                {
                    case "SingleLine":
                        if(value is string singleLineDialogue)
                        {
                            target = singleLineDialogue.Replace("\\n", Environment.NewLine, StringComparison.Ordinal);
                        }
                        break;
                }
            }
            return target;
        }
    }
}
