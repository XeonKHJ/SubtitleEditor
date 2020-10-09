using SubtitleEditor.UWP.History;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace SubtitleEditor.UWP.ViewModels
{
    public class OperationRecorderViewModel : ObservableCollection<DialogueViewModel>, INotifyPropertyChanged
    {
        private OperationRecorder _recorder;
        public OperationRecorderViewModel(OperationRecorder recorder)
        {
            _recorder = recorder;
            RegisterEventsForRecorder();
            UpdateButtonStatus(null, null);
        }

        public OperationRecorderViewModel()
        {
            
        }

        private void RegisterEventsForRecorder()
        {
            if(_recorder != null)
            {
                _recorder.Recorded += Recorder_Recorded;
                _recorder.Undone += Recorder_Undone;
                _recorder.Redone += Recorder_Redone;
            }
        }

        private void Recorder_Redone(OperationStack operations, DateTime recordTime)
        {
            System.Diagnostics.Debug.WriteLine("Recorder_Redone");
            UpdateButtonStatus(null, null);
        }

        private void Recorder_Undone(OperationStack operations, DateTime recordTime)
        {
            System.Diagnostics.Debug.WriteLine("Recorder_Undone");
            UpdateButtonStatus(null, null);
        }

        private void Recorder_Recorded(OperationStack operations, DateTime recordTime)
        {
            System.Diagnostics.Debug.WriteLine("Recorder_Recorded");
            UpdateButtonStatus(null, null);
        }

        private void UpdateButtonStatus(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsUndoEnable = (_recorder.UndoStack.Count != 0);
            IsRedoEnable = (_recorder.RedoStack.Count != 0);
        }

        private bool _isRedoEnable;
        public bool IsRedoEnable
        {
            set
            {
                _isRedoEnable = value;
                OnPropertyChanged();
            }
            get
            {
                return _isRedoEnable;
            }
        }

        private bool _isUndoEnable;
        public bool IsUndoEnable
        {
            set
            {
                _isUndoEnable = value;
                OnPropertyChanged();
            }
            get
            {
                return _isUndoEnable;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadRecorder(OperationRecorder recorder)
        {
            _recorder = recorder;
            RegisterEventsForRecorder();
            UpdateButtonStatus(null, null);
        }

        public void Undo()
        {
            _recorder.Undo();
        }

        public void Redo()
        {
            _recorder.Redo();
        }
    }
}
