using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveTwitter.Model;
using ReactiveUI;
using Tweetinvi;
using Tweetinvi.Models;

namespace ReactiveTwitter.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private string _userName;
        private string _userPicture;
        private IEnumerable<ITweet> _tweets;
        private bool _isGettingPin;
        private string _pinValue;

        public MainViewModel()
        {
            var authObs = Observable.Start(TwitterAuthenticator.AuthenticateUser);
            authObs.Subscribe(logged =>
            {
                if (!logged)
                    IsGettingPin = true;
                else
                    SetAuthenticatedUser(User.GetAuthenticatedUser());
            });
            ConfirmPinCommand = ReactiveCommand.Create(DoConfirmPin);
            CancelPinCommand = ReactiveCommand.Create(DoCancelPin);
        }

        private void DoCancelPin()
        {
            IsGettingPin = false;
        }

        private void DoConfirmPin()
        {
            IsGettingPin = false;
            TwitterAuthenticator.CreateAndSetCredentials(PinValue);
            var user = User.GetAuthenticatedUser();
            SetAuthenticatedUser(user);
        }

        private void SetAuthenticatedUser(IAuthenticatedUser u)
        {
            UserName = u.Name;
            UserPicture = u.ProfileImageUrl400x400;
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(15)).Subscribe(_ =>
                Tweets = u.GetHomeTimeline(100));
        }

        public string UserName
        {
            get { return _userName; }
            set { this.RaiseAndSetIfChanged(ref _userName, value); }
        }

        public string UserPicture
        {
            get { return _userPicture; }
            set { this.RaiseAndSetIfChanged(ref _userPicture, value); }
        }

        public string PinValue
        {
            get { return _pinValue; }
            set { this.RaiseAndSetIfChanged(ref _pinValue, value); }
        }
        public bool IsGettingPin
        {
            get { return _isGettingPin; }
            private set { this.RaiseAndSetIfChanged(ref _isGettingPin, value); }
        }

        public IEnumerable<ITweet> Tweets
        {
            get { return _tweets; }
            private set { this.RaiseAndSetIfChanged(ref _tweets, value); }
        }

        public ReactiveCommand ConfirmPinCommand { get; }
        public ReactiveCommand CancelPinCommand { get; }
    }
}
