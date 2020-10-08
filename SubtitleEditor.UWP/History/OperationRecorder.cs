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
    internal class OperationRecorder : Stack<OperationList>
    {
        /// <summary>
        /// 每个记录器要有一个ID用于于一个文件对应
        /// </summary>
        public string RecorderId { get; private set; }
        public OperationRecorder(StorageFile storageFile)
        {
            if(storageFile != null)
            {
                RecorderId = storageFile.FolderRelativeId;
            }
        }

        public OperationRecorder(string recorderId)
        {
            RecorderId = recorderId;
        }

        /// <summary>
        /// 添加一个操作
        /// </summary>
        /// <param name="operation">要添加的操作</param>
        public void Push(Operation operation)
        {
            this.Push(new OperationList(operation.EditType.ToString()));
        }
    }

    internal delegate void ItemModifiedHandler<in T, in U>(string propertyName, T oldItem, U newItem, string descrption);
}
