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
    public class OperationRecorder
    {
        public enum DoingType { Redo, Undo};

        public Stack<Tuple<OperationStack, DateTime>> RedoStack { get; } = new Stack<Tuple<OperationStack, DateTime>>();
        public Stack<Tuple<OperationStack, DateTime>> UndoStack { get; } = new Stack<Tuple<OperationStack, DateTime>>();

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
        /// 记录操作，记录时间为记录时的系统时间。
        /// </summary>
        /// <param name="operations"></param>
        public void Record(OperationStack operations)
        {
            var operationTime = DateTime.Now;
            if (operations != null)
            {
                Recording?.Invoke(operations, operationTime);
                UndoStack.Push(new Tuple<OperationStack, DateTime>(operations, operationTime));
                Recorded(operations, operationTime);
            }
        }

        /// <summary>
        /// 将会用作主要的函数来记录操作。
        /// </summary>
        /// <param name="operations"></param>
        /// <param name="operationTime"></param>
        public void Record(OperationStack operations, DateTime operationTime)
        {
            if(operations != null)
            {
                Recording?.Invoke(operations, operationTime);
                UndoStack.Push(new Tuple<OperationStack, DateTime>(operations, operationTime));
                Recorded?.Invoke(operations, operationTime);
            }
        }

        /// <summary>
        /// 添加一个操作
        /// </summary>
        /// <param name="operation">要添加的操作</param>
        public void Record(Operation operation)
        {
            if(operation != null)
            {
                var stack = new OperationStack(operation.Position.ToString());
                stack.Push(operation);
                var recordTime = DateTime.Now;
                Recording?.Invoke(stack, recordTime);
                UndoStack.Push(new Tuple<OperationStack, DateTime>(stack, recordTime));
                Recorded?.Invoke(stack, recordTime);
            }
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        /// <returns></returns>
        public OperationStack Undo()
        {
            var stack = UndoStack.Pop();

            Undoing?.Invoke(stack.Item1, stack.Item2);

            Undone?.Invoke(stack.Item1, stack.Item2);
            return stack.Item1;
        }

        public OperationStack Redo()
        {
            var stack = RedoStack.Pop();
            Redoing?.Invoke(stack.Item1, stack.Item2);
            Redone?.Invoke(stack.Item1, stack.Item2);
            return stack.Item1;
        }

        public delegate void RecorderEventHandler(OperationStack operations, DateTime recordTime);
        public event RecorderEventHandler Undoing;
        public event RecorderEventHandler Redoing;
        public event RecorderEventHandler Recording;
        public event RecorderEventHandler Recorded;
        public event RecorderEventHandler Redone;
        public event RecorderEventHandler Undone;
    }

    internal delegate void ItemModifiedHandler<in T, in U>(string propertyName, T oldItem, U newItem, string descrption);
}
