using Lottery.View;
using Lottery.ViewModel.Base;
using System.Windows.Input;

namespace Lottery.ViewModel
{
    public class MainMenuViewModel : ObservableObject
    {
        public ICommand ShowFriendsViewCommand { get; }
        // Aquí añadir comandos para los otros botones (Settings, Profile, CreateLobby, etc.)

        public MainMenuViewModel()
        {
            ShowFriendsViewCommand = new RelayCommand(ExecuteShowFriendsView);
        }

        private void ExecuteShowFriendsView()
        {
            InviteFriendsView friendsView = new InviteFriendsView();
            friendsView.Show();
        }
    }
}