using Contracts.DTOs;
using Lottery.Helpers;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.View.Friends;
using Lottery.View.MainMenu;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Friends
{
    public class FriendViewModel : ObservableObject
    {
        public FriendDto Dto
        {
            get;
        }

        public string Nickname
        {
            get
            {
                return Dto.Nickname;
            }
        }

        public int UserId
        {
            get
            {
                return Dto.FriendId;
            }
        }

        public FriendViewModel(FriendDto dto)
        {
            Dto = dto;
        }
    }

    public class FoundUserViewModel : ObservableObject
    {
        public FriendDto Dto
        {
            get;
        }

        public string Nickname
        {
            get
            {
                return Dto.Nickname;
            }
        }

        public int UserId
        {
            get
            {
                return Dto.UserId;
            }
        }

        private bool _isFriend;
        public bool IsFriend
        {
            get
            {
                return _isFriend;
            }
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
            get
            {
                return _hasPendingRequest;
            }
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
            get
            {
                return _pendingRequestSenderId;
            }
            set
            {
                if (SetProperty(ref _pendingRequestSenderId, value))
                {
                    NotifyChanges();
                }
            }
        }

        private readonly int _currentUserId;

        public bool CanSendRequest
        {
            get
            {
                return !IsFriend && !HasPendingRequest;
            }
        }

        public bool CanCancelRequest
        {
            get
            {
                return !IsFriend && HasPendingRequest && PendingRequestSenderId == _currentUserId;
            }
        }

        public bool CanAcceptRequest
        {
            get
            {
                bool isNotFriend = !IsFriend;
                bool hasRequest = HasPendingRequest;
                bool isNotSender = PendingRequestSenderId != _currentUserId;
                bool isValidSender = PendingRequestSenderId != 0;

                return isNotFriend && hasRequest && isNotSender && isValidSender;
            }
        }

        public bool CanRejectRequest
        {
            get
            {
                return CanAcceptRequest;
            }
        }

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

    public class InviteFriendsViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private readonly Dictionary<string, string> _errorMap;

        private string _inviteLobbyCode;
        private bool _isInviteMode = false;

        private string _searchNickname;
        public string SearchNickname
        {
            get
            {
                return _searchNickname;
            }
            set
            {
                SetProperty(ref _searchNickname, value);
            }
        }

        public ObservableCollection<FoundUserViewModel> SearchResults
        {
            get;
        } = new ObservableCollection<FoundUserViewModel>();

        public ObservableCollection<FriendViewModel> FriendsList
        {
            get;
        } = new ObservableCollection<FriendViewModel>();

        public ICommand SearchCommand
        {
            get;
        }

        public RelayCommand<FoundUserViewModel> SendRequestCommand
        {
            get;
        }

        public RelayCommand<FoundUserViewModel> CancelRequestCommand
        {
            get;
        }

        public RelayCommand<FoundUserViewModel> AcceptRequestCommand
        {
            get;
        }

        public RelayCommand<FoundUserViewModel> RejectRequestCommand
        {
            get;
        }

        public RelayCommand<FriendViewModel> RemoveFriendCommand
        {
            get;
        }

        public RelayCommand<FriendViewModel> InviteFriendCommand
        {
            get;
            private set;
        }

        public ICommand ViewRequestsCommand
        {
            get;
        }

        public ICommand LoadFriendsCommand
        {
            get;
        }

        public ICommand GoBackToMenuCommand
        {
            get;
        }

        public InviteFriendsViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;

            _errorMap = new Dictionary<string, string>
            {
                { "FRIEND_DUPLICATE", Lang.InviteFriendsExceptionFriendDuplicate },
                { "FRIEND_INVALID", Lang.InviteFriendsExceptionFriendInvalid },
                { "FRIEND_NOT_FOUND", Lang.InviteFriendsExceptionFriendNotFound },
                { "AUTH_USER_NOT_FOUND", Lang.InviteFriendsExceptionUserNotFound },
                { "USER_NOT_FOUND", Lang.InviteFriendsExceptionUserNotFound },
                { "USER_IN_LOBBY", Lang.InviteFriendsExceptionUserInLobby },
                { "USER_OFFLINE", Lang.InviteFriendsExceptionUserOffline },
                { "FRIEND_NOT_CONNECTED", Lang.InviteFriendsExceptionUserOffline },
                { "FRIEND_GUEST_RESTRICTED", Lang.InviteFriendsExceptionFriendGuestRestricted },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "FR-500", Lang.GlobalExceptionInternalServerError }
            };

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
            await ExecuteRequest(async () =>
            {
                IEnumerable<FriendDto> friends = await ServiceProxy.Instance.Client.GetFriendsAsync(_currentUserId);

                FriendsList.Clear();
                if (friends != null)
                {
                    foreach (FriendDto friend in friends)
                    {
                        FriendsList.Add(new FriendViewModel(friend));
                    }
                }
            });
        }

        private async Task SearchUser()
        {
            if (!string.IsNullOrWhiteSpace(SearchNickname))
            {
                await LoadFriends();

                await ExecuteRequest(async () =>
                {
                    ILotteryService client = ServiceProxy.Instance.Client;
                    FriendDto user = await client.FindUserByNicknameAsync(SearchNickname);

                    if (user != null)
                    {
                        if (user.UserId == _currentUserId)
                        {
                            SearchResults.Clear();
                        }
                        else
                        {
                            IEnumerable<FriendDto> friends = await client.GetFriendsAsync(_currentUserId);
                            IEnumerable<FriendDto> pendingSent = await client.GetSentRequestsAsync(_currentUserId);
                            IEnumerable<FriendDto> pendingReceived = await client.GetPendingRequestsAsync(_currentUserId);

                            bool isFriend = friends.Any(f => f.FriendId == user.UserId);
                            bool hasPendingSent = pendingSent.Any(r => r.UserId == user.UserId);
                            FriendDto receivedRequest = pendingReceived.FirstOrDefault(r => r.FriendId == user.UserId);

                            SearchResults.Clear();

                            FoundUserViewModel foundUser = new FoundUserViewModel(user, _currentUserId);
                            foundUser.IsFriend = isFriend;
                            foundUser.HasPendingRequest = hasPendingSent || (receivedRequest != null);

                            if (hasPendingSent)
                            {
                                foundUser.PendingRequestSenderId = _currentUserId;
                            }
                            else
                            {
                                foundUser.PendingRequestSenderId = receivedRequest?.FriendId ?? 0;
                            }

                            SearchResults.Add(foundUser);
                        }
                    }
                }, _errorMap);
            }
        }

        private async Task SendRequest(FoundUserViewModel userVm)
        {
            if (userVm != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.SendRequestFriendshipAsync(_currentUserId, userVm.UserId);

                    userVm.HasPendingRequest = true;
                    userVm.PendingRequestSenderId = _currentUserId;
                    ShowSuccess(string.Format(Lang.InviteFriendsRequestsSent, userVm.Nickname));
                }, _errorMap);
            }
        }

        private async Task CancelRequest(FoundUserViewModel userVm)
        {
            if (userVm != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.CancelFriendRequestAsync(_currentUserId, userVm.UserId);

                    userVm.HasPendingRequest = false;
                    userVm.PendingRequestSenderId = 0;
                    ShowSuccess(Lang.InviteFriendsCancelRequest);
                }, _errorMap);
            }
        }

        private async Task AcceptRequest(FoundUserViewModel userVm)
        {
            if (userVm != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.AcceptFriendRequestAsync(_currentUserId, userVm.UserId);

                    userVm.HasPendingRequest = false;
                    userVm.IsFriend = true;

                    ShowSuccess(string.Format(userVm.Nickname, Lang.IniviteFriendsSucces));
                    await LoadFriends();
                }, _errorMap);
            }
        }

        private async Task RejectRequest(FoundUserViewModel userVm)
        {
            if (userVm != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.RejectFriendRequestAsync(_currentUserId, userVm.UserId);

                    userVm.HasPendingRequest = false;
                    userVm.PendingRequestSenderId = 0;
                    ShowSuccess(Lang.InviteFriendsFriendRequestRejected);
                }, _errorMap);
            }
        }

        private async Task RemoveFriend(FriendViewModel selectedFriend)
        {
            if (selectedFriend != null)
            {
                MessageBoxResult result = CustomMessageBox.Show(
                    string.Format(Lang.InviteFriendsAreYouSure, selectedFriend.Nickname),
                    Lang.GlobalMessageBoxTitleConfirm,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await ExecuteRequest(async () =>
                    {
                        await ServiceProxy.Instance.Client.RemoveFriendAsync(_currentUserId, selectedFriend.UserId);
                        await LoadFriends();
                    }, _errorMap);
                }
            }
        }

        private async Task InviteFriendToLobby(FriendViewModel friend)
        {
            bool isValidLobby = !string.IsNullOrEmpty(_inviteLobbyCode);

            if (_isInviteMode && isValidLobby && friend != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.InviteFriendToLobbyAsync(_inviteLobbyCode, friend.UserId);
                    ShowSuccess(string.Format(Lang.InviteFriendsSucces, friend.Nickname));
                }, _errorMap);
            }
        }

        public void SetInviteMode(string lobbyCode)
        {
            _isInviteMode = true;
            _inviteLobbyCode = lobbyCode;
        }

        private void ViewRequests()
        {
            FriendRequestsView requestsView = new FriendRequestsView();
            requestsView.ShowDialog();
            _ = LoadFriends();
        }

        private void ExecuteGoBackToMenu(Window friendsWindow)
        {
            if (!_isInviteMode)
            {
                MainMenuView mainMenuView = new MainMenuView();
                mainMenuView.Show();
            }

            if (friendsWindow != null)
            {
                friendsWindow.Close();
            }
        }
    }
}