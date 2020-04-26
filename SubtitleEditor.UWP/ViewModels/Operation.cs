using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.ViewModels
{
    public enum OperationType { Edit, Add, Delete}
    public enum OperatedProperty { Line, From, To}
    class Operation
    {
        public DateTime OperateTime { set; get; }
        public OperatedProperty Property { set; get; }
        public OperationType EditType { set; get; }
    }
}
