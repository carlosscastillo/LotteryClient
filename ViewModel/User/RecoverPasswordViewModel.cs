using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.User;
using Lottery.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.User
{
    public class RecoverPasswordViewModel : BaseViewModel
    {
        private readonly Dictionary<string, string> _errorMap;
        private string _verifiedEmail;
        
        private bool _isEmailStepVisible;
        public bool IsEmailStepVisible { get => _isEmailStepVisible; set => SetProperty(ref _isEmailStepVisible, value); }

        private bool _isCodeStepVisible;
        public bool IsCodeStepVisible { get => _isCodeStepVisible; set => SetProperty(ref _isCodeStepVisible, value); }

        private bool _isNewPasswordStepVisible;
        public bool IsNewPasswordStepVisible { get => _isNewPasswordStepVisible; set => SetProperty(ref _isNewPasswordStepVisible, value); }
        
        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _verificationCode;
        public string VerificationCode { get => _verificationCode; set => SetProperty(ref _verificationCode, value); }
        
        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _confirmNewPassword;
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set => SetProperty(ref _confirmNewPassword, value);
        }        
        
        private bool _isNewPasswordVisible;
        public bool IsNewPasswordVisible
        {
            get => _isNewPasswordVisible;
            set => SetProperty(ref _isNewPasswordVisible, value);
        }

        private bool _isConfirmNewPasswordVisible;
        public bool IsConfirmNewPasswordVisible
        {
            get => _isConfirmNewPasswordVisible;
            set => SetProperty(ref _isConfirmNewPasswordVisible, value);
        }
        
        public ICommand SendCodeCommand { get; }
        public ICommand VerifyCodeCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand BackCommand { get; }

        public RecoverPasswordViewModel()
        {
            _errorMap = new Dictionary<string, string>
            {
                { "USER_NOT_FOUND", Lang.LoginGenericError },
                { "EMAIL_SEND_FAILED", Lang.RegisterEmailSendFailed },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "INVALID_CODE", Lang.RegisterCodeExpiredOrIncorrect }
            };
            
            IsNewPasswordVisible = false;
            IsConfirmNewPasswordVisible = false;

            IsEmailStepVisible = true;
            IsCodeStepVisible = false;
            IsNewPasswordStepVisible = false;

            SendCodeCommand = new RelayCommand(async () => await SendCode());
            VerifyCodeCommand = new RelayCommand(async () => await VerifyCode());
            FinishCommand = new RelayCommand(async () => await Finish());
            BackCommand = new RelayCommand(GoBackToEmail);
        }
        
        public void UpdateNewPassword(string password)
        {
            NewPassword = password;
        }

        public void UpdateConfirmNewPassword(string password)
        {
            ConfirmNewPassword = password;
        }
        
        private async Task SendCode()
        {           
            var validator = new UserValidator().ValidateEmailOnly();
            var validationResult = validator.Validate(new UserDto { Email = Email });

            if (!validationResult.IsValid)
            {                
                ShowError(validationResult.Errors.First().ErrorMessage, Lang.LoginValidationTitle, MessageBoxImage.Warning);
            }
            else
            {             
                await ExecuteRequest(async () =>
                {
                    bool result = await ServiceProxy.Instance.Client.RecoverPasswordRequestAsync(Email);

                    if (result)
                    {
                        ShowSuccess(Lang.RegisterVerificationCodeSent);
                        IsEmailStepVisible = false;
                        IsCodeStepVisible = true;
                    }
                    else
                    {                     
                        ShowError(Lang.RegisterEmailSendFailed, Lang.GlobalMessageBoxTitleError);
                    }
                }, _errorMap);
            }
        }

        private async Task VerifyCode()
        {            
            var result = new CodeValidator().Validate(VerificationCode);
            if (!result.IsValid)
            {
                ShowError(result.Errors.First().ErrorMessage, Lang.RegisterInvalidCodeTitle, MessageBoxImage.Warning);
            }
            else
            {
                await ExecuteRequest(async () =>
                {
                    var client = ServiceProxy.Instance.Client;
                                        
                    bool verified = await client.VerifyCodeAsync(Email, VerificationCode);

                    if (verified)
                    {
                        _verifiedEmail = Email;

                        IsCodeStepVisible = false;
                        IsNewPasswordStepVisible = true;
                    }
                    else
                    {
                        ShowError(Lang.RegisterCodeExpiredOrIncorrect, Lang.RegisterVerificationFailedTitle, MessageBoxImage.Warning);                        
                    }
                }, _errorMap);
            }
        }

        private async Task Finish()
        {
            var userValidator = new UserValidator().ValidatePasswordOnly();            
            var userValResult = userValidator.Validate(new UserDto { Password = NewPassword });

            if (!userValResult.IsValid)
            {
                ShowError(userValResult.Errors.First().ErrorMessage, Lang.LoginValidationTitle, MessageBoxImage.Warning);
            }
            else if (NewPassword != ConfirmNewPassword)
            {
                ShowError(Lang.RegisterPasswordsDoNotMatch, Lang.LoginValidationTitle, MessageBoxImage.Warning);
            }
            else
            {
                await ExecuteRequest(async () =>
                {                    
                    bool ok = await ServiceProxy.Instance.Client.RecoverPasswordAsync(_verifiedEmail, NewPassword);

                    if (ok)
                    {
                        await ServiceProxy.Instance.Client.ConsumeVerificationCodeAsync(_verifiedEmail);
                        ShowSuccess(Lang.GlobalMessageBoxTitleSuccess);
                        
                        Application.Current.Windows.OfType<Window>()
                            .SingleOrDefault(w => w.DataContext == this)?.Close();
                    }
                    else
                    {
                        ShowError(Lang.GlobalExceptionInternalServerError, Lang.GlobalMessageBoxTitleError);
                    }
                }, _errorMap);
            }
        }
        private void GoBackToEmail()
        {
            IsCodeStepVisible = false;
            IsEmailStepVisible = true;            
            VerificationCode = string.Empty;
        }
    }
}