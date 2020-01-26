using static GoodTimeStudio.OneMinecraftLauncher.WPF.CoreManager;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
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
using KMCCC.Authentication;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models.Minecraft;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using System.IO;
using System.Net.Http;
using KMCCC.Launcher;
using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using System.Collections.ObjectModel;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Models;
using System.Timers;
using System.Threading;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.View
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : UserControl
    {

        private UserDialog _UserDialog;
        private System.Timers.Timer AutoSaveTimer;

        public StartPage()
        {
            InitializeComponent();
            Loaded += StartPage_Loaded;

            ViewModel.AccountTypesList = new ObservableCollection<AccountType>();
            AccountTypes.AllAccountTypes.ForEach(a => { ViewModel.AccountTypesList.Add(a); });
            ViewModel.LaunchOptionsList = Config.INSTANCE.LaunchOptions;

            AutoSaveTimer = new System.Timers.Timer(20000); //10 sec
            AutoSaveTimer.AutoReset = false;
            AutoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;

            //Init dialogs
            _UserDialog = new UserDialog(ViewModel);

            //Init launching flyout view model
            MainWindow.Current.LaunchingFlyout.DataContext = ViewModel;

        }

        private void AutoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Config.SaveConfigToFile();
        }

        private async void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Config.INSTANCE != null) // determine launcher have inited
            {
                if (!string.IsNullOrEmpty(Config.INSTANCE.AccountType))
                {
                    ViewModel.SelectedAccountType = AccountTypes.GetAccountTypeFromTag(Config.INSTANCE.AccountType);
                }

                ViewModel.User = Config.INSTANCE.User;
                ViewModel.PlayerName = Config.INSTANCE.Playername;

                if (CoreMCL.UserAuthenticator == null)
                {
                    ViewModel.UserDialogResultString = string.Empty;
                    bool offline = ViewModel.SelectedAccountType == AccountTypes.Offline;
                    if (offline)
                    {
                        ViewModel.SetupUserDialog(Models.UserDialogState.Offline);
                    }
                    else
                    {
                        ViewModel.SetupUserDialog(Models.UserDialogState.Logging);
                    }

                    AuthenticationInfo info = await Auth(ViewModel.SelectedAccountType, ViewModel.User, Config.INSTANCE.Password, true);
                    IAuthenticator authenticator = GenAuthenticatorFromAuthInfo(info);
                    if (authenticator != null)
                    {
                        CoreMCL.UserAuthenticator = authenticator;
                        ViewModel.PlayerName = info.DisplayName;
                        if (!offline)
                        {
                            ViewModel.SetupUserDialog(Models.UserDialogState.LoggedIn);
                        }
                    }
                    else
                    {
                        ViewModel.UserDialogResultString = "用户验证失败 \r\n " + info?.Error;
                        ViewModel.SetupUserDialog(Models.UserDialogState.Input);
                        await MainWindow.Current.ShowMetroDialogAsync(_UserDialog, DefaultDialogSettings);
                    }
                }

                if (ViewModel.LaunchOptionsList != null)
                {
                    foreach (LaunchOption opt in ViewModel.LaunchOptionsList)
                    {
                        if (!MinecraftVersionManager.VersionIdList.Contains(opt.versionId))
                        {
                            MinecraftVersionManager.VersionIdList.Add(opt.versionId);
                        }
                    }
                }
            }
        }

        private void Tile_Download_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.GoToPage(typeof(DownloadPage));
        }

        private void Tile_Settings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.GoToPage(typeof(SettingsPage));
        }

        private void Button_UserDialog_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.ShowMetroDialogAsync(_UserDialog, DefaultDialogSettings);
        }

        private async void Tile_Launch_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.ShowLaunchingFlyout();
            bool flag = await launch();
            if (flag)
            {
                MainWindow.Current.CloseLaunchingFlyout();
            }
        }

        private async Task<bool> launch()
        {
            Config.INSTANCE.User = ViewModel.User;
            AutoSaveTimer.Stop(); // trigger auto save

            // Checking minecraft version
            ViewModel.ProgressDescriptionText = "正在检查版本文件";
            KMCCC.Launcher.Version kver = CoreManager.CoreMCL.Core.GetVersion(ViewModel.SelectedLaunchOption.versionId);

            if (kver == null)
            {
                await MainWindow.Current.ShowMessageAsync("启动失败", "版本未指定，请选择一个要启动的Minecraft版本");
                return true;
            }
            if (CoreMCL.UserAuthenticator == null)
            {
                await MainWindow.Current.ShowMessageAsync("启动失败", "未指定用户，请前往账户设置选择要登入Minecraft的用户");
                return true;
            }

            #region Check libraries and natives
            ViewModel.ProgressDescriptionText = "正在检查库文件...";

            List<MinecraftAssembly> missing = null;
            await Task.Run(() =>
            {
                missing = CoreMCL.CheckLibraries(kver);
                missing?.AddRange(CoreMCL.CheckNatives(kver));
            });
            if (missing?.Count > 0)
            {
                ObservableCollection<DownloadItem> downloads = new ObservableCollection<DownloadItem>();
                ViewModel.ProgressDescriptionText = "正在下载缺失的库文件...";
                missing.ForEach(lib =>
                {
                    if (Uri.TryCreate(lib.Url, UriKind.Absolute, out Uri uri))
                    {
                        DownloadItem item = new DownloadItem(lib.Name, lib.Path, uri, i =>
                        {
                            downloads.Remove(i);
                        });
                        downloads.Add(item);
                    }
                });

                DownloadManager manager = new DownloadManager(new List<DownloadItem>(downloads), CoreManager.DownloadSource);
                ViewModel.DownloadQuene = downloads;
                await manager.DownloadAll();
                if (downloads.Count > 0)
                {
                    ViewModel.ProgressDescriptionText = "下载失败";
                    return false;
                }
            }
            #endregion

            // Check Assets
            ViewModel.ProgressDescriptionText = "正在检查资源文件";
            if (!CheckAssetsIndex(kver))
            {
                ViewModel.ProgressDescriptionText = "正在获取资源元数据";
                try
                {
                    await Task.Run(async () =>
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string json = await client.GetStringAsync(kver.AssetsIndex.Url);
                            string path = string.Format(@"{0}\assets\indexes\{1}.json", CoreMCL.Core.GameRootPath, kver.Assets);
                            FileInfo fileInfo = new FileInfo(path);
                            if (!fileInfo.Directory.Exists)
                            {
                                fileInfo.Directory.Create();
                            }
                            fileInfo.Create().Dispose();
                            File.WriteAllText(path, json);
                        }
                    });
                }
                catch (HttpRequestException ex)
                {
                    await MainWindow.Current.ShowMessageAsync("获取资源元数据失败", ex.Message + ex.StackTrace, MessageDialogStyle.Affirmative, DefaultDialogSettings);
                    return true;
                }
                catch (IOException ex)
                {
                    await MainWindow.Current.ShowMessageAsync("获取资源元数据失败", ex.Message + ex.StackTrace, MessageDialogStyle.Affirmative, DefaultDialogSettings);
                    return true;
                }
            }

            ViewModel.ProgressDescriptionText = "正在检查资源文件...";
            (bool hasValidIndex, List<MinecraftAsset> missingAssets) assetsResult = (false, null);
            await Task.Run(() =>
            {
                assetsResult = CoreMCL.CheckAssets(kver);
            });
            if (!assetsResult.hasValidIndex)
            {
                await MainWindow.Current.ShowMessageAsync("获取资源元数据失败", "发生未知错误，无法获取有效的资源元数据，我们将继续尝试启动游戏，但这可能会导致游戏中出现无翻译和无声音等问题");
            }
            else
            {
                if (assetsResult.missingAssets.Count > 0)
                {
                    ViewModel.ProgressDescriptionText = "正在下载资源文件...";
                    ObservableCollection<DownloadItem> downloads = new ObservableCollection<DownloadItem>();
                    assetsResult.missingAssets.ForEach(ass =>
                    {
                        if (Uri.TryCreate(ass.GetDownloadUrl(), UriKind.Absolute, out Uri uri))
                        {
                            DownloadItem item = new DownloadItem("资源: " + ass.Hash, CoreMCL.Core.GameRootPath + "\\" + ass.GetPath(), uri, i => 
                            {
                                downloads.Remove(i);
                            });
                            downloads.Add(item);
                        }
                    });

                    DownloadManager manager = new DownloadManager(new List<DownloadItem>(downloads), CoreManager.DownloadSource);
                    ViewModel.DownloadQuene = downloads;
                    await manager.DownloadAll();
                    if (downloads.Count > 0)
                    {
                        ViewModel.ProgressDescriptionText = "下载失败";
                        return false;
                    }
                }
            }

            ViewModel.ProgressDescriptionText = "正在启动...";
            LaunchResult result = CoreMCL.Launch(ViewModel.SelectedLaunchOption);
            if (!result.Success)
            {
                await MainWindow.Current.ShowMessageAsync("启动失败", result.ErrorMessage + "\r\n" + result.Exception);
            }
            return true;
        }

        private bool CheckAssetsIndex(KMCCC.Launcher.Version kver)
        {
            var result = CoreMCL.CheckAssets(kver);
            return result.hasValidIndex;
        }

        private void Button_AddOption_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LaunchOptionsList.Add(new LaunchOption("未命名的配置"));
        }

        private void OptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AutoSaveTimer.Stop();
            AutoSaveTimer.Start();
        }

        private void Version_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoSaveTimer.Stop();
            AutoSaveTimer.Start();
        }
    }
}
