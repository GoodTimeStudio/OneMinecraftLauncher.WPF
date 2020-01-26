using GoodTimeStudio.OneMinecraftLauncher.Core.Downloading;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading
{
    public class DownloadManager : BindableBase
    {
        /// <summary>
        /// Maximum of the downloading item in the same time
        /// </summary>
        private static readonly int MaxDownloadingCount = 10;

        private CancellationTokenSource cancellationTokenSource;

        private List<DownloadItem> _list;
        public IDownloadSource Source;
        private bool isDownloading = false;

        private int _ItemCount;
        public int ItemCount
        {
            get => _ItemCount;
            set
            {
                _ItemCount = value;
                ProgressTextInCount = DownloadedItemCount + " / " + value;
            }
        }

        private int _DownloadedItemCount;
        public int DownloadedItemCount
        {
            get => _DownloadedItemCount;
            set
            {
                _DownloadedItemCount = value;
                ProgressTextInCount = value + " / " + ItemCount;
            }
        }

        private double _TotalProgress;
        public double TotalProgress
        {
            get => _TotalProgress;
            set => SetProperty(ref _TotalProgress, value);
        }

        private string _ProgressTextInCount;
        public string ProgressTextInCount
        {
            get => _ProgressTextInCount;
            set => SetProperty(ref _ProgressTextInCount, value);
        }

        public DownloadManager(List<DownloadItem> list, IDownloadSource source)
        {
            _list = list;
            Source = source;
            ItemCount = list.Count;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task DownloadAll()
        {
            if (!isDownloading)
            {
                List<Task> tasks = new List<Task>(MaxDownloadingCount);

                foreach(DownloadItem item in _list)
                {
                    if (tasks.Count >= MaxDownloadingCount)
                    {
                        Task completed = await Task.WhenAny(tasks);
                        tasks.Remove(completed);
                    }
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    tasks.Add(item.Start(cancellationTokenSource));
                }

                await Task.WhenAll(tasks);
            }
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

    }
}
