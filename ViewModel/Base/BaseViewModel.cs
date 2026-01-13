using Contracts.Faults;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.User;
using System;
using System.Collections.Generic;
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
            HandleConnectionError();
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
                    catch { }

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
            var currentWindow = Application.Current.MainWindow;

            if (currentWindow != null && currentWindow.GetType().Name == "LoginView")
            {
                return;
            }

            LoginView loginScreen = new LoginView();

            loginScreen.Show();

            var windowsToClose = new List<Window>();
            foreach (Window win in Application.Current.Windows)
            {
                if (win != loginScreen)
                {
                    windowsToClose.Add(win);
                }
            }

            foreach (var win in windowsToClose)
            {
                win.Close();
            }

            Application.Current.MainWindow = loginScreen;
        }

        protected async Task ExecuteRequest(Func<Task> action, Dictionary<string, string> errorMap = null)
        {
            try
            {
                var client = ServiceProxy.Instance.Client as ICommunicationObject;

                if (client == null ||
                    client.State == CommunicationState.Faulted ||
                    client.State == CommunicationState.Closed)
                {
                    HandleConnectionError();
                    return;
                }

                await action();
            }
            catch (FaultException<ServiceFault> ex)
            {
                HandleServiceFault(ex, errorMap);
            }
            catch (CommunicationObjectAbortedException)
            {
                HandleConnectionError();
            }
            catch (CommunicationObjectFaultedException)
            {
                HandleConnectionError();
            }
            catch (CommunicationException)
            {
                HandleConnectionError();
            }
            catch (TimeoutException)
            {
                HandleConnectionError();
            }
            catch (Exception ex)
            {
                ShowError(string.Format(Lang.GlobalMessageBoxUnexpectedError, ex.Message), Lang.GlobalMessageBoxTitleError);
            }
        }

        private void HandleServiceFault(FaultException<ServiceFault> fault, Dictionary<string, string> viewErrorMap)
        {
            string message = null;
            string errorCode = fault.Detail?.ErrorCode;
            string serverMessage = fault.Detail?.Message;

            var globalErrorMap = new Dictionary<string, string>
            {
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "INTERNAL_SERVER_ERROR", Lang.GlobalExceptionInternalServerError },
                { "GLOBAL_TIMEOUT", Lang.GlobalExceptionConnectionLostMessage },
                { "GAME_NOT_ENOUGH_PLAYERS", Lang.LobbyStartGameNotEnoughPlayers },
                { "GLOBAL_BAD_REQUEST", Lang.GlobalMessageBoxUnexpectedError }
            };

            if (!string.IsNullOrEmpty(errorCode) && viewErrorMap != null && viewErrorMap.ContainsKey(errorCode))
            {
                message = viewErrorMap[errorCode];
            }
            else if (!string.IsNullOrEmpty(errorCode) && globalErrorMap.ContainsKey(errorCode))
            {
                message = globalErrorMap[errorCode];
            }
            else
            {
                message = !string.IsNullOrEmpty(serverMessage)
                    ? serverMessage
                    : string.Format(Lang.GlobalExceptionServerError, errorCode ?? "UNKNOWN", "Error");
            }

            MessageBoxImage icon = MessageBoxImage.Warning;
            if (errorCode == "DB_ERROR" || errorCode == "INTERNAL_SERVER_ERROR" || errorCode == "USER_OFFLINE")
            {
                icon = MessageBoxImage.Error;
            }

            ShowError(message, Lang.GlobalMessageBoxTitleError, icon);
        }

        protected void ShowSuccess(string message)
        {
            CustomMessageBox.Show(message, Lang.GlobalMessageBoxTitleSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
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