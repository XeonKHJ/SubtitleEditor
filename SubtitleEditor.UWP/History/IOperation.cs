using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.History
{
    interface IOperation<T>
    {
        T Changer { set; get; }
        string PropertyName { set; get; }
        OperationType Type { set; get; }

        object OldValue { set; get; }

        object NewValue { set; get; }
    }
}
