using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading
{
    public class DownloadItem : BindableBase
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Uri Uri { get; set; }

        public ItemState State { get; set; }

        private Action<DownloadItem> DownloadCompletedAction;

        public DownloadItem(string name, string path, Uri uri)
        {
            Name = name;
            Path = path;
            Uri = uri;
            Progress = 0;
            State = ItemState.Standby;
        }

        public DownloadItem(string name, string path, Uri uri, Action<DownloadItem> downloadCompletedAction) : this(name, path, uri)
        {
            DownloadCompletedAction = downloadCompletedAction;
        }

        public async Task Start(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            if (State != ItemState.Standby)
            {
                return;
            }

            State = ItemState.Downloading;
            Console.WriteLine("Attempt to download: " + Name);

            WebClient client = null;
            try
            {
                FileInfo file = new FileInfo(Path);
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }

                client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                await client.DownloadFileTaskAsync(Uri, Path);

                Console.WriteLine("Download completed: " + Name);
                State = ItemState.Completed;
                DownloadCompletedAction?.Invoke(this);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Download canceled: " + Name);
                State = ItemState.Cancelled;
            }
            catch (Exception e)
            {
                Console.WriteLine("Downlaod failed: " + Name);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                State = ItemState.Failed;
                ErrorText = e.Message;
            }
            finally
            {
                client?.Dispose();
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            DisplaySize = Math.Round(e.BytesReceived / 1024d / 1024d, 2) + "/" + Math.Round(e.TotalBytesToReceive / 1024d / 1024d, 2) + " Mb";
        }

        private double progress;
        public double Progress
        {
            get => progress;
            set
            {
                this.SetProperty(ref progress, value);
                this.OnPropertyChanged(nameof(ProgressText));
            }
        }

        private string displaySize;
        public string DisplaySize
        {
            get => displaySize;
            set => this.SetProperty(ref displaySize, value);
        }

        public string ProgressText
        {
            get => Progress + "%";
        }

        private string errText;
        public string ErrorText
        {
            get => errText;
            set => this.SetProperty(ref errText, value);
        }

    }

    public enum ItemState
    {
        Standby,
        Downloading,
        Completed,
        Pause,
        Failed,
        Cancelled
    }

}
