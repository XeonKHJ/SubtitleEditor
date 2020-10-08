using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.History
{
    public enum OperationType { Modify, Add, Delete}
    public enum OperatedPosition { Dialogue, Subtitle}
    public class Operation
    {
        public Operation(OperatedPosition position, OperationType type, object oldValue, object newValue)
        {
            Position = position;
        }
        public OperatedPosition Position { private set; get; }
        public OperationType Type { private set; get; }

        public object OldValue { private set; get; }

        public object NewValue { private set; get; }
    }
}
