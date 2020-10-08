using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.History
{
    public class OperationStack : Stack<Operation>
    {
        public string Name { set; get; } = string.Empty;

        /// <summary>
        /// 创建一个空操作列表
        /// </summary>
        /// <param name="listName">列表名字</param>
        public OperationStack(string listName)
        {
            Name = listName;
        }

        /// <summary>
        /// 创建仅有一个操作的操作列表
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="listName">列表名字</param>
        public OperationStack(Operation operation, string listName)
        {
            this.Push(operation);
        }
    }
}
