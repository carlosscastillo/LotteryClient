﻿using Lottery.LotteryServiceReference;
using Lottery.View;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        private ILotteryService _serviceClient;

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set => SetProperty(ref _isLoggingIn, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand SignUpCommand { get; }

        public LoginViewModel()
        {
            _serviceClient = new LotteryServiceClient();

            LoginCommand = new RelayCommand<Window>(async (window) => await Login(window));
            SignUpCommand = new RelayCommand<Window>(ExecuteSignUp);
        }

        public async Task Login(Window window)
        {
            if (IsLoggingIn) return;

            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {
                UserSessionDTO user = await _serviceClient.LoginUserAsync(Username, Password);

                SessionManager.Login(user);
                SessionManager.ServiceClient = _serviceClient;

                MainMenuView mainMenuView = new MainMenuView();
                mainMenuView.Show();
                window.Close();
            }
            catch (FaultException<ServiceFault> ex)
            {
                ErrorMessage = ex.Detail.Message;

                MessageBox.Show(ex.Detail.Message, "Error de Inicio de Sesión", MessageBoxButton.OK, MessageBoxImage.Warning);

                AbortAndRecreateClient();
            }
            catch (FaultException ex)
            {
                ErrorMessage = "Error de conexión. No se pudo contactar al servidor.";
                MessageBox.Show(ErrorMessage, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ha ocurrido un error inesperado.";
                MessageBox.Show($"{ErrorMessage}\nDetalle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AbortAndRecreateClient();
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        // Para que el usuario pueda intentar iniciar sesión de nuevo
        private void AbortAndRecreateClient()
        {
            if (_serviceClient != null)
            {
                var clientChannel = _serviceClient as ICommunicationObject;
                if (clientChannel.State == CommunicationState.Faulted)
                {
                    clientChannel.Abort();
                }
                else
                {
                    try { clientChannel.Close(); }
                    catch { clientChannel.Abort(); }
                }
            }

            _serviceClient = new LotteryServiceClient();
        }

        private void ExecuteSignUp(Window loginWindow)
        {
            UserRegisterView registerView = new UserRegisterView();
            registerView.Show();

            loginWindow?.Close();
        }
    }
}