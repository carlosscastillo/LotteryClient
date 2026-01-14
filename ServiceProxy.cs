using Lottery.LotteryServiceReference;
using System;
using System.ServiceModel;

namespace Lottery
{
    public class ServiceProxy
    {
        private static ServiceProxy _instance;
        public static ServiceProxy Instance => _instance ?? (_instance = new ServiceProxy());

        private ILotteryService _client;
        private ClientCallbackHandler _callbackHandler;

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

        public void Reconnect()
        {
            CloseSafe();
            CreateClient();
        }

        public void CloseSafe()
        {
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
            ConnectionLost?.Invoke();
        }
    }
}