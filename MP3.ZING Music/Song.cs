using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3.ZING_Music
{
    public class Song
    {
        private string name;
        private string singer;
        private string url;
        private int pos;
        private string lyric;
        private string sourceUrl;
        private string media;
        private double position;
        private double maxLength;

        public string Name { get => name; set => name = value; }
        public string Singer { get => singer; set => singer = value; }
        public string Url { get => url; set => url = value; }
        public int Pos { get => pos; set => pos = value; }
        public string Lyric { get => lyric; set => lyric = value; }
        public string SourceUrl { get => sourceUrl; set => sourceUrl = value; }
        public string Media { get => media; set => media = value; }
        public double Position { get => position; set => position = value; }
        public double MaxLength { get => maxLength; set => maxLength = value; }
    }
}
