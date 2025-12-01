using FluentValidation;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Lottery.Helpers;

namespace Lottery.ViewModel.User
{
    public class CustomizeProfileViewModel : ObservableObject
    {
        private UserDto _currentUserFull;
        private AvatarItemViewModel _selectedAvatar;

        public CustomizeProfileViewModel()
        {
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

            _ = LoadFullUserData();
        }

        private int _idAvatar;
        public int IdAvatar { get => _idAvatar; set => SetProperty(ref _idAvatar, value); }

        private int _idUser;
        public int IdUser { get => _idUser; set => SetProperty(ref _idUser, value); }

        private string _nickname;
        public string Nickname { get => _nickname; set => SetProperty(ref _nickname, value); }

        private string _firstName;
        public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

        private string _paternalLastName;
        public string PaternalLastName { get => _paternalLastName; set => SetProperty(ref _paternalLastName, value); }

        private string _maternalLastName;
        public string MaternalLastName { get => _maternalLastName; set => SetProperty(ref _maternalLastName, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _newEmail;
        public string NewEmail { get => _newEmail; set => SetProperty(ref _newEmail, value); }

        private string _verificationCode;
        public string VerificationCode { get => _verificationCode; set => SetProperty(ref _verificationCode, value); }

        private string _avatarUrl;
        public string AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }

        private string _currentPassword;
        public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }

        private string _newPassword;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _confirmNewPassword;
        public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }
        public bool IsCurrentPasswordVisible
        {
            get => _isCurrentPasswordVisible;
            set => SetProperty(ref _isCurrentPasswordVisible, value);
        }
        private bool _isCurrentPasswordVisible;
        public bool IsNewPasswordVisibleEye
        {
            get => _isNewPasswordVisibleEye;
            set => SetProperty(ref _isNewPasswordVisibleEye, value);
        }
        private bool _isNewPasswordVisibleEye;
        public bool IsConfirmNewPasswordVisible
        {
            get => _isConfirmNewPasswordVisible;
            set => SetProperty(ref _isConfirmNewPasswordVisible, value);
        }
        private bool _isConfirmNewPasswordVisible;

        private ObservableCollection<AvatarItemViewModel> _avatars;
        public ObservableCollection<AvatarItemViewModel> Avatars { get => _avatars; set => SetProperty(ref _avatars, value); }

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
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        private bool _isOverlayVisible;
        public bool IsOverlayVisible { get => _isOverlayVisible; set => SetProperty(ref _isOverlayVisible, value); }

        private bool _isChangeEmailVisible;
        public bool IsChangeEmailVisible { get => _isChangeEmailVisible; set => SetProperty(ref _isChangeEmailVisible, value); }

        private bool _isVerifyEmailVisible;
        public bool IsVerifyEmailVisible { get => _isVerifyEmailVisible; set => SetProperty(ref _isVerifyEmailVisible, value); }

        private bool _isEmailVerifiedVisible;
        public bool IsEmailVerifiedVisible { get => _isEmailVerifiedVisible; set => SetProperty(ref _isEmailVerifiedVisible, value); }

        private bool _isChangePasswordVisible;
        public bool IsChangePasswordVisible { get => _isChangePasswordVisible; set => SetProperty(ref _isChangePasswordVisible, value); }

        private bool _isNewPasswordVisible;
        public bool IsNewPasswordVisible { get => _isNewPasswordVisible; set => SetProperty(ref _isNewPasswordVisible, value); }

        private bool _isAvatarOverlayVisible;
        public bool IsAvatarOverlayVisible { get => _isAvatarOverlayVisible; set => SetProperty(ref _isAvatarOverlayVisible, value); }

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

