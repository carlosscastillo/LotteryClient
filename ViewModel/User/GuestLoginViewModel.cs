using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.User
{
    public class GuestLoginViewModel : BaseViewModel
    {
        private string _nickname;
        private bool _isBusy;
        private string _errorMessage;
        private readonly Dictionary<string, string> _errorMap;

        public event Action RequestClose;

        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginGuestCommand { get; }

        public GuestLoginViewModel()
        {
            _errorMap = new Dictionary<string, string>
            {
                { "AUTH_BAD_REQUEST", Lang.GuestLoginInvalidNickname },
                { "AUTH_INVALID_LENGTH", Lang.GuestLoginInvalidLength },
                { "AUTH_EMPTY_NICKNAME", Lang.GuestLoginEmptyNickname },
                { "AUTH_INVALID_FORMAT", Lang.GuestLoginInvalidFormat },
                { "AUTH_DB_ERROR", Lang.GlobalExceptionInternalServerError },
                { "AUTH_INTERNAL_500", Lang.GlobalExceptionInternalServerError }
            };

            LoginGuestCommand = new RelayCommand(async () => await LoginGuest());
        }

        private async Task LoginGuest()
        {
            if (string.IsNullOrWhiteSpace(Nickname))
            {
                ErrorMessage = Lang.GuestLoginEmptyNickname;
                ShowError(Lang.GuestLoginEmptyNickname, Lang.GlobalMessageBoxTitleWarning, MessageBoxImage.Warning);
            }
            else
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                await ExecuteRequest(async () =>
                {
                    var client = ServiceProxy.Instance.Client;
                    UserDto guestUser = await client.LoginGuestAsync(Nickname);

                    if (guestUser != null)
                    {
                        SessionManager.Login(guestUser);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MainMenuView mainMenu = new MainMenuView();
                            mainMenu.Show();

                            RequestClose?.Invoke();
                        });
                    }
                    else
                    {
                        ErrorMessage = Lang.GuestLoginGenericError;
                        ShowError(Lang.GuestLoginGenericError);
                    }
                }, _errorMap);

                IsBusy = false;
            }
        }
    }
}