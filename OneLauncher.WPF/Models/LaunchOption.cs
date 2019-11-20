using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class LaunchOption : LaunchOptionBase
    {
        private bool _IsReady;
        [JsonIgnore]
        public bool IsReady
        {
            get => _IsReady;
            set
            {
                this.SetProperty(ref _IsReady, value);
                this.OnPropertyChanged(nameof(IsNotReady));
            }
        }

        public bool IsNotReady
        {
            get => !IsReady;
        }

        private ImageSource _IconImage;
        public ImageSource IconImage
        {
            get => _IconImage;
            set => SetProperty(ref _IconImage, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        public LaunchOption(string name) : base(name)
        {
        }
    }
}
