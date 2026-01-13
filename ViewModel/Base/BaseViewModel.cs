using Contracts.Faults;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.User; // Asegúrate de que LoginView esté aquí, o cambia a Lottery.View.Login
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.ViewModel.Base
{
    public abstract class BaseViewModel : ObservableObject
    {
        // Semáforo compartido para evitar spam de mensajes
        private static bool _isHandlingDisconnection = false;

        public BaseViewModel()
        {
            ServiceProxy.Instance.ConnectionLost -= HandleConnectionLost;
            ServiceProxy.Instance.ConnectionLost += HandleConnectionLost;
        }

        // 1. Entrada por EVENTO (Automático WCF)
        private void HandleConnectionLost()
        {
            // Redirigimos todo al método centralizado
            HandleConnectionError();
        }

        // 2. Entrada por EXCEPCIÓN (Acción del usuario) y Lógica Central
        private void HandleConnectionError()
        {
            // Usamos el Dispatcher para asegurar que corra en el hilo UI y bloquear concurrencia
            Application.Current.Dispatcher.Invoke(() =>
            {
                // --- PROTECCIÓN CENTRALIZADA ---
                // Si ya estamos manejando una desconexión (sea por evento o excepción), nos salimos.
                if (_isHandlingDisconnection)
                {
                    return;
                }

                _isHandlingDisconnection = true;

                try
                {
                    // 1. Mostrar mensaje (Solo una vez)
                    CustomMessageBox.Show(
                        Lang.GlobalExceptionConnectionLostMessage,
                        Lang.GlobalMessageBoxTitleError,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    // 2. Matar el proxy viejo para permitir reconexión limpia
                    try
                    {
                        ServiceProxy.Instance.CloseSafe();
                    }
                    catch { }

                    // 3. Redirigir al Login
                    NavigateToLoginOrExit();
                }
                finally
                {
                    // Liberamos la bandera después de unos segundos
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
            // Nota: Ya estamos dentro del Dispatcher gracias a HandleConnectionError

            var currentWindow = Application.Current.MainWindow;

            // Evitar recargas si ya estamos en Login
            if (currentWindow != null && currentWindow.GetType().Name == "LoginView")
            {
                return;
            }

            // 1. Instanciar Login
            LoginView loginScreen = new LoginView();

            // 2. Mostrar Login primero
            loginScreen.Show();

            // 3. Cerrar las demás ventanas
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

            // 4. Establecer como ventana principal
            Application.Current.MainWindow = loginScreen;
        }

        protected async Task ExecuteRequest(Func<Task> action, Dictionary<string, string> errorMap = null)
        {
            try
            {
                // 1. VALIDACIÓN PREVENTIVA
                var client = ServiceProxy.Instance.Client as ICommunicationObject;

                if (client == null ||
                    client.State == CommunicationState.Faulted ||
                    client.State == CommunicationState.Closed)
                {
                    HandleConnectionError();
                    return;
                }

                // 2. INTENTO DE EJECUCIÓN
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

        private void HandleServiceFault(FaultException<ServiceFault> fault, Dictionary<string, string> errorMap)
        {
            string message;
            string errorCode = fault.Detail.ErrorCode;
            MessageBoxImage icon = MessageBoxImage.Warning;

            if (errorMap != null && errorMap.TryGetValue(errorCode, out var customMessage))
            {
                message = customMessage;
            }
            else
            {
                message = string.Format(Lang.GlobalExceptionServerError, errorCode, fault.Detail.Message);
            }

            if (errorCode == "USER_OFFLINE" || errorCode == "SERVER_ERROR")
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