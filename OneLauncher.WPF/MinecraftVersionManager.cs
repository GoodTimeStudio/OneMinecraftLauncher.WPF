using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF
{
    public class MinecraftVersionManager
    {
        public static readonly string VersionManifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public static MinecraftVersionsList AllVersionsList;
        private static List<KMCCC.Launcher.Version> LocalVersionsList;

        public static void Init()
        {
            LocalVersionsList = new List<KMCCC.Launcher.Version>();
            LocalVersionsList.AddRange(CoreManager.CoreMCL.Core.GetVersions());
            RefreshMinecraftVersions();                   
        }

        #region local version
        public static MinecraftVersion GetLatestRelease()
        {
            MinecraftVersion latest = null;

            if (AllVersionsList != null)
            {
                latest = AllVersionsList.versions.Find(v => v.id == AllVersionsList.latest.release);
                if (latest != null)
                {
                    return latest;
                }
            }

            foreach (MinecraftVersion v in LocalVersionsList.Where(t => t.type == "release"))
            {
                if (latest == null)
                {
                    latest = v;
                    continue;
                }

                if (v.time > latest.time)
                {
                    latest = v;
                }
            }

            return latest;
        }

        #endregion

        public async static void RefreshMinecraftVersions()
        {
            string json = await GetMinecraftVersionManifestAsync();

            if (!string.IsNullOrWhiteSpace(json))
            {
                MinecraftVersionsList list = JsonConvert.DeserializeObject<MinecraftVersionsList>(json);

                if (list != null)
                {
                    AllVersionsList = list;
                }
            }
        }

        public static List<MinecraftVersion> GetReleaseVersionsList()
        {
            if (AllVersionsList != null)
            {
                return AllVersionsList.versions.Where(t => t.type == "release").ToList();
            }
            return null;
        }

        /// <summary>
        /// Get minecraft version manifest.
        /// </summary>
        /// <returns>version manifest json</returns>
        private async static Task<string> GetMinecraftVersionManifestAsync()
        {
            string json;
            try
            {
                using (HttpClient client = new HttpClient(new WebRequestHandler { CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Default) }))
                {
                    json = await client.GetStringAsync(VersionManifestUrl);
                }
            }
            catch (HttpRequestException)
            {
                return string.Empty;
            }

            return json;
        }
    }

}
