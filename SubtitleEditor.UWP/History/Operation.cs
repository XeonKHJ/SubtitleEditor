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
        public Operation(OperatedPosition position, OperationType type, Object oldValue, Object newValue)
        {
            Position = position;
        }
        public OperatedPosition Position { private set; get; }
        public OperationType Type { private set; get; }
    }
}
