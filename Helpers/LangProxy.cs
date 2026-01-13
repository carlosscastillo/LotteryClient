using Lottery.Properties.Langs;
using System.ComponentModel;

namespace Lottery.Helpers
{
    public class LangProxy : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string GlobalButtonCreateLobby => Lang.GlobalButtonCreateLobby;
        public string GlobalButtonJoinLobby => Lang.GlobalButtonJoinLobby;
        public string MainMenuButtonSettings => Lang.MainMenuButtonSettings;
        public string MainMenuButtonFriends => Lang.MainMenuButtonFriends;
        public string MainMenuButtonProfile => Lang.MainMenuButtonProfile;
        public string LeaderboardLabelTitle => Lang.LeaderboardLabelTitle;
        public string MainMenuButtonLogout => Lang.MainMenuButtonLogout;

        public void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}