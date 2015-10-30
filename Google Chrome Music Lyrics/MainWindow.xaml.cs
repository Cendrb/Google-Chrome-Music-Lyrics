using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

namespace Google_Chrome_Music_Lyrics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpListener listener = new HttpListener();

        public MainWindow()
        {
            InitializeComponent();

            listener.Prefixes.Add("http://127.0.0.1:6923/");

            listener.Start();

            Run();
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
                {
                    try
                    {
                        while (listener.IsListening)
                        {
                            ThreadPool.QueueUserWorkItem((c) =>
                            {
                                HttpListenerContext context = c as HttpListenerContext;
                                try
                                {
                                    byte[] data = new byte[1024];
                                    context.Request.InputStream.Read(data, 0, data.Length);
                                    string rawString = Encoding.UTF8.GetString(data).Replace("\0", String.Empty);
                                    SongData song = SongData.Parse(rawString);
                                    song.FetchLyrics();

                                    Dispatcher.Invoke(() => loadFromSongToGUI(song));
                                    notifyAndroidDevice(song);

                                    byte[] buf = Encoding.UTF8.GetBytes("penis");
                                    context.Response.ContentLength64 = buf.Length;
                                    context.Response.OutputStream.Write(buf, 0, buf.Length);  
                                }
                                catch (Exception e)
                                {

                                }
                                finally
                                {
                                    context.Response.OutputStream.Close();
                                }
                            }, listener.GetContext());
                        }
                    }
                    catch (Exception e)
                    {

                    }
                });
        }

        private void loadFromSongToGUI(SongData song)
        {
            titleText.Content = song.Title;
            artistAlbumText.Content = song.Artist + " - " + song.Album;
            lyricsText.Content = song.Lyrics;
            Topmost = true;
            Topmost = false;
        }

        private void notifyAndroidDevice(SongData song)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse("192.168.1.92"), 6969);
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.WriteLine(song.Title);
                writer.WriteLine(song.Artist);
                writer.WriteLine(song.Album);
                writer.WriteLine(song.Lyrics);
                writer.Close();
                client.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
