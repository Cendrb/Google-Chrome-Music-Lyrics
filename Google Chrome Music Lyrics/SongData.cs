using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Google_Chrome_Music_Lyrics
{
    class SongData
    {
        public string Artist { get; private set; }
        public string Album { get; private set; }
        public string Title { get; private set; }

        public string Lyrics
        {
            get
            {
                if (lyrics == null)
                    throw new InvalidOperationException("No lyrics fetched. Need to call FetchLyrics() method first.");
                else
                    return lyrics;
            }
        }
        private string lyrics;

        public SongData(string artist, string album, string title)
        {
            Artist = artist;
            Album = album;
            Title = title;
        }

        public static SongData Parse(string rawString)
        {
            string[] parts = rawString.Split(';');
            return new SongData(parts[0], parts[1], parts[2]);
        }

        public bool FetchLyrics()
        {
            string title = Title.Replace(".", "").Replace("'", "").Replace("’", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("!", "");
            string artist = Artist.Replace(".", "").Replace("'", "").Replace("’", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("!", "");
            string url = String.Format("http://www.azlyrics.com/lyrics/{0}/{1}.html", artist, title).Replace(" ", "").ToLower();
            WebRequest request = WebRequest.Create(url);

            try
            {
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(reader.ReadToEnd());
                HtmlNode result = document.DocumentNode.SelectNodes("//div[@class='container main-page']/div[@class='row']/div[@class='col-xs-12 col-lg-8 text-center']/div[6]")[0];
                lyrics = result.InnerText;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
