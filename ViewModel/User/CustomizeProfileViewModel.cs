using Lottery.LotteryServiceReference;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Lottery.ViewModel.User
{
    public class CustomizeProfileViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private UserDto _currentUserFull;

        public CustomizeProfileViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;

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
            SaveNewPasswordCommand = new RelayCommand(SaveNewPassword);
            BackToChangePasswordCommand = new RelayCommand(BackToChangePassword);

            OpenChangeAvatarCommand = new RelayCommand(OpenChangeAvatar);
            // 

            GoBackToMenuCommand = new RelayCommand<object>(GoBackToMenu);

            IsEditing = false;
            IsOverlayVisible = false;
            IsChangeEmailVisible = false;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;
            IsChangePasswordVisible = false;
            IsNewPasswordVisible = false;
            IsAvatarSelectionVisible = false;

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

        private ObservableCollection<string> _avatars;
        public ObservableCollection<string> Avatars { get => _avatars; set => SetProperty(ref _avatars, value); }

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
        
        private bool _isAvatarSelectionVisible;
        public bool IsAvatarSelectionVisible { get => _isAvatarSelectionVisible; set => SetProperty(ref _isAvatarSelectionVisible, value); }
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
        public RelayCommand<string> SelectAvatarCommand { get; }
        public RelayCommand BackToProfileCommand { get; }
        public RelayCommand<object> GoBackToMenuCommand { get; }
        private void EditProfile() => IsEditing = true;

        private async Task SaveChanges()
        {
            if (_currentUserFull == null || !ValidateFields()) return;
            IsBusy = true;

            _currentUserFull.UserId = IdUser;
            _currentUserFull.Nickname = Nickname;
            _currentUserFull.FirstName = FirstName;
            _currentUserFull.PaternalLastName = PaternalLastName;
            _currentUserFull.MaternalLastName = MaternalLastName;
            _currentUserFull.AvatarId = IdAvatar;

            try
            {
                var (success, message) = await _serviceClient.UpdateProfileAsync(_currentUserFull.UserId, _currentUserFull);
                if (success)
                {
                    MessageBox.Show("Perfil actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsEditing = false;
                }
                else
                {
                    MessageBox.Show($"No se pudo actualizar el perfil: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar los cambios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("El nombre es obligatorio.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(PaternalLastName))
            {
                MessageBox.Show("El apellido paterno es obligatorio.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Nickname))
            {
                MessageBox.Show("El nickname es obligatorio.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void CancelEdit()
        {
            if (_currentUserFull != null)
                MapFromDTO(_currentUserFull);
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
            IsOverlayVisible = true;
            IsAvatarSelectionVisible = true;
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

        private async Task SendVerifyEmail()
        {
            if (_currentUserFull == null)
            {
                MessageBox.Show("Los datos del usuario aún no se han cargado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewEmail))
            {
                MessageBox.Show("Por favor, ingresa un nuevo correo electrónico.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool codeSent = await _serviceClient.RequestEmailChangeAsync(IdUser, NewEmail);
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
                MessageBox.Show(ex.Detail.Message, "Error de servicio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al solicitar la verificación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task VerifyEmailCode()
        {
            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                MessageBox.Show("Ingresa el código de verificación.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;

            try
            {
                bool verified = await _serviceClient.ConfirmEmailChangeAsync(IdUser, NewEmail, VerificationCode);
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar el código: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }

        private async Task VerifyCurrentPassword()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                MessageBox.Show("Por favor ingresa tu contraseña actual.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;

            try
            {
                bool isValid = await _serviceClient.VerifyPasswordAsync(IdUser, CurrentPassword);
                if (isValid)
                {
                    // Paso a nueva contraseña
                    IsChangePasswordVisible = false;
                    IsNewPasswordVisible = true;
                }
                else
                {
                    MessageBox.Show("La contraseña actual es incorrecta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private async void SaveNewPassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                MessageBox.Show("Por favor completa todos los campos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                bool result = await _serviceClient.ChangePasswordAsync(IdUser, NewPassword);
                if (result)
                {
                    MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseOverlay();
                }
                else
                {
                    MessageBox.Show("No se pudo cambiar la contraseña. Verifica los datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar la contraseña: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
                CurrentPassword = string.Empty;
            }
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
        
        private void GoBackToMenu(object windowObj)
        {
            if (windowObj is Window window)
            {                                
                var mainMenuView = new MainMenuView();
                mainMenuView.Show();
                window.Close();
            }
        }

        private async Task LoadFullUserData()
        {
            try
            {
                if (SessionManager.CurrentUser == null) return;

                var fullUser = await _serviceClient.GetUserProfileAsync(SessionManager.CurrentUser.UserId);
                if (fullUser != null)
                {
                    _currentUserFull = fullUser;
                    IdUser = _currentUserFull.UserId;
                    MapFromDTO(_currentUserFull);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los datos del usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Avatars = new ObservableCollection<string>
            {
                "/Images/Avatar/avatar00.png",
                "/Images/Avatar/avatar01.jpg",
                "/Images/Avatar/avatar02.jpg",
                "/Images/Avatar/avatar03.jpg",
                "/Images/Avatar/avatar04.jpeg",
                "/Images/Avatar/avatar05.jpg",
                "/Images/Avatar/avatar06.jpg",
                "/Images/Avatar/avatar07.jpg",
                "/Images/Avatar/avatar08.jpg",
                "/Images/Avatar/avatar09.jpg",
                "/Images/Avatar/avatar10.jpg",
            };
        }

    }
}
