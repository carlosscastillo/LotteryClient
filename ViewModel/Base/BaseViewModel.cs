using Contracts.Faults;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.ViewModel.Base
{
    public abstract class BaseViewModel : ObservableObject
    {
        private static bool _isHandlingDisconnection = false;

        public BaseViewModel()
        {
            ServiceProxy.Instance.ConnectionLost -= HandleConnectionLost;
            ServiceProxy.Instance.ConnectionLost += HandleConnectionLost;
        }

        private void HandleConnectionLost()
        {
            if (!ServiceProxy.Instance.IsOfflineMode)
            {
                HandleConnectionError();
            }
        }

        private void HandleConnectionError()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_isHandlingDisconnection)
                {
                    return;
                }

                _isHandlingDisconnection = true;

                try
                {
                    CustomMessageBox.Show(
                        Lang.GlobalExceptionConnectionLostMessage,
                        Lang.GlobalMessageBoxTitleError,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    try
                    {
                        ServiceProxy.Instance.CloseSafe();
                    }
                    catch (Exception)
                    {
                        /* Empty catch handled per proxy logic */
                    }

                    NavigateToLoginOrExit();
                }
                finally
                {
                    ResetDisconnectionFlagAsync();
                }
            });
        }

        private async void ResetDisconnectionFlagAsync()
        {
            await Task.Delay(2000);
            _isHandlingDisconnection = false;
        }

        private void NavigateToLoginOrExit()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window currentWindow = Application.Current.MainWindow;

                if (currentWindow != null && currentWindow.GetType() == typeof(LoginView))
                {
                    return;
                }

                LoginView loginScreen = new LoginView();
                loginScreen.Show();

                Application.Current.MainWindow = loginScreen;

                List<Window> windowsToClose = Application.Current.Windows.Cast<Window>().ToList();

                foreach (Window win in windowsToClose)
                {
                    if (win != loginScreen)
                    {
                        win.Close();
                    }
                }
            });
        }

        protected async Task ExecuteRequest(Func<Task> action, Dictionary<string, string> errorMap = null)
        {
            if (ServiceProxy.Instance.IsOfflineMode)
            {
                ServiceProxy.Instance.EnqueueAction(action);
                return;
            }

            try
            {
                ILotteryService client = ServiceProxy.Instance.Client;
                ICommunicationObject channel = client as ICommunicationObject;

                if (channel == null ||
                    channel.State == CommunicationState.Faulted ||
                    channel.State == CommunicationState.Closed)
                {
                    ServiceProxy.Instance.EnqueueAction(action);
                    return;
                }

                await action();
            }
            catch (Exception ex)
            {
                if (ex is FaultException<ServiceFault> faultEx)
                {
                    HandleServiceFault(faultEx, errorMap);
                    return;
                }

                if (ex is CommunicationException ||
                    ex is TimeoutException ||
                    ex is EndpointNotFoundException ||
                    ex is ObjectDisposedException)
                {
                    ServiceProxy.Instance.EnqueueAction(action);
                    return;
                }

                string errorMessage = string.Format(Lang.GlobalMessageBoxUnexpectedError, ex.Message);
                ShowError(errorMessage, Lang.GlobalMessageBoxTitleError);
            }
        }

        private void HandleServiceFault(FaultException<ServiceFault> fault, Dictionary<string, string> viewErrorMap)
        {
            string message = null;
            string errorCode = fault.Detail?.ErrorCode;
            string serverMessage = fault.Detail?.Message;

            Dictionary<string, string> globalErrorMap = new Dictionary<string, string>
            {
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "INTERNAL_SERVER_ERROR", Lang.GlobalExceptionInternalServerError },
                { "GLOBAL_TIMEOUT", Lang.GlobalExceptionConnectionLostMessage },
                { "GAME_NOT_ENOUGH_PLAYERS", Lang.LobbyStartGameNotEnoughPlayers },
                { "GLOBAL_BAD_REQUEST", Lang.GlobalMessageBoxUnexpectedError }
            };

            bool hasCode = !string.IsNullOrEmpty(errorCode);

            if (hasCode && viewErrorMap != null && viewErrorMap.ContainsKey(errorCode))
            {
                message = viewErrorMap[errorCode];
            }
            else if (hasCode && globalErrorMap.ContainsKey(errorCode))
            {
                message = globalErrorMap[errorCode];
            }
            else
            {
                if (!string.IsNullOrEmpty(serverMessage))
                {
                    message = serverMessage;
                }
                else
                {
                    message = string.Format(Lang.GlobalExceptionServerError, errorCode ?? "UNKNOWN", "Error");
                }
            }

            MessageBoxImage icon = MessageBoxImage.Warning;

            bool isFatalError = errorCode == "DB_ERROR" ||
                               errorCode == "INTERNAL_SERVER_ERROR" ||
                               errorCode == "USER_OFFLINE";

            if (isFatalError)
            {
                icon = MessageBoxImage.Error;
            }

            ShowError(message, Lang.GlobalMessageBoxTitleError, icon);
        }

        protected void ShowSuccess(string message)
        {
            CustomMessageBox.Show(
                message,
                Lang.GlobalMessageBoxTitleSuccess,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        protected void ShowError(string message)
        {
            ShowError(message, Lang.GlobalMessageBoxTitleError, MessageBoxImage.Error);
        }

        protected void ShowError(string message, string title, MessageBoxImage icon = MessageBoxImage.Error)
        {
            CustomMessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
    }
}