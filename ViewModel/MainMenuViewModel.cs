using Lottery.View;
using Lottery.ViewModel.Base;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class MainMenuViewModel : ObservableObject
    {
        public string Nickname { get; }
        public ICommand ShowFriendsViewCommand { get; }
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

            ShowFriendsViewCommand = new RelayCommand(ExecuteShowFriendsView);
        }

        private void ExecuteShowFriendsView()
        {
            InviteFriendsView friendsView = new InviteFriendsView();
            friendsView.Show();
        }
    }
}