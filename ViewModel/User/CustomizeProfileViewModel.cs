using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.ViewModel.User
{
    public class CustomizeProfileViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private UserRegisterDTO _currentUserFull;

        public CustomizeProfileViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;

            EditCommand = new RelayCommand(EditProfile);
            SaveChangesCommand = new RelayCommand(async () => await SaveChanges());
            CancelCommand = new RelayCommand(CancelEdit);

            ChangeEmailCommand = new RelayCommand(OpenChangeEmail);
            SendVerificationCodeCommand = new RelayCommand(async () => await SendVerifyEmail());
            VerifyEmailCodeCommand = new RelayCommand(async () => await VerifyEmailCode());
            BackToChangeEmailCommand = new RelayCommand(BackToEditEmail);
            CloseOverlayCommand = new RelayCommand(CloseOverlay);

            ChangePasswordCommand = new RelayCommand(ChangePassword);
            GoBackToMenuCommand = new RelayCommand<object>(GoBackToMenu);

            IsEditing = false;
            IsOverlayVisible = false;
            IsChangeEmailVisible = false;
            IsVerifyEmailVisible = false;
            IsEmailVerifiedVisible = false;

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

        public bool IsReadOnly => !IsEditing;
        public Visibility EditButtonVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        public Visibility SaveCancelVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;

        public RelayCommand EditCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand ChangeEmailCommand { get; }
        public RelayCommand SendVerificationCodeCommand { get; }
        public RelayCommand VerifyEmailCodeCommand { get; }
        public RelayCommand BackToChangeEmailCommand { get; }
        public RelayCommand OpenChangeEmailCommand { get; }
        public RelayCommand CloseOverlayCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand<object> GoBackToMenuCommand { get; }

        private void EditProfile() => IsEditing = true;

        private async Task SaveChanges()
        {
            if (_currentUserFull == null || !ValidateFields()) return;
            IsBusy = true;

            _currentUserFull.IdUser = IdUser;
            _currentUserFull.Nickname = Nickname;
            _currentUserFull.FirstName = FirstName;
            _currentUserFull.PaternalLastName = PaternalLastName;
            _currentUserFull.MaternalLastName = MaternalLastName;
            _currentUserFull.IdAvatar = IdAvatar;

            try
            {
                var (success, message) = await _serviceClient.UpdateProfileAsync(_currentUserFull.IdUser, _currentUserFull);
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

        private void BackToEditEmail()
        {
            IsVerifyEmailVisible = false;
            IsChangeEmailVisible = true;
            VerificationCode = string.Empty;
        }

        private void ChangePassword()
        {
            MessageBox.Show("Función de cambio de contraseña aún no implementada.");
        }

        private void GoBackToMenu(object windowObj)
        {
            if (windowObj is Window window)
                window.Close();
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
                    IdUser = _currentUserFull.IdUser;
                    MapFromDTO(_currentUserFull);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los datos del usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MapFromDTO(UserRegisterDTO dto)
        {
            IdAvatar = dto.IdAvatar;
            IdUser = dto.IdUser;
            Nickname = dto.Nickname;
            FirstName = dto.FirstName;
            PaternalLastName = dto.PaternalLastName;
            MaternalLastName = dto.MaternalLastName;
            Email = dto.Email;
            AvatarUrl = dto.AvatarUrl;
        }
    }
}
