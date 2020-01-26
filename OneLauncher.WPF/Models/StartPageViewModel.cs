using GoodTimeStudio.OneMinecraftLauncher.Core.Models;
using GoodTimeStudio.OneMinecraftLauncher.WPF.Downloading;
using KMCCC.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GoodTimeStudio.OneMinecraftLauncher.WPF.Models
{
    public class StartPageViewModel : BindableBase
    {
        private ObservableCollection<LaunchOption> _LaunchOptionsList;
        public ObservableCollection<LaunchOption> LaunchOptionsList
        {
            get => _LaunchOptionsList;
            set => SetProperty(ref _LaunchOptionsList, value);
        }

        private LaunchOption _SelectedLaunchOption;
        public LaunchOption SelectedLaunchOption
        {
            get => _SelectedLaunchOption;
            set => SetProperty(ref _SelectedLaunchOption, value);
        }

        private ObservableCollection<AccountType> _AccountTypesList;
        public ObservableCollection<AccountType> AccountTypesList
        {
            get => _AccountTypesList;
            set => SetProperty(ref _AccountTypesList, value);
        }

        private AccountType _SelectedAccountType;
        public AccountType SelectedAccountType
        {
            get => _SelectedAccountType;
            set => SetProperty(ref _SelectedAccountType, value);
        }

        private DownloadManager _manager;
        public DownloadManager manager
        {
            get => _manager;
            set => SetProperty(ref _manager, value);
        }

        private ObservableCollection<DownloadItem> _DownloadQuene;
        public ObservableCollection<DownloadItem> DownloadQuene
        {
            get => _DownloadQuene;
            set => SetProperty(ref _DownloadQuene, value);
        }

        private Visibility _DownloadListVisibility;
        public Visibility DownloadListVisibility
        {
            get => _DownloadListVisibility;
            set => SetProperty(ref _DownloadListVisibility, value);
        }

        private string _User;
        /// <summary>
        /// Username or Email
        /// </summary>
        public string User
        {
            get => _User;
            set => SetProperty(ref _User, value);
        }

        private string _PlayerName;
        public string PlayerName
        {
            get => _PlayerName;
            set => SetProperty(ref _PlayerName, value);
        }

        private string _ProgressDescriptionText;
        public string ProgressDescriptionText
        {
            get => _ProgressDescriptionText;
            set => SetProperty(ref _ProgressDescriptionText, value);
        }

        private int _LaunchingProgress;
        public int LaunchingProgress
        {
            get => _LaunchingProgress;
            set => SetProperty(ref _LaunchingProgress, value);
        }

        #region UserDialog
        private string _UserDialogPrimaryButtonContent;
        public string UserDialogPrimaryButtonContent
        {
            get => _UserDialogPrimaryButtonContent;
            set => SetProperty(ref _UserDialogPrimaryButtonContent, value);
        }

        private Visibility _UserDialogPasswordBoxVisibility;
        public Visibility UserDialogPasswordBoxVisibility
        {
            get => _UserDialogPasswordBoxVisibility;
            set => SetProperty(ref _UserDialogPasswordBoxVisibility, value);
        }

        private bool _UserDialogWorking;
        public bool UserDialogWorking
        {
            get => _UserDialogWorking;
            set
            {
                SetProperty(ref _UserDialogWorking, value);
                OnPropertyChanged(nameof(UserDialogNotWorking));
                OnPropertyChanged(nameof(UserDialogProgressBarVisbility));
            }
        }

        public bool UserDialogNotWorking
        {
            get => !UserDialogWorking;
        }

        public Visibility UserDialogProgressBarVisbility
        {
            get => UserDialogWorking ? Visibility.Visible : Visibility.Collapsed;
        }

        private string _UserDialogResultString;
        public string UserDialogResultString
        {
            get => _UserDialogResultString;
            set => SetProperty(ref _UserDialogResultString, value);
        }

        private bool _UserDialogNotLock;
        public bool UserDialogNotLock
        {
            get => _UserDialogNotLock;
            set
            {
                SetProperty(ref _UserDialogNotLock, value);
            }
        }

        public UserDialogState UserDialogCurrentState { get; set; }

        public void SetupUserDialog(UserDialogState state)
        {
            UserDialogCurrentState = state;
            switch (state)
            {
                case UserDialogState.Input:
                    UserDialogNotLock = true;
                    UserDialogPrimaryButtonContent = "登陆";
                    UserDialogPasswordBoxVisibility = Visibility.Visible;
                    UserDialogWorking = false;
                    break;
                case UserDialogState.Logging:
                    UserDialogNotLock = false;
                    UserDialogPrimaryButtonContent = "登陆";
                    UserDialogPasswordBoxVisibility = Visibility.Visible;
                    UserDialogWorking = true;
                    break;
                case UserDialogState.LoggedIn:
                    UserDialogNotLock = false;
                    UserDialogPrimaryButtonContent = "登出";
                    UserDialogPasswordBoxVisibility = Visibility.Collapsed;
                    UserDialogWorking = false;
                    break;
                case UserDialogState.Offline:
                    UserDialogNotLock = true;
                    UserDialogPrimaryButtonContent = "确定";
                    UserDialogPasswordBoxVisibility = Visibility.Collapsed;
                    UserDialogWorking = false;
                    break;
            }
        }

        #endregion
    }

    public enum UserDialogState
    {
        Input,
        Logging,
        LoggedIn,
        Offline,
    }
}
