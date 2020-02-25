using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace SubtitleEditor.UWP.Controls
{
    public sealed class StandAloneMediaTransportControls : Control
    {
        public StandAloneMediaTransportControls()
        {
            this.DefaultStyleKey = typeof(StandAloneMediaTransportControls);
        }

        public IMediaPlaybackSource MediaSource
        {
            get { return (IMediaPlaybackSource)GetValue(MediaSourceProperty); }
            set { SetValue(MediaSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaSourceProperty =
            DependencyProperty.Register("MediaSource", typeof(int), typeof(StandAloneMediaTransportControls), new PropertyMetadata(0));


    }
}
