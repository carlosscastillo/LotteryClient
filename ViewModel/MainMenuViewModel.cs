using Lottery.View;
using Lottery.ViewModel.Base;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class MainMenuViewModel : ObservableObject
    {
        public string Nickname { get; }
        public ICommand ShowFriendsViewCommand { get; }
        public ICommand CreateLobbyCommand { get; }
        public ICommand JoinLobbyCommand { get; }
        // Aquí nos falta añadir comandos para los otros botones (Settings, Profile, CreateLobby, etc.)

        public MainMenuViewModel()
        {
            if (SessionManager.CurrentUser != null)
            {
                Nickname = SessionManager.CurrentUser.Nickname;
            }
            else
            {
                Nickname = "Invitado";
            }

            ShowFriendsViewCommand = new RelayCommand<Window>(ExecuteShowFriendsView);

            CreateLobbyCommand = new RelayCommand(ExecuteCreateLobby);
            JoinLobbyCommand = new RelayCommand(ExecuteJoinLobby);
        }

        private void ExecuteShowFriendsView(Window mainMenuWindow)
        {
            InviteFriendsView friendsView = new InviteFriendsView();

            friendsView.WindowState = WindowState.Maximized;

            friendsView.Show();

            mainMenuWindow?.Close();
        }

        private void ExecuteCreateLobby()
        {
            // ... (tu lógica para crear lobby)
        }

        private void ExecuteJoinLobby()
        {
            // ... (tu lógica para unirse a lobby)
        }
    }
}