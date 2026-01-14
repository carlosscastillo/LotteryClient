using Lottery.LotteryServiceReference;
using System;
using System.ServiceModel;

namespace Lottery
{
    public class ServiceProxy
    {
        private static ServiceProxy _instance;
        public static ServiceProxy Instance => _instance ?? (_instance = new ServiceProxy());

        public ILotteryService Client { get; private set; }
        private ClientCallbackHandler _callbackHandler;

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
            Client = client;

            if (Client is ICommunicationObject channel)
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
            var channel = Client as ICommunicationObject;
            if (channel == null)
            {
                return;
            }

            try
            {
                channel.Faulted -= OnConnectionLost;
                channel.Closed -= OnConnectionLost;

                if (channel.State == CommunicationState.Faulted || channel.State == CommunicationState.Closed)
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
            catch (Exception)
            {
                channel.Abort();
            }
            finally
            {
                Client = null;
            }
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            ConnectionLost?.Invoke();
        }
    }
}