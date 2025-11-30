using Lottery.LotteryServiceReference;
using System.ServiceModel;

namespace Lottery
{
    public class ServiceProxy
    {
        private static ServiceProxy _instance;
        public static ServiceProxy Instance => _instance ?? (_instance = new ServiceProxy());

        public ILotteryService Client { get; private set; }
        private ClientCallbackHandler _callbackHandler;

        private ServiceProxy()
        {
            CreateClient();
        }

        private void CreateClient()
        {
            _callbackHandler = new ClientCallbackHandler();
            var context = new InstanceContext(_callbackHandler);
            Client = new LotteryServiceClient(context);
        }

        public void Reconnect()
        {
            CloseSafe();
            CreateClient();
        }

        public void CloseSafe()
        {
            if (Client == null)
            {
                return;
            }

            var channel = Client as ICommunicationObject;
            if (channel == null)
            {
                return;
            }

            try
            {
                if (channel.State == CommunicationState.Faulted)
                {
                    channel.Abort();
                }
                else
                {
                    channel.Close();
                }
            }
            catch
            {
                channel.Abort();
            }
        }
    }
}