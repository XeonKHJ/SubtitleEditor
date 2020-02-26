using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace SubtitleEditor.UWP.Controls.ViewModels
{
    public class MediaPlayerViewModel : INotifyPropertyChanged
    {
        private MediaPlayer _mediaPlayer;
        public MediaPlayerViewModel()
        { }

        public MediaPlayerViewModel(MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string stringPosition = "0";
        public string StringPosition
        {
            set
            {
                stringPosition = value;
                OnPropertyChanged();
            }
            get
            {
                return stringPosition;
            }
        }
        public string StringDuration { set; get; }

        private double timelinePosition = 0.0;
        public double TimelinePosition
        {
            get
            {
                return timelinePosition;
            }
            set
            {
                timelinePosition = value;
                OnPropertyChanged();
            }
        }

        public void UpdateValues()
        {
            
        }

        public void LoadMediaPlayer(MediaPlayer mediaPlayer)
        {

        }
        
        public double TimelineDuration { set; get; }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static int CalculateFrame(TimeSpan span, double frameRate)
        {
            return Convert.ToInt32(span.TotalSeconds * frameRate);
        }
    }

}