        private async Task LoadFullUserData()
        {
            try
            {
                if (SessionManager.CurrentUser == null)
                {
                    return;
                }

                var fullUser = await ServiceProxy.Instance.Client.GetUserProfileAsync(SessionManager.CurrentUser.UserId);

                if (fullUser != null)
                {
                    _currentUserFull = fullUser;
                    IdUser = _currentUserFull.UserId;
                    MapFromDTO(_currentUserFull);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error al cargar perfil");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los datos del usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveChanges()
        {
            if (_currentUserFull == null) return;

            _currentUserFull.UserId = IdUser;
            _currentUserFull.Nickname = Nickname;
            _currentUserFull.FirstName = FirstName;
            _currentUserFull.PaternalLastName = PaternalLastName;
            _currentUserFull.MaternalLastName = MaternalLastName;
            _currentUserFull.AvatarId = IdAvatar;
            
            var validator = new UserValidator().ValidateRegister();
            var validationResult = validator.Validate(_currentUserFull);

            if (!validationResult.IsValid)
            {
                string errores = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
                MessageBox.Show(errores, "Datos inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            try
            {
                var (success, message) = await ServiceProxy.Instance.Client.UpdateProfileAsync(_currentUserFull.UserId, _currentUserFull);

                if (success)
                {
                    MessageBox.Show("Perfil actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsEditing = false;
                    SessionManager.CurrentUser.Nickname = Nickname;
                }
                else
                {
                    MessageBox.Show($"No se pudo actualizar el perfil: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error al guardar");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los cambios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SendVerifyEmail()
        {
            if (_currentUserFull == null)
            {
                MessageBox.Show("Los datos del usuario aún no se han cargado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var validator = new UserValidator().ValidateEmailOnly();
            var result = validator.Validate(new UserDto { Email = NewEmail });

            if (!result.IsValid)
            {
                string errores = string.Join("\n", result.Errors.Select(e => e.ErrorMessage));
                MessageBox.Show(errores, "Correo inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool codeSent = await ServiceProxy.Instance.Client.RequestEmailChangeAsync(IdUser, NewEmail);

                if (codeSent)
                {
                    MessageBox.Show("Se envió un código de verificación al nuevo correo.", "Verificación enviada");
                    IsChangeEmailVisible = false;
                    IsVerifyEmailVisible = true;
                }
                else
                {
                    MessageBox.Show("No se pudo enviar el código. Verifica que el correo no esté en uso o sea diferente al actual.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error de servicio");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al solicitar la verificación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task VerifyEmailCode()
        {
            var validator = new CodeValidator();
            var result = validator.Validate(VerificationCode);

            if (!result.IsValid)
            {
                MessageBox.Show(result.Errors.First().ErrorMessage, "Código inválido",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;

            try
            {
                bool verified = await ServiceProxy.Instance.Client.ConfirmEmailChangeAsync(IdUser, NewEmail, VerificationCode);

                if (verified)
                {
                    _currentUserFull.Email = NewEmail;
                    Email = NewEmail;
                    IsVerifyEmailVisible = false;
                    IsEmailVerifiedVisible = true;
                    MessageBox.Show("Correo verificado y actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Código incorrecto o expirado. Intenta nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error de Verificación");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar el código: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task VerifyCurrentPassword()
        {
            var validator = new UserValidator().ValidatePasswordOnly();
            var result = validator.Validate(new UserDto { Password = CurrentPassword });

            if (!result.IsValid)
            {
                string errores = string.Join("\n", result.Errors.Select(e => e.ErrorMessage));
                MessageBox.Show(errores, "Contraseña inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;

            try
            {
                bool isValid = await ServiceProxy.Instance.Client.VerifyPasswordAsync(IdUser, CurrentPassword);

                if (isValid)
                {
                    IsChangePasswordVisible = false;
                    IsNewPasswordVisible = true;
                }
                else
                {
                    MessageBox.Show("La contraseña actual es incorrecta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error de Verificación");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar la contraseña: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveNewPassword()
        {
            var validator = new UserValidator().ValidatePasswordOnly();
            var result = validator.Validate(new UserDto { Password = NewPassword });

            if (!result.IsValid)
            {
                string errores = string.Join("\n", result.Errors.Select(e => e.ErrorMessage));
                MessageBox.Show(errores, "Contraseña inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;

            try
            {
                bool resultChange = await ServiceProxy.Instance.Client.ChangePasswordAsync(IdUser, NewPassword);

                if (resultChange)
                {
                    MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseOverlay();
                }
                else
                {
                    MessageBox.Show("No se pudo cambiar la contraseña. Verifica los datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, "Error al cambiar contraseña");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                NewPassword = ConfirmNewPassword = CurrentPassword = string.Empty;
            }
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            var detail = fault.Detail;
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "USER_DUPLICATE":
                    message = "El nickname o correo electrónico ya están en uso por otro usuario.";
                    break;

                case "USER_NOT_FOUND":
                    message = "No se encontró la información del usuario.";
                    break;

                case "VERIFY_EMAIL_SEND_FAILED":
                    message = "No pudimos enviar el correo. Verifica que la dirección sea correcta.";
                    break;

                case "VERIFY_ERROR":
                    message = "Código de verificación incorrecto o expirado.";
                    break;

                case "USER_OFFLINE":
                    message = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                    icon = MessageBoxImage.Error;
                    break;

                case "USER_INTERNAL_ERROR":
                    message = "Ocurrió un error interno al procesar tu perfil.";
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = $"Error del servidor: {detail.Message}";
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        private void EditProfile() => IsEditing = true;

        private void CancelEdit()
        {
            if (_currentUserFull != null)
            {
                MapFromDTO(_currentUserFull);
            }
            IsEditing = false;
        }

        private void OpenChangeEmail()
        {
            IsOverlayVisible = true;
            IsChangeEmailVisible = true;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;
            NewEmail = string.Empty;
            VerificationCode = string.Empty;
        }

        private void OpenChangePassword()
        {
            IsOverlayVisible = true;
            IsChangePasswordVisible = true;
            IsNewPasswordVisible = false;
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }

        private void OpenChangeAvatar()
        {
            IsAvatarOverlayVisible = true;
            LoadAvatars();
        }

        private void CloseOverlay()
        {
            IsOverlayVisible = false;
            IsChangeEmailVisible = false;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;
            NewEmail = string.Empty;
            VerificationCode = string.Empty;
        }

        private void BackToChangePassword()
        {
            IsNewPasswordVisible = false;
            IsChangePasswordVisible = true;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }

        private void BackToEditEmail()
        {
            IsVerifyEmailVisible = false;
            IsChangeEmailVisible = true;
            VerificationCode = string.Empty;
        }

        private void CloseAvatarOverlay()
        {
            if (Avatars != null)
            {
                _selectedAvatar = null;
                foreach (var avatar in Avatars)
                {
                    avatar.IsSelected = (avatar.AvatarId == this.IdAvatar);
                    if (avatar.IsSelected)
                    {
                        _selectedAvatar = avatar;
                    }
                }
                OnPropertyChanged(nameof(IsAvatarSelected));
            }
            IsAvatarOverlayVisible = false;
        }

        private void AcceptAvatarChange()
        {
            if (_selectedAvatar != null)
            {
                AvatarUrl = _selectedAvatar.AvatarUrl;
                IdAvatar = _selectedAvatar.AvatarId;
            }
            CloseAvatarOverlay();
        }

        private bool CanAcceptAvatarChange()
        {
            return IsAvatarSelected;
        }

        private void SelectAvatar(AvatarItemViewModel selectedAvatar)
        {
            if (selectedAvatar == null)
            {
                return;
            }

            foreach (var avatar in Avatars)
            {
                avatar.IsSelected = false;
            }

            selectedAvatar.IsSelected = true;
            _selectedAvatar = selectedAvatar;

            OnPropertyChanged(nameof(IsAvatarSelected));
            AcceptAvatarChangeCommand.RaiseCanExecuteChanged();
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

        private void MapFromDTO(UserDto dto)
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

        private void LoadAvatars()
        {
            Avatars = new ObservableCollection<AvatarItemViewModel>
            {
                new AvatarItemViewModel 
                { 
                    AvatarId = 0, AvatarUrl = "/Images/Avatar/avatar00.png" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 1, AvatarUrl = "/Images/Avatar/avatar01.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 2, AvatarUrl = "/Images/Avatar/avatar02.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 3, AvatarUrl = "/Images/Avatar/avatar03.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 4, AvatarUrl = "/Images/Avatar/avatar04.jpeg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 5, AvatarUrl = "/Images/Avatar/avatar05.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 6, AvatarUrl = "/Images/Avatar/avatar06.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 7, AvatarUrl = "/Images/Avatar/avatar07.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 8, AvatarUrl = "/Images/Avatar/avatar08.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 9, AvatarUrl = "/Images/Avatar/avatar09.jpg" 
                },
                new AvatarItemViewModel 
                { 
                    AvatarId = 10, AvatarUrl = "/Images/Avatar/avatar10.jpg" 
                },
            };

            _selectedAvatar = Avatars.FirstOrDefault(a => a.AvatarId == this.IdAvatar);
            if (_selectedAvatar != null)
            {
                _selectedAvatar.IsSelected = true;
            }

            OnPropertyChanged(nameof(IsAvatarSelected));
            CommandManager.InvalidateRequerySuggested();
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