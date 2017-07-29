using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace MP3.ZING_Music
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string VN = @"http://m.nhaccuatui.com/playlist/top-20-bai-hat-viet-nam-nhaccuatui-tuan-292017-va.pmGGLD7NSiHX.html";
        public static string US = @"http://m.nhaccuatui.com/playlist/top-20-bai-hat-au-my-nhaccuatui-tuan-292017-va.oF9tz1Yqd2V8.html";
        public static string KP = @"http://m.nhaccuatui.com/playlist/top-20-bai-hat-han-quoc-nhaccuatui-tuan-292017-va.EVPH6A3YMl44.html";


        public HttpClient httpClient = new HttpClient();
        public Song CurrentSong;

        public ObservableCollection<Song> IlistBXHV = new ObservableCollection<Song>();/*BXH nhạc Việt Nam*/
        public ObservableCollection<Song> IlistBXHU = new ObservableCollection<Song>();/*BXH nhạc US-UK*/
        public ObservableCollection<Song> IlistBXHK = new ObservableCollection<Song>();/*BXH nhạc Hàn Quốc*/

        public MainWindow()
        {
            InitializeComponent();
            UCSongPlayer.BackToMain += UCSongPlayer_BackToMain;//Event BackButton_Click
            UCSongPlayer.BtnNextClicked += UCSongPlayer_BtnNextClicked;
            UCSongPlayer.BtnPreviousClicked += UCSongPlayer_BtnPreviousClicked;

            tgbVN.IsChecked = true;
            Crawl();//Lấy data từ nhaccuatui.com
            lbSongs.ItemsSource = IlistBXHV;

           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // DownloadPlaylist();
        }


        #region Crawl data from nhaccuatui.com
        /// <summary>
        /// Load bảng xếp hạng hiện tại từ nhaccuatui.com
        /// </summary>
        public void Crawl()
        {
            Task t0 = new Task(() => { DownloadPlaylist(); });
            t0.Start();
            Task t1 = new Task(() => { CrawlBXH(VN, IlistBXHV); }); /*Load BXH nhạc Việt Nam)*/
            t1.Start();
            Task t2 = new Task(() => { CrawlBXH(US, IlistBXHU); });//Load BXH nhạc US-UK*/
            t2.Start();
            Task t3 = new Task(() => { CrawlBXH(KP, IlistBXHK); });//Load BXH nhạc Hàn Quốc*/
            t3.Start();
        }
      
        /// <summary>
        /// Lấy BXH từ 1 subPage của nhaccuatui.com
        /// </summary>
        /// <param name="url">Link BXH</param>
        /// <param name="IlbSongs">Listbox lưu BXH</param>
        public void CrawlBXH(string url, ObservableCollection<Song> IlbSongs)
        {
            App.Current.Dispatcher.Invoke((Action)delegate //Invoke tránh lỗi "This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread"
            {
               IlbSongs.Clear();
            });
            int pos;
            string songName;
            string songUrl = "";
            string singer;

            string html = httpClient.GetStringAsync(url).Result;//Lấy full code html từ link

            string pattern = @"<span class=""ico-phone""></span>(.*?)</p>";
            var listBXH = Regex.Matches(html, pattern, RegexOptions.Singleline);//Lấy code danh sách các bài hát

            //Lấy danh sách các bài hát trong BXH:
            foreach (var topSongs in listBXH)
            {
                songName = Regex.Match(topSongs.ToString(), @"title=""(.*?)""", RegexOptions.Singleline).Value.ToString().Replace(@"title=""", "").Replace(@"""", "").Trim();
                songUrl = Regex.Match(topSongs.ToString(), @"href=""(.*?)""", RegexOptions.Singleline).Value.ToString().Replace(@"href=""", "").Replace(@"""", "").Replace("m.","www.");
                singer = Regex.Match(topSongs.ToString(), @"<p>(.*?)</p>", RegexOptions.Singleline).Value.ToString().Replace(@"<p>","").Replace(@"</p>","");
                pos = int.Parse(Regex.Match(topSongs.ToString(), @"'(.*?)'", RegexOptions.Singleline).Value.ToString().Replace("'", "")) + 1;


                App.Current.Dispatcher.Invoke((Action)delegate //Invoke tránh lỗi "This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread"
                {
                    IlbSongs.Add(new Song() { Name = songName, Singer = singer, SourceUrl = songUrl, Pos = pos, Lyric = "Lời bài hát chưa được cập nhật", Media = AppDomain.CurrentDomain.BaseDirectory + "Song\\" + songName + ".mp3" });
                });
            }

            HttpClient httplink = new HttpClient();
            httplink.BaseAddress = new Uri(songUrl);

            html = httplink.GetStringAsync("").Result;
            var temp = Regex.Matches(html, @"id=""itemSong(.*?)itemprop=""url"" />", RegexOptions.Singleline);
            for(int i = 0; i < 20; i++)
            {
                IlbSongs[i].Url = Regex.Match(temp[i].ToString(), @"""http(.*?)html", RegexOptions.Singleline).Value.ToString().Replace(@"""", "");
            }
        }

        /// <summary>
        /// Download a song
        /// </summary>
        /// <param name="IlbSongs"></param>
        public void DownLoad(ObservableCollection<Song> IlbSongs)
        {
            foreach (Song song in IlbSongs)
            {
                DownloadSong(song);
            }
        }

        /// <summary>
        /// Download a song
        /// </summary>
        /// <param name="songinfo"></param>
        public void DownloadSong(Song songinfo)
        {
            string songname = AppDomain.CurrentDomain.BaseDirectory + "Song\\" + songinfo.Name + ".mp3";
            if (!File.Exists(songname))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(songinfo.SourceUrl, songname);

            }
        }

        /// <summary>
        /// Download playlist
        /// </summary>
        public void DownloadPlaylist()
        {
            Task t4 = new Task(() => { DownLoad(IlistBXHV); });
            t4.Start();
            Task t5 = new Task(() => { DownLoad(IlistBXHU); });
            t5.Start();
            Task t6 = new Task(() => { DownLoad(IlistBXHK); });
            t6.Start();
        }

        #endregion

        #region Page selected
        /// <summary>
        /// Back to main page while click Back button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCSongPlayer_BackToMain(object sender, EventArgs e)
        {
            //UCSongPlayer.mdaSongPlayer.Stop();
            grdTop10.Visibility = Visibility.Visible;
            UCSongPlayer.Visibility = Visibility.Hidden;

        }


        /// <summary>
        /// Go to play-media page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            grdTop10.Visibility = Visibility.Hidden;
            UCSongPlayer.Visibility = Visibility.Visible;
            
            Song song = (sender as Grid).DataContext as Song;
            if (song != CurrentSong)
                PlaySong(song);

        }

        #endregion

        #region Song action
        /// <summary>
        /// Play a song
        /// </summary>
        /// <param name="song"></param>
        public void PlaySong(Song song)
        {
            UCSongPlayer.DataContext = song;
            CurrentSong = song;


            if (song.Lyric == "Lời bài hát chưa được cập nhật")
            {
                song.Lyric = GetLyric(song.Url);
                UCSongPlayer.txblLyric.Text = song.Lyric;

                song.SourceUrl = GetSource(song.SourceUrl);
            }

            UCSongPlayer.SongPlaying = song;
            UCSongPlayer.mdaSongPlayer.Source = new Uri(song.Media);
            UCSongPlayer.mdaSongPlayer.Play();
        }

        /// <summary>
        /// Get lyric of a song
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetLyric(string url)
        {
            if(url == null)
            {
                UCSongPlayer_BackToMain(new object(), new EventArgs());
                return "Lời bài hát chưa được cập nhật";
            }

            HttpClient httpSong = new HttpClient();
            httpSong.BaseAddress = new Uri(url);
            //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors){return true;};

            string htmlSong = WebUtility.HtmlDecode(httpSong.GetStringAsync("").Result);

            string lyric = Regex.Match(Regex.Match(htmlSong, @"<p id=""divLyric""(.*?)</p>", RegexOptions.Singleline).Value.ToString(), @"<br />(.*?)</p>", RegexOptions.Singleline).Value.ToString();
            lyric = lyric.Replace("</p>", "").Replace("<br />", "\n");

            return lyric;
        }

        /// <summary>
        /// Get source media of a song
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetSource(string url)
        {
            url = url.Replace("www", "m");
            string sourceurl = "";
            HttpClient httpSong1 = new HttpClient();
            httpSong1.BaseAddress = new Uri(url);

            string temp = httpSong1.GetStringAsync("").Result;

            sourceurl = Regex.Match(Regex.Match(temp, @"<div class=""download"">(.*?)mp3", RegexOptions.Singleline).Value.ToString(), @"http(.*?)mp3", RegexOptions.Singleline).Value.ToString();
            return sourceurl; 
        }

        #endregion

        #region ToggleButtons region
        /// <summary>
        /// Button BXHVN Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click1(object sender, RoutedEventArgs e)
        {
            if (tgbVN.IsChecked == true)
            {
                tgbUSUK.IsChecked = false;
                tgbKpop.IsChecked = false;

            }

            lbSongs.ItemsSource = IlistBXHV;

        }

        /// <summary>
        /// Button BXHUSUK Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click2(object sender, RoutedEventArgs e)
        {
            if (tgbUSUK.IsChecked == true)
            {
                tgbVN.IsChecked = false;
                tgbKpop.IsChecked = false;

            }
            lbSongs.ItemsSource = IlistBXHU;
        }

        /// <summary>
        /// Button BXHHQ Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click3(object sender, RoutedEventArgs e)
        {
            if (tgbKpop.IsChecked == true)
            {
                tgbVN.IsChecked = false;
                tgbUSUK.IsChecked = false;

            }
            lbSongs.ItemsSource = IlistBXHK;
        }


        #endregion

        #region Toolbar
        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            PlaySong(lbSongs.Items[0] as Song);
            grdTop10.Visibility = Visibility.Hidden;
            UCSongPlayer.Visibility = Visibility.Visible;
            UCSongPlayer.tgbAutoPlay.IsChecked = true;
        }

        private void btnOpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            if(tgbVN.IsChecked == true)
            {
                Process.Start(VN.Replace("m.", "www."));
            }
            else if (tgbUSUK.IsChecked == true)
            {
                Process.Start(US.Replace("m.", "www."));
            }
            else if (tgbKpop.IsChecked == true)
            {
                Process.Start(KP.Replace("m.", "www."));
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            Crawl();//Lấy data từ nhaccuatui.com
            lbSongs.ItemsSource = IlistBXHV;
        }

        #endregion

        #region Choose a song
        /// <summary>
        /// Phát bài trước
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCSongPlayer_BtnPreviousClicked(object sender, EventArgs e)
        {
            int pos = lbSongs.Items.IndexOf(CurrentSong);

            if (pos > 0)
                PlaySong(lbSongs.Items[pos - 1] as Song);
        }
        /// <summary>
        /// Phát bài tiếp theo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UCSongPlayer_BtnNextClicked(object sender, EventArgs e)
        {
            int pos = lbSongs.Items.IndexOf(CurrentSong);

            if (pos < 19)
                PlaySong(lbSongs.Items[pos + 1] as Song);
        }
        #endregion
    }
}
