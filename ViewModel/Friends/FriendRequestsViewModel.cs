using Contracts.DTOs;
using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Friends
{
    public class FriendRequestsViewModel : BaseViewModel
    {
        private readonly int _currentUserId;
        private readonly Dictionary<string, string> _errorMap;

        public ObservableCollection<FriendDto> PendingRequests { get; } = new ObservableCollection<FriendDto>();

        public ICommand LoadRequestsCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        public FriendRequestsViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;

            _errorMap = new Dictionary<string, string>
            {
                { "FRIEND_NOT_FOUND", Lang.FriendRequestsExceptionFriendNotFound },
                { "FRIEND_INVALID", Lang.FriendRequestsExceptionFriendInvalid },
                { "FRIEND_DUPLICATE", Lang.FriendRequestsExceptionFriendDuplicate },
                { "USER_OFFLINE", Lang.FriendRequestsExceptionUserOffline },
                { "DB_ERROR", Lang.GlobalExceptionConnectionDatabaseMessage },
                { "FR-500", Lang.FriendRequestsExceptionFR500 }
            };

            LoadRequestsCommand = new RelayCommand(async () => await LoadRequests());
            AcceptCommand = new RelayCommand<FriendDto>(async (request) => await AcceptRequest(request));
            RejectCommand = new RelayCommand<FriendDto>(async (request) => await RejectRequest(request));

            _ = LoadRequests();
        }

        private async Task LoadRequests()
        {
            await ExecuteRequest(async () =>
            {
                var requests = await ServiceProxy.Instance.Client.GetPendingRequestsAsync(_currentUserId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PendingRequests.Clear();
                    if (requests != null)
                    {
                        foreach (var req in requests)
                        {
                            PendingRequests.Add(req);
                        }
                    }
                });
            }, _errorMap);
        }

        private async Task AcceptRequest(FriendDto request)
        {
            if (request != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.AcceptFriendRequestAsync(_currentUserId, request.FriendId);

                    ShowSuccess(string.Format(Lang.FriendRequestsNowFriendWith, request.Nickname));

                    await LoadRequests();

                }, _errorMap);
            }
        }

        private async Task RejectRequest(FriendDto request)
        {
            if (request != null)
            {
                await ExecuteRequest(async () =>
                {
                    await ServiceProxy.Instance.Client.RejectFriendRequestAsync(_currentUserId, request.FriendId);
                    await LoadRequests();

                }, _errorMap);
            }
        }
    }
}