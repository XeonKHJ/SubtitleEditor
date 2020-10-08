using SubtitleEditor.UWP.History;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleEditor.UWP.ViewModels
{
    public class HistoryViewModel : ObservableCollection<DialogueViewModel>, INotifyPropertyChanged
    {
        private OperationRecorder _recorder;

        public HistoryViewModel(OperationRecorder recorder)
        {
            _recorder = recorder;
        }

        public HistoryViewModel()
        {

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
        }
    }
}
