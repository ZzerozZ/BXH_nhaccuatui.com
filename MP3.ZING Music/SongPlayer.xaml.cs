using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MP3.ZING_Music
{
    /// <summary>
    /// Interaction logic for SongPlayer.xaml
    /// </summary>
    public partial class SongPlayer : UserControl
    {
        #region Variable and Property

        DispatcherTimer timer; /*Timer*/
        private Song songPlayer; /*Current song*/
        private bool isPlaying; /*Song is playing or not*/
        public Song SongPlaying { get { return songPlayer; } set { songPlayer = value; DownloadSong(songPlayer); } }/*Current song*/

        public bool IsPlaying { get => isPlaying;
            set
            {
                isPlaying = value;
                if(isPlaying)
                {
                    mdaSongPlayer.Pause();
                    btnPlayer.Content = "Play";
                    timer.Stop();
                }
                else
                {
                    mdaSongPlayer.Play();
                    btnPlayer.Content = "Pause";
                    timer.Start();
                }
            }
        } /*Song is playing or not*/
        #endregion

        public SongPlayer()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            isPlaying = false;
        }

        #region Timer
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            SongPlaying.Position++;//Cập nhật thời gian bài hát
            txblTime.Text = FormatTime((int)SongPlaying.Position);//Cập nhật thời gian bài hát
            if (txblTime.Text == txblTotaltime.Text)//The end of song
            {
                if(tgbAutoPlay.IsChecked == false)
                {
                    SongPlaying.Position = 0; //Replay song
                    txblTime.Text = "00:00"; //Reset timer
                    mdaSongPlayer.Position = new TimeSpan(0, 0, 0);
                }
                else
                {
                    btnNextClicked(btnNextSong, new EventArgs());
                }
                
            }

            sldDuration.Value = SongPlaying.Position;//Update duaration 
        }
        #endregion

        #region Event Definition

        private event EventHandler backToMain;
        public event EventHandler BackToMain
        {
            add { backToMain += value; }
            remove { backToMain -= value; }
        }

        private event EventHandler btnNextClicked;
        public event EventHandler BtnNextClicked
        {
            add { btnNextClicked += value; }
            remove { btnNextClicked -= value; }
        }

        private event EventHandler btnPreviousClicked;
        public event EventHandler BtnPreviousClicked
        {
            add { btnPreviousClicked += value; }
            remove { btnPreviousClicked -= value; }
        }
        #endregion

        #region Media
        public void DownloadSong(Song songinfo)
        {
            string songname = AppDomain.CurrentDomain.BaseDirectory + "Song\\" + songinfo.Name + ".mp3";
            if(!File.Exists(songname))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(songinfo.SourceUrl, songname);

            }
        }

        private void mdaSongPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SongPlaying.MaxLength = mdaSongPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            sldDuration.Maximum = SongPlaying.MaxLength;
            SongPlaying.Position = 0;
            timer.Start();
            isDraging = false;
            txblTime.Text = "00:00";
            txblTotaltime.Text = FormatTime((int)SongPlaying.MaxLength);
        }
        public bool isDraging; 
        private void sldDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(isDraging)
            {
                SongPlaying.Position = sldDuration.Value;
                txblTime.Text = FormatTime((int)SongPlaying.Position);
                mdaSongPlayer.Position = new TimeSpan(0, 0, (int)SongPlaying.Position);
            }
                
        }
        #endregion

        #region Format time

        /// <summary>
        /// Format time to "hh:mm:ss"
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private string FormatTime(int time)
        {
            string t = "";
            t += (time / 3600 < 10)? "0" + (time / 3600).ToString() + ":" : (time / 3600).ToString() + ":";
            t += (time / 60 < 10) ? "0" + (time / 60).ToString() + ":" : (time / 60).ToString() + ":";
            t += (time % 60 < 10) ? "0" + (time % 60).ToString() : (time % 60).ToString();

            if (t[0] == t[1] && t[1] == '0')
                t = t.Remove(0, 3);
            return t;
        }
        #endregion

        #region Slove event
        private void sldDuration_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraging = true;
        }

        private void sldDuration_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraging = false;
        }

        private void btnPlayer_Click(object sender, RoutedEventArgs e)
        {
            IsPlaying = !IsPlaying;
        }

        private void btnOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(SongPlaying.Url);
        }

        private void btnOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Song\\" + SongPlaying.Name + ".mp3";
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "Song\\", string.Format("/select, \"" + filePath + "\""));
        }

        #endregion

        #region Send data to main page

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (backToMain != null)
            {
                backToMain(this, new EventArgs());
            }
        }

        private void btnNextSong_Click(object sender, RoutedEventArgs e)
        {
            btnNextClicked(this, new EventArgs());
        }

        private void btnPreviousSong_Click(object sender, RoutedEventArgs e)
        {
            btnPreviousClicked(this, new EventArgs());
        }

        #endregion
    }
}
