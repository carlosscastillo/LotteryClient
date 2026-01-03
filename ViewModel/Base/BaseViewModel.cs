using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.ViewModel.Base
{
    public abstract class BaseViewModel : ObservableObject
    {
        protected async Task ExecuteRequest(Func<Task> action, Dictionary<string, string> errorMap = null)
        {
            try
            {
                await action();
            }
            catch (FaultException<ServiceFault> ex)
            {
                HandleServiceFault(ex, errorMap);
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