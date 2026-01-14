using Lottery.LotteryServiceReference;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lottery
{
    public class ServiceProxy
    {
        private static ServiceProxy _instance;
        public static ServiceProxy Instance => _instance ?? (_instance = new ServiceProxy());

        private ILotteryService _client;
        private ClientCallbackHandler _callbackHandler;

        private readonly Queue<Func<Task>> _pendingActions = new Queue<Func<Task>>();
        private DispatcherTimer _reconnectTimer;
        private DispatcherTimer _offlineTimeoutTimer;
        public bool IsOfflineMode { get; private set; } = false;

        public ILotteryService Client
        {
            get
            {
                if (_client == null ||
                    (_client as ICommunicationObject)?.State == CommunicationState.Faulted ||
                    (_client as ICommunicationObject)?.State == CommunicationState.Closed)
                {
                    CreateClient();
                }
                return _client;
            }
        }

        public event Action ConnectionLost;

        private ServiceProxy()
        {
            CreateClient();
            InitializeTimers();
        }

        private void InitializeTimers()
        {
            _reconnectTimer = new DispatcherTimer();
            _reconnectTimer.Interval = TimeSpan.FromSeconds(2);
            _reconnectTimer.Tick += async (s, e) => await TryFlushQueueAsync();

            _offlineTimeoutTimer = new DispatcherTimer();
            _offlineTimeoutTimer.Interval = TimeSpan.FromMinutes(1);
            _offlineTimeoutTimer.Tick += (s, e) =>
            {
                StopOfflineMode();
                _pendingActions.Clear();

                ConnectionLost?.Invoke();
            };
        }

        private void CreateClient()
        {
            _callbackHandler = new ClientCallbackHandler();
            var context = new InstanceContext(_callbackHandler);

            var client = new LotteryServiceClient(context);
            _client = client;

            if (_client is ICommunicationObject channel)
            {
                channel.Faulted += OnConnectionLost;
                channel.Closed += OnConnectionLost;
            }
        }

        public void EnqueueAction(Func<Task> action)
        {
            _pendingActions.Enqueue(action);

            if (!IsOfflineMode)
            {
                IsOfflineMode = true;
                _offlineTimeoutTimer.Start();
                _reconnectTimer.Start();
            }
        }

        private async Task TryFlushQueueAsync()
        {
            if (_pendingActions.Count == 0) return;

            try
            {
                var action = _pendingActions.Peek();

                var forceCheck = Client;

                await action();

                _pendingActions.Dequeue();

                if (_pendingActions.Count == 0)
                {
                    StopOfflineMode();
                }
            }
            catch (Exception)
            {
            }
        }

        private void StopOfflineMode()
        {
            IsOfflineMode = false;
            _offlineTimeoutTimer.Stop();
            _reconnectTimer.Stop();
        }

        public void Reconnect()
        {
            CloseSafe();
            CreateClient();
        }

        public void CloseSafe()
        {
            StopOfflineMode();
            _pendingActions.Clear();

            if (_client == null)
            {
                return;
            }

            var channel = _client as ICommunicationObject;
            if (channel == null)
            {
                return;
            }

            try
            {
                channel.Faulted -= OnConnectionLost;
                channel.Closed -= OnConnectionLost;

                if (channel.State == CommunicationState.Faulted)
                {
                    channel.Abort();
                }
                else
                {
                    try
                    {
                        channel.Close();
                    }
                    catch
                    {
                        channel.Abort();
                    }
                }
            }
            catch
            {
                channel.Abort();
            }
            finally
            {
                _client = null;
            }
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            if (!IsOfflineMode)
            {
                ConnectionLost?.Invoke();
            }
        }
    }
}