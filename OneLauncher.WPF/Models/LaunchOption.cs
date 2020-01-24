using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

        private int _MaxJvmMemory;
        public int MaxJvmMemory
        {
            get => _MaxJvmMemory;
            set => this.SetProperty(ref _MaxJvmMemory, value);
        }

        [JsonIgnore]
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

    public class VersionLockOption : LaunchOption
    {
        public new readonly string versionId;

        public VersionLockOption(string name) : base(name) { }

        public VersionLockOption(string name, string versionId) : base(name)
        {
            this.versionId = versionId;
        }
    }

    public class LaunchOptionTypesBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = string.Empty;
            if (serializedType == typeof(LaunchOption))
            {
                typeName = "custom";
            }
            else if (serializedType == typeof(VersionLockOption))
            {
                typeName = "version_lock";
            }
            else
            {
                typeName = string.Empty;
            }
        }

        public Type BindToType(string assemblyName, string typeName)

        {
            switch (typeName)
            {
                case "version_lock":
                    return typeof(VersionLockOption);
                default:
                    return typeof(LaunchOption);
            }
        }
    }
}
