using Lottery.LotteryServiceReference;
using Lottery.ViewModel.Base;
using System.Threading.Tasks;
using System.Windows;

namespace Lottery.ViewModel
{
    public class CustomizeProfileViewModel : ObservableObject
    {
        private readonly ILotteryService _serviceClient;
        private UserRegisterDTO _currentUserFull;

        public CustomizeProfileViewModel()
        {
            _serviceClient = SessionManager.ServiceClient;

            // Inicializar comandos
            EditCommand = new RelayCommand(EditProfile);
            SaveChangesCommand = new RelayCommand(async () => await SaveChanges());
            CancelCommand = new RelayCommand(CancelEdit);

            IsEditing = false;

            // Cargar datos completos desde el servicio
            _ = LoadFullUserData();
        }

        #region Propiedades del perfil

        private string _nickname;
        public string Nickname { get => _nickname; set => SetProperty(ref _nickname, value); }

        private string _firstName;
        public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

        private string _paternalLastName;
        public string PaternalLastName { get => _paternalLastName; set => SetProperty(ref _paternalLastName, value); }

        private string _maternalLastName;
        public string MaternalLastName { get => _maternalLastName; set => SetProperty(ref _maternalLastName, value); }

        private string _avatarUrl;
        public string AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }

        #endregion

        #region Estado de edición

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

        public bool IsReadOnly => !IsEditing;
        public Visibility EditButtonVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        public Visibility SaveCancelVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;

        #endregion

        #region Comandos

        public RelayCommand EditCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand CancelCommand { get; }

        private void EditProfile() => IsEditing = true;

        private async Task SaveChanges()
        {
            if (_currentUserFull == null) return;

            // Actualizar DTO completo
            _currentUserFull.Nickname = Nickname;
            _currentUserFull.FirstName = FirstName;
            _currentUserFull.PaternalLastName = PaternalLastName;
            _currentUserFull.MaternalLastName = MaternalLastName;
            //_currentUserFull.AvatarUrl = AvatarUrl;

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
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al guardar los cambios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit()
        {
            if (_currentUserFull != null)
            {
                MapFromDTO(_currentUserFull);
            }
            IsEditing = false;
        }

        #endregion

        #region Métodos privados

        private async Task LoadFullUserData()
        {
            try
            {
                if (SessionManager.CurrentUser == null) return;

                var fullUser = _serviceClient.GetUserProfile(SessionManager.CurrentUser.UserId);
                if (fullUser != null)
                {
                    _currentUserFull = fullUser;
                    MapFromDTO(_currentUserFull);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"No se pudieron cargar los datos del usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MapFromDTO(UserRegisterDTO dto)
        {
            Nickname = dto.Nickname;
            FirstName = dto.FirstName;
            PaternalLastName = dto.PaternalLastName;
            MaternalLastName = dto.MaternalLastName;
            AvatarUrl = dto.AvatarUrl;
        }

        #endregion
    }
}
