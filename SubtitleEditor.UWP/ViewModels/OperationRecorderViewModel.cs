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
        }

        public OperationRecorderViewModel()
        {
            this.CollectionChanged += OperationRecorderViewModel_CollectionChanged;
        }

        private void RegisterEventsForRecorder()
        {
            
        }

        private void OperationRecorderViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsUndoEnable = (Items.Count == 0);
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
