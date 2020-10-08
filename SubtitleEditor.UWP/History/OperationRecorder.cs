using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SubtitleEditor.UWP.History
{
    /// <summary>
    /// 操作记录器，用于记录对字幕的每一个操作。每个操作记录器对应一个编辑的字幕。
    /// </summary>
    public class OperationRecorder : Stack<Tuple<OperationStack, DateTime>>
    {
        public enum DoingType { Redo, Undo};

        private Stack<Tuple<OperationStack, DateTime>> RedoStack = new Stack<Tuple<OperationStack, DateTime>>();
        /// <summary>
        /// 每个记录器要有一个ID用于于一个文件对应
        /// </summary>
        public string RecorderId { get; private set; } = string.Empty;
        public OperationRecorder(StorageFile storageFile)
        {
            if(storageFile != null)
            {
                RecorderId = storageFile.FolderRelativeId;
            }
        }

        public OperationRecorder()
        { }

        public OperationRecorder(string recorderId)
        {
            RecorderId = recorderId;
        }

        /// <summary>
        /// 将会用作主要的函数来记录操作。
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="operationTime"></param>
        public void Push(OperationStack operations, DateTime operationTime)
        {
            this.Push(new Tuple<OperationStack, DateTime>(operations, operationTime));
        }

        /// <summary>
        /// 添加一个操作
        /// </summary>
        /// <param name="operation">要添加的操作</param>
        public void Push(Operation operation)
        {
            this.Push(new OperationStack(operation.Position.ToString()), DateTime.Now);
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        /// <returns></returns>
        public OperationStack Undo()
        {
            var stack = Pop();

            Undoing?.Invoke(stack.Item1, stack.Item2);

            return stack.Item1;
        }

        public OperationStack Redo()
        {
            var stack = RedoStack.Pop();
            Redoing?.Invoke(stack.Item1, stack.Item2);
            return stack.Item1;
        }

        public delegate void DoingEventHandler(OperationStack operations, DateTime recordTime);
        public event DoingEventHandler Undoing;
        public event DoingEventHandler Redoing;
    }

    internal delegate void ItemModifiedHandler<in T, in U>(string propertyName, T oldItem, U newItem, string descrption);
}
