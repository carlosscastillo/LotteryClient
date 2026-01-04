using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.ViewModel.User
{
    public class CustomizeProfileViewModel : BaseViewModel
    {
        private readonly Dictionary<string, string> _errorMap;
        private UserDto _currentUserFull;
        private SocialMediaDto _socialMedia;
        private AvatarItemViewModel _selectedAvatar;

        private int _idAvatar;
        public int IdAvatar
        {
            get => _idAvatar;
            set => SetProperty(ref _idAvatar, value);
        }

        private int _idUser;
        public int IdUser
        {
            get => _idUser;
            set => SetProperty(ref _idUser, value);
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _paternalLastName;
        public string PaternalLastName
        {
            get => _paternalLastName;
            set => SetProperty(ref _paternalLastName, value);
        }

        private string _maternalLastName;
        public string MaternalLastName
        {
            get => _maternalLastName;
            set => SetProperty(ref _maternalLastName, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _twitter;
        public string Twitter
        {
            get => _twitter;
            set => SetProperty(ref _twitter, value);
        }

        private string _facebook;
        public string Facebook
        {
            get => _facebook;
            set => SetProperty(ref _facebook, value);
        }

        private string _instagram;
        public string Instagram
        {
            get => _instagram;
            set => SetProperty(ref _instagram, value);
        }

        private string _tikTok;
        public string TikTok
        {
            get => _tikTok;
            set => SetProperty(ref _tikTok, value);
        }

        private string _newEmail;
        public string NewEmail
        {
            get => _newEmail;
            set => SetProperty(ref _newEmail, value);
        }

        private string _verificationCode;
        public string VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value);
        }

        private string _avatarUrl;
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        private string _currentPassword;
        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
        }

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

        private bool _isCurrentPasswordVisible;
        public bool IsCurrentPasswordVisible
        {
            get => _isCurrentPasswordVisible;
            set => SetProperty(ref _isCurrentPasswordVisible, value);
        }

        private bool _isNewPasswordVisibleEye;
        public bool IsNewPasswordVisibleEye
        {
            get => _isNewPasswordVisibleEye;
            set => SetProperty(ref _isNewPasswordVisibleEye, value);
        }

        private bool _isConfirmNewPasswordVisible;
        public bool IsConfirmNewPasswordVisible
        {
            get => _isConfirmNewPasswordVisible;
            set => SetProperty(ref _isConfirmNewPasswordVisible, value);
        }

        private ObservableCollection<AvatarItemViewModel> _avatars;
        public ObservableCollection<AvatarItemViewModel> Avatars
        {
            get => _avatars;
            set => SetProperty(ref _avatars, value);
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                SetProperty(ref _isEditing, value);
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(EditButtonVisibility));
                OnPropertyChanged(nameof(SaveCancelVisibility));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        private bool _isChangeEmailVisible;
        public bool IsChangeEmailVisible
        {
            get => _isChangeEmailVisible;
            set => SetProperty(ref _isChangeEmailVisible, value);
        }

        private bool _isVerifyEmailVisible;
        public bool IsVerifyEmailVisible
        {
            get => _isVerifyEmailVisible;
            set => SetProperty(ref _isVerifyEmailVisible, value);
        }

        private bool _isEmailVerifiedVisible;
        public bool IsEmailVerifiedVisible
        {
            get => _isEmailVerifiedVisible;
            set => SetProperty(ref _isEmailVerifiedVisible, value);
        }

        private bool _isChangePasswordVisible;
        public bool IsChangePasswordVisible
        {
            get => _isChangePasswordVisible;
            set => SetProperty(ref _isChangePasswordVisible, value);
        }

        private bool _isNewPasswordVisible;
        public bool IsNewPasswordVisible
        {
            get => _isNewPasswordVisible;
            set => SetProperty(ref _isNewPasswordVisible, value);
        }

        private bool _isAvatarOverlayVisible;
        public bool IsAvatarOverlayVisible
        {
            get => _isAvatarOverlayVisible;
            set => SetProperty(ref _isAvatarOverlayVisible, value);
        }

        public bool IsAvatarSelected => _selectedAvatar != null && _selectedAvatar.AvatarId != this.IdAvatar;
        public bool IsReadOnly => !IsEditing;
        public Visibility EditButtonVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        public Visibility SaveCancelVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;

        public RelayCommand EditCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand SendVerificationCodeCommand { get; }
        public RelayCommand VerifyEmailCodeCommand { get; }
        public RelayCommand BackToChangeEmailCommand { get; }
        public RelayCommand OpenChangeEmailCommand { get; }
        public RelayCommand CloseOverlayCommand { get; }
        public RelayCommand OpenChangePasswordCommand { get; }
        public RelayCommand VerifyPasswordCommand { get; }
        public RelayCommand SaveNewPasswordCommand { get; }
        public RelayCommand BackToChangePasswordCommand { get; }
        public RelayCommand OpenChangeAvatarCommand { get; }
        public RelayCommand<AvatarItemViewModel> SelectAvatarCommand { get; }
        public RelayCommand AcceptAvatarChangeCommand { get; }
        public RelayCommand CloseAvatarOverlayCommand { get; }
        public RelayCommand<object> GoBackToMenuCommand { get; }

        public CustomizeProfileViewModel()
        {
            _errorMap = new Dictionary<string, string>
            {
                { "USER_DUPLICATE", Lang.RegisterUserDuplicate },
                { "AUTH_INVALID_CREDENTIALS", Lang.LoginInvalidCredentials },
                { "VERIFY_ERROR", Lang.RegisterVerifyError },
                { "VERIFY_EMAIL_SEND_FAILED", Lang.RegisterEmailSendFailed },
                { "USER_NOT_FOUND", Lang.LoginGenericError }
            };

            EditCommand = new RelayCommand(EditProfile);
            SaveChangesCommand = new RelayCommand(async () => await SaveChanges());
            CancelCommand = new RelayCommand(CancelEdit);

            OpenChangeEmailCommand = new RelayCommand(OpenChangeEmail);
            SendVerificationCodeCommand = new RelayCommand(async () => await SendVerifyEmail());
            VerifyEmailCodeCommand = new RelayCommand(async () => await VerifyEmailCode());
            BackToChangeEmailCommand = new RelayCommand(BackToEditEmail);
            CloseOverlayCommand = new RelayCommand(CloseOverlay);

            OpenChangePasswordCommand = new RelayCommand(OpenChangePassword);
            VerifyPasswordCommand = new RelayCommand(async () => await VerifyCurrentPassword());
            SaveNewPasswordCommand = new RelayCommand(async () => await SaveNewPassword());
            BackToChangePasswordCommand = new RelayCommand(BackToChangePassword);

            OpenChangeAvatarCommand = new RelayCommand(OpenChangeAvatar);
            SelectAvatarCommand = new RelayCommand<AvatarItemViewModel>(SelectAvatar);
            AcceptAvatarChangeCommand = new RelayCommand(AcceptAvatarChange, CanAcceptAvatarChange);
            CloseAvatarOverlayCommand = new RelayCommand(CloseAvatarOverlay);

            GoBackToMenuCommand = new RelayCommand<object>(GoBackToMenu);

            IsEditing = false;
            IsOverlayVisible = false;
            IsChangeEmailVisible = false;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;
            IsChangePasswordVisible = false;
            IsNewPasswordVisible = false;
            IsAvatarOverlayVisible = false;

            _socialMedia = new SocialMediaDto();

            _ = LoadFullUserData();
        }

        private async Task LoadFullUserData()
        {
            if (SessionManager.CurrentUser != null)
            {
                await ExecuteRequest(async () =>
                {
                    int userId = SessionManager.CurrentUser.UserId;
                    var fullUser = await ServiceProxy.Instance.Client.GetUserProfileAsync(userId);
                    var socialData = await ServiceProxy.Instance.Client.GetSocialMediaAsync(userId);

                    if (fullUser != null)
                    {
                        _currentUserFull = fullUser;
                        IdUser = _currentUserFull.UserId;
                        MapUserFromDTO(_currentUserFull);
                    }

                    if (socialData != null)
                    {
                        _socialMedia = socialData;
                        MapSocialMediaFromDTO(_socialMedia);
                    }
                    else
                    {
                        _socialMedia = new SocialMediaDto { IdUser = userId };
                    }
                }, _errorMap);
            }
        }

        private async Task SaveChanges()
        {
            if (_currentUserFull != null && _socialMedia != null)
            {
                _currentUserFull.UserId = IdUser;
                _currentUserFull.Nickname = Nickname;
                _currentUserFull.FirstName = FirstName;
                _currentUserFull.PaternalLastName = PaternalLastName;
                _currentUserFull.MaternalLastName = MaternalLastName;
                _currentUserFull.AvatarId = IdAvatar;

                _socialMedia.IdUser = IdUser;
                _socialMedia.Twitter = Twitter;
                _socialMedia.Facebook = Facebook;
                _socialMedia.Instagram = Instagram;
                _socialMedia.TikTok = TikTok;

                var userValidator = new UserValidator().ValidateProfileUpdate();
                var userValResult = userValidator.Validate(_currentUserFull);

                if (!userValResult.IsValid)
                {
                    string errores = string.Join("\n", userValResult.Errors.Select(e => e.ErrorMessage));
                    ShowError(errores, Lang.LoginValidationTitle, MessageBoxImage.Warning);
                }
                else
                {
                    var socialValidator = new SocialMediaValidator().ValidateAll();
                    var socialValResult = socialValidator.Validate(_socialMedia);

                    if (!socialValResult.IsValid)
                    {
                        string errores = string.Join("\n", socialValResult.Errors.Select(e => e.ErrorMessage));
                        ShowError(errores, Lang.LoginValidationTitle, MessageBoxImage.Warning);
                    }
                    else
                    {
                        IsBusy = true;
                        await ExecuteRequest(async () =>
                        {
                            var (successUser, msgUser) = await ServiceProxy.Instance.Client.UpdateProfileAsync(_currentUserFull.UserId, _currentUserFull);

                            if (!successUser)
                            {
                                ShowError($"{Lang.GlobalMessageBoxTitleError}: {msgUser}", Lang.GlobalMessageBoxTitleError);
                            }
                            else
                            {
                                bool successSocial = await ServiceProxy.Instance.Client.SaveOrUpdateSocialMediaAsync(_socialMedia);

                                if (successSocial)
                                {
                                    ShowSuccess(Lang.GlobalMessageBoxTitleSuccess);
                                    IsEditing = false;
                                    SessionManager.CurrentUser.Nickname = Nickname;
                                }
                                else
                                {
                                    ShowError(Lang.GlobalMessageBoxTitleWarning, Lang.GlobalMessageBoxTitleWarning, MessageBoxImage.Warning);
                                }
                            }
                        }, _errorMap);
                        IsBusy = false;
                    }
                }
            }
        }

        private async Task SendVerifyEmail()
        {
            if (_currentUserFull != null)
            {
                var validator = new UserValidator().ValidateEmailOnly();
                var result = validator.Validate(new UserDto { Email = NewEmail });

                if (!result.IsValid)
                {
                    ShowError(result.Errors.First().ErrorMessage, Lang.LoginValidationTitle, MessageBoxImage.Warning);
                }
                else
                {
                    await ExecuteRequest(async () =>
                    {
                        bool sent = await ServiceProxy.Instance.Client.RequestEmailChangeAsync(IdUser, NewEmail);
                        if (sent)
                        {
                            IsChangeEmailVisible = false;
                            IsVerifyEmailVisible = true;
                        }
                        else
                        {
                            ShowError(Lang.RegisterEmailSendFailed, Lang.GlobalMessageBoxTitleError);
                        }
                    }, _errorMap);
                }
            }
        }

        private async Task VerifyEmailCode()
        {
            await ExecuteRequest(async () =>
            {
                bool ok = await ServiceProxy.Instance.Client.ConfirmEmailChangeAsync(IdUser, NewEmail, VerificationCode);
                if (ok)
                {
                    Email = NewEmail;
                    IsVerifyEmailVisible = false;
                    IsEmailVerifiedVisible = true;
                }
                else
                {
                    ShowError(Lang.RegisterCodeExpiredOrIncorrect, Lang.LoginValidationTitle, MessageBoxImage.Warning);
                }
            }, _errorMap);
        }

        private async Task VerifyCurrentPassword()
        {
            await ExecuteRequest(async () =>
            {
                bool ok = await ServiceProxy.Instance.Client.VerifyPasswordAsync(IdUser, CurrentPassword);
                if (ok)
                {
                    IsChangePasswordVisible = false;
                    IsNewPasswordVisible = true;
                }
                else
                {
                    ShowError(Lang.LoginInvalidCredentials, Lang.LoginValidationTitle, MessageBoxImage.Warning);
                }
            }, _errorMap);
        }

        private async Task SaveNewPassword()
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
                    bool ok = await ServiceProxy.Instance.Client.ChangePasswordAsync(IdUser, NewPassword);
                    if (ok)
                    {
                        ShowSuccess(Lang.GlobalMessageBoxTitleSuccess);
                        CloseOverlay();
                    }
                }, _errorMap);
            }
        }

        private void EditProfile()
        {
            IsEditing = true;
        }

        private void CancelEdit()
        {
            if (_currentUserFull != null)
            {
                MapUserFromDTO(_currentUserFull);
            }
            if (_socialMedia != null)
            {
                MapSocialMediaFromDTO(_socialMedia);
            }
            IsEditing = false;
        }

        private void OpenChangeEmail()
        {
            IsOverlayVisible = true;
            IsChangeEmailVisible = true;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;
            NewEmail = "";
            VerificationCode = "";
        }

        private void OpenChangePassword()
        {
            ResetPasswordPanelState();

            IsOverlayVisible = true;
            IsChangePasswordVisible = true;
            IsNewPasswordVisible = false;
        }

        private void CloseOverlay()
        {
            IsOverlayVisible = false;
            IsChangeEmailVisible = false;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;
            IsChangePasswordVisible = false;
            IsNewPasswordVisible = false;

            ResetPasswordPanelState();

            NewEmail = string.Empty;
            VerificationCode = string.Empty;
        }

        private void BackToChangePassword()
        {
            ResetPasswordPanelState();

            IsNewPasswordVisible = false;
            IsChangePasswordVisible = true;
        }

        private void ResetPasswordPanelState()
        {           
            IsCurrentPasswordVisible = false;
            IsNewPasswordVisibleEye = false;
            IsConfirmNewPasswordVisible = false;
            
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }

        private void BackToEditEmail()
        {
            IsVerifyEmailVisible = false;
            IsChangeEmailVisible = true;
        }

        private void OpenChangeAvatar()
        {
            IsAvatarOverlayVisible = true;
            LoadAvatars();
        }

        private void CloseAvatarOverlay()
        {
            IsAvatarOverlayVisible = false;
        }

        private void AcceptAvatarChange()
        {
            if (_selectedAvatar != null)
            {
                IdAvatar = _selectedAvatar.AvatarId;
                AvatarUrl = _selectedAvatar.AvatarUrl;
            }
            CloseAvatarOverlay();
        }

        private bool CanAcceptAvatarChange()
        {
            return IsAvatarSelected;
        }

        private void SelectAvatar(AvatarItemViewModel av)
        {
            if (av != null)
            {
                foreach (var a in Avatars)
                {
                    a.IsSelected = false;
                }
                av.IsSelected = true;
                _selectedAvatar = av;
                OnPropertyChanged(nameof(IsAvatarSelected));
                AcceptAvatarChangeCommand.RaiseCanExecuteChanged();
            }
        }

        private void GoBackToMenu(object windowObj)
        {
            if (windowObj is Window window)
            {
                var mainMenuView = new MainMenuView();
                mainMenuView.Show();
                window.Close();
            }
        }

        private void MapUserFromDTO(UserDto dto)
        {
            IdAvatar = dto.AvatarId;
            IdUser = dto.UserId;
            Nickname = dto.Nickname;
            FirstName = dto.FirstName;
            PaternalLastName = dto.PaternalLastName;
            MaternalLastName = dto.MaternalLastName;
            Email = dto.Email;
            AvatarUrl = dto.AvatarUrl;
        }

        private void MapSocialMediaFromDTO(SocialMediaDto dto)
        {
            Twitter = dto.Twitter;
            Facebook = dto.Facebook;
            Instagram = dto.Instagram;
            TikTok = dto.TikTok;
        }

        private void LoadAvatars()
        {
            Avatars = new ObservableCollection<AvatarItemViewModel>();
            for (int i = 0; i <= 10; i++)
            {
                string ext = (i == 4) ? "jpeg" : "jpg";
                if (i == 0)
                {
                    ext = "png";
                }
                Avatars.Add(new AvatarItemViewModel { AvatarId = i, AvatarUrl = $"/Images/Avatar/avatar{i:D2}.{ext}" });
            }
            _selectedAvatar = Avatars.FirstOrDefault(a => a.AvatarId == IdAvatar);
            if (_selectedAvatar != null)
            {
                _selectedAvatar.IsSelected = true;
            }
            OnPropertyChanged(nameof(IsAvatarSelected));
        }
    }

    public class AvatarItemViewModel : ObservableObject
    {
        public int AvatarId { get; set; }
        public string AvatarUrl { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}