using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CotpsBot.Helpers;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CotpsBot.ViewModels
{
    public class BaseViewModel: INotifyPropertyChanged
    {
        #region Fields

        bool isBusy = false;
        bool isRefreshing = false;
        bool isNotConnected = false;

        #endregion

        #region Constructor

        public BaseViewModel()
        {
            // this.SwitchCommand = new Command(this.SwitchClicked);
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            IsNotConnected = Connectivity.NetworkAccess != NetworkAccess.Internet;
        }

        ~BaseViewModel()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }
        #endregion
        
        #region Properties
        public bool IsNotConnected {
            get { return isNotConnected; }
            set { SetProperty(ref isNotConnected, value); }
        }
        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set { SetProperty(ref isRefreshing, value); }
        }
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }
        #endregion

        #region Commands

        // public Command SwitchCommand { get; set; }

        #endregion
        
        #region Methods
        
        // private void SwitchClicked(object obj)
        // {
        //     Debug.WriteLine("Switch was clicked!");
        // }
        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var _prev = IsNotConnected;
            IsNotConnected = e.NetworkAccess != NetworkAccess.Internet;
            
            if (e.NetworkAccess != NetworkAccess.Internet && !_prev)
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("Lost Internet Connection. Switching to Offline mode"));

            if (e.NetworkAccess == NetworkAccess.Internet && _prev)
                await App.Current.MainPage.DisplaySnackBarAsync(new SuccessSnackBar("Internet connection established. Switching to online mode."));

        }
        
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
        
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}