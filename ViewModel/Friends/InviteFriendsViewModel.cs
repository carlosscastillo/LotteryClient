using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Friends;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Friends
{
    public class FriendViewModel : ObservableObject
    {
        public FriendDto Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.FriendId;
        public FriendViewModel(FriendDto dto) { Dto = dto; }
    }

    public class FoundUserViewModel : ObservableObject
    {
        public FriendDto Dto { get; }
        public string Nickname => Dto.Nickname;
        public int UserId => Dto.UserId;

        private bool _isFriend;
        public bool IsFriend
        {
            get => _isFriend;
            set 
            { 
                if (SetProperty(ref _isFriend, value))
                {
                    NotifyChanges();
                }
            }
        }

        private bool _hasPendingRequest;
        public bool HasPendingRequest
        {
            get => _hasPendingRequest;
            set 
            { 
                if (SetProperty(ref _hasPendingRequest, value))
                {
                    NotifyChanges();
                }
            }
        }

        private int _pendingRequestSenderId = 0;
        public int PendingRequestSenderId
        {
            get => _pendingRequestSenderId;
            set 
            { 
                if (SetProperty(ref _pendingRequestSenderId, value))
                {
                    NotifyChanges();
                }
            }
        }

        private readonly int _currentUserId;

        public bool CanSendRequest => !IsFriend && !HasPendingRequest;
        public bool CanCancelRequest => !IsFriend && HasPendingRequest && PendingRequestSenderId == _currentUserId;
        public bool CanAcceptRequest =>
            !IsFriend && HasPendingRequest && PendingRequestSenderId != _currentUserId && PendingRequestSenderId != 0;
        public bool CanRejectRequest => CanAcceptRequest;

        public FoundUserViewModel(FriendDto dto, int currentUserId)
        {
            Dto = dto;
            _currentUserId = currentUserId;
        }

        private void NotifyChanges()
        {
            OnPropertyChanged(nameof(CanSendRequest));
            OnPropertyChanged(nameof(CanCancelRequest));
            OnPropertyChanged(nameof(CanAcceptRequest));
            OnPropertyChanged(nameof(CanRejectRequest));
        }
    }

    public class InviteFriendsViewModel : ObservableObject
    {
        private readonly int _currentUserId;

        private string _inviteLobbyCode;
        private bool _isInviteMode = false;

        private string _searchNickname;
        public string SearchNickname
        {
            get => _searchNickname;
            set => SetProperty(ref _searchNickname, value);
        }

        public ObservableCollection<FoundUserViewModel> SearchResults { get; } = 
            new ObservableCollection<FoundUserViewModel>();
        public ObservableCollection<FriendViewModel> FriendsList { get; } = 
            new ObservableCollection<FriendViewModel>();

        public ICommand SearchCommand { get; }
        public RelayCommand<FoundUserViewModel> SendRequestCommand { get; }
        public RelayCommand<FoundUserViewModel> CancelRequestCommand { get; }
        public RelayCommand<FoundUserViewModel> AcceptRequestCommand { get; }
        public RelayCommand<FoundUserViewModel> RejectRequestCommand { get; }
        public RelayCommand<FriendViewModel> RemoveFriendCommand { get; }
        public RelayCommand<FriendViewModel> InviteFriendCommand { get; private set; }

        public ICommand ViewRequestsCommand { get; }
        public ICommand LoadFriendsCommand { get; }
        public ICommand GoBackToMenuCommand { get; }

        public InviteFriendsViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;

            SearchCommand = new RelayCommand(async () => await SearchUser());
            SendRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await SendRequest(user));
            CancelRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await CancelRequest(user));
            AcceptRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await AcceptRequest(user));
            RejectRequestCommand = new RelayCommand<FoundUserViewModel>(async (user) => await RejectRequest(user));
            RemoveFriendCommand = new RelayCommand<FriendViewModel>(async (friend) => await RemoveFriend(friend));

            InviteFriendCommand = new RelayCommand<FriendViewModel>(async (friend) => await InviteFriendToLobby(friend));

            ViewRequestsCommand = new RelayCommand(ViewRequests);
            LoadFriendsCommand = new RelayCommand(async () => await LoadFriends());
            GoBackToMenuCommand = new RelayCommand<Window>(ExecuteGoBackToMenu);

            _ = LoadFriends();
        }

        private async Task LoadFriends()
        {
            try
            {
                var friends = await ServiceProxy.Instance.Client.GetFriendsAsync(_currentUserId);

                FriendsList.Clear();
                if (friends != null)
                {
                    foreach (var friend in friends)
                    {
                        FriendsList.Add(new FriendViewModel(friend));
                    }
                }
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.InviteFriendsErrorLoadingFriends);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.InviteFriendsConnectionError, ex.Message), 
                    Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SearchUser()
        {
            if (string.IsNullOrWhiteSpace(SearchNickname))
            {
                return;
            }

            await LoadFriends();

            try
            {
                var client = ServiceProxy.Instance.Client;

                var user = await client.FindUserByNicknameAsync(SearchNickname);

                if (user == null)
                {
                    return;
                }

                if (user.UserId == _currentUserId)
                {
                    SearchResults.Clear();
                    return;
                }

                var friends = await client.GetFriendsAsync(_currentUserId);
                var pendingSent = await client.GetSentRequestsAsync(_currentUserId);
                var pendingReceived = await client.GetPendingRequestsAsync(_currentUserId);

                bool isFriend = friends.Any(f => f.FriendId == user.UserId);
                bool hasPendingSent = pendingSent.Any(r => r.UserId == user.UserId);
                var receivedRequest = pendingReceived.FirstOrDefault(r => r.FriendId == user.UserId);

                SearchResults.Clear();
                SearchResults.Add(new FoundUserViewModel(user, _currentUserId)
                {
                    IsFriend = isFriend,
                    HasPendingRequest = hasPendingSent || (receivedRequest != null),
                    PendingRequestSenderId = hasPendingSent ? _currentUserId : receivedRequest?.FriendId ?? 0
                });
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == "USER_NOT_FOUND")
                {
                    ShowServiceError(ex, Lang.InviteFriendsSearch);
                    SearchResults.Clear();
                }
                else
                {
                    ShowServiceError(ex, Lang.InviteFriendsSearchError);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.GlobalMessageBoxUnexpectedError, ex.Message), 
                    Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SendRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;
            try
            {
                await ServiceProxy.Instance.Client.SendRequestFriendshipAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = true;
                userVm.PendingRequestSenderId = _currentUserId;
                MessageBox.Show(string.Format(Lang.InviteFriendsRequestsSent, userVm.Nickname), 
                    Lang.GlobalMessageBoxTitleSent, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.InviteFriendsNotSent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.InviteFriendsError, ex.Message));
            }
        }

        private async Task CancelRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;
            try
            {
                await ServiceProxy.Instance.Client.CancelFriendRequestAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = false;
                userVm.PendingRequestSenderId = 0;
                MessageBox.Show(Lang.InviteFriendsCancelRequest);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.InviteFriendErrorCanceling);
                if (ex.Detail.ErrorCode == "FRIEND_NOT_FOUND")
                {
                    userVm.HasPendingRequest = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.InviteFriendsError, ex.Message));
            }
        }

        private async Task AcceptRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;
            try
            {
                await ServiceProxy.Instance.Client.AcceptFriendRequestAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = false;
                userVm.IsFriend = true;

                MessageBox.Show(string.Format(userVm.Nickname, Lang.IniviteFriendsSucces), 
                    Lang.GlobalMessageBoxTitleSuccess);
                await LoadFriends();
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.InviteFriendsErrorWhenAccepting);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.InviteFriendsError, ex.Message));
            }
        }

        private async Task RejectRequest(FoundUserViewModel userVm)
        {
            if (userVm == null) return;
            try
            {
                await ServiceProxy.Instance.Client.RejectFriendRequestAsync(_currentUserId, userVm.UserId);

                userVm.HasPendingRequest = false;
                userVm.PendingRequestSenderId = 0;
                MessageBox.Show(Lang.InviteFriendsFriendRequestRejected);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.InviteFriendsErrorWhenRejecting);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.InviteFriendsError, ex.Message));
            }
        }

        private async Task RemoveFriend(FriendViewModel selectedFriend)
        {
            if (selectedFriend == null) return;

            if (MessageBox.Show(string.Format(Lang.InviteFriendsAreYouSure, selectedFriend.Nickname),
                Lang.GlobalMessageBoxTitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    await ServiceProxy.Instance.Client.RemoveFriendAsync(_currentUserId, selectedFriend.UserId);
                    await LoadFriends();
                }
                catch (FaultException<ServiceFault> ex)
                {
                    ShowServiceError(ex, Lang.InviteFriendsErrorWhenDeleting);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Lang.InviteFriendsError, ex.Message));
                }
            }
        }

        private async Task InviteFriendToLobby(FriendViewModel friend)
        {
            if (!_isInviteMode || string.IsNullOrEmpty(_inviteLobbyCode)) return;

            try
            {
                await ServiceProxy.Instance.Client.InviteFriendToLobbyAsync(_inviteLobbyCode, friend.UserId);
                MessageBox.Show(string.Format(Lang.InviteFriendsSucces, friend.Nickname), 
                    Lang.GlobalMessageBoxTitleSent);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.InviteFriendsErrorInvitation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.InviteFriendsError, ex.Message));
            }
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            var detail = fault.Detail;
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "FRIEND_DUPLICATE":
                    message = Lang.InviteFriendsExceptionFriendDuplicate + SearchNickname;
                    break;

                case "FRIEND_INVALID":
                    message = Lang.InviteFriendsExceptionFriendInvalid;
                    icon = MessageBoxImage.Error;
                    break;

                case "FRIEND_NOT_FOUND":
                    message = Lang.InviteFriendsExceptionFriendNotFound;
                    break;

                case "USER_NOT_FOUND":
                    message = Lang.InviteFriendsExceptionUserNotFound;
                    title = "Búsqueda";
                    icon = MessageBoxImage.Information;
                    break;

                case "USER_IN_LOBBY":
                    message = Lang.InviteFriendsExceptionUserInLobby;
                    break;

                case "USER_OFFLINE":
                case "FRIEND_NOT_CONNECTED":
                    message = Lang.InviteFriendsExceptionUserOffline;
                    break;

                case "FRIEND_GUEST_RESTRICTED":
                    message = Lang.InviteFriendsExceptionFriendGuestRestricted;
                    break;

                case "FR-500":
                    message = Lang.GlobalExceptionInternalServerError;
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = string.Format(Lang.GlobalExceptionServerError, detail.ErrorCode, detail.Message);
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        public void SetInviteMode(string lobbyCode)
        {
            _isInviteMode = true;
            _inviteLobbyCode = lobbyCode;
        }

        private void ViewRequests()
        {
            var requestsView = new FriendRequestsView();
            requestsView.ShowDialog();
            _ = LoadFriends();
        }

        private void ExecuteGoBackToMenu(Window friendsWindow)
        {
            if (!_isInviteMode)
            {
                var mainMenuView = new MainMenuView();
                mainMenuView.Show();
            }
            friendsWindow?.Close();
        }
    }
}