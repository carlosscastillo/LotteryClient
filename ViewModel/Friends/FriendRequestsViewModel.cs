using Lottery.LotteryServiceReference;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Friends
{
    public class FriendRequestsViewModel : ObservableObject
    {
        private readonly int _currentUserId;

        public ObservableCollection<FriendDto> PendingRequests { get; } = new ObservableCollection<FriendDto>();

        public ICommand LoadRequestsCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        public FriendRequestsViewModel()
        {
            _currentUserId = SessionManager.CurrentUser.UserId;

            LoadRequestsCommand = new RelayCommand(async () => await LoadRequests());
            AcceptCommand = new RelayCommand<FriendDto>(async (request) => await AcceptRequest(request));
            RejectCommand = new RelayCommand<FriendDto>(async (request) => await RejectRequest(request));

            _ = LoadRequests();
        }

        private async Task LoadRequests()
        {
            try
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
            }
            catch (FaultException<ServiceFault> ex)
            {
                if (ex.Detail.ErrorCode == "USER_OFFLINE")
                {
                    ShowServiceError(ex, Lang.FriendRequestsSessionError);
                }
                else
                {
                    MessageBox.Show(string.Format(Lang.FriendRequestsErrorLoadingRequests, ex.Message));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.FriendRequestsConnectionError, ex.Message), 
                    Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AcceptRequest(FriendDto request)
        {
            if (request == null) return;

            try
            {
                await ServiceProxy.Instance.Client.AcceptFriendRequestAsync(_currentUserId, request.FriendId);

                await LoadRequests();

                MessageBox.Show(string.Format(Lang.FriendRequestsNowFriendWith, request.Nickname), 
                    Lang.GlobalMessageBoxTitleSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.FriendRequestsRequestCouldNotBeAccepted);
                if (ex.Detail.ErrorCode == "FRIEND_NOT_FOUND" || ex.Detail.ErrorCode == "FRIEND_DUPLICATE")
                {
                    await LoadRequests();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.GlobalMessageBoxUnexpectedError, ex.Message), 
                    Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RejectRequest(FriendDto request)
        {
            if (request == null) return;

            try
            {
                await ServiceProxy.Instance.Client.RejectFriendRequestAsync(_currentUserId, request.FriendId);
                await LoadRequests();
            }
            catch (FaultException<ServiceFault> ex)
            {
                ShowServiceError(ex, Lang.FriendRequestsRequestCouldNotBeReject);
                await LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Lang.GlobalMessageBoxUnexpectedError, ex.Message), Lang.GlobalMessageBoxTitleError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowServiceError(FaultException<ServiceFault> fault, string title)
        {
            var detail = fault.Detail;
            string message = detail.Message;
            MessageBoxImage icon = MessageBoxImage.Warning;

            switch (detail.ErrorCode)
            {
                case "FRIEND_NOT_FOUND":
                    message = Lang.FriendRequestsExceptionFriendNotFound;
                    break;

                case "FRIEND_INVALID":
                    message = Lang.FriendRequestsExceptionFriendInvalid;
                    break;

                case "FRIEND_DUPLICATE":
                    message = Lang.FriendRequestsExceptionFriendDuplicate;
                    break;

                case "USER_OFFLINE":
                    message = Lang.FriendRequestsExceptionUserOffline;
                    icon = MessageBoxImage.Error;
                    break;

                case "FR-500":
                    message = Lang.FriendRequestsExceptionFR500;
                    icon = MessageBoxImage.Error;
                    break;

                default:
                    message = string.Format(Lang.GlobalExceptionServerError, detail.ErrorCode, detail.Message);
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }
    }
}