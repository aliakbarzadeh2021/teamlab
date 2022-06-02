using System;
using System.ServiceModel;

namespace ASC.Core.Notify.Jabber
{
    class JabberServiceClient
    {
        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(2);

        private static DateTime lastErrorTime = default(DateTime);

        private static bool IsServiceProbablyNotAvailable()
        {
            return lastErrorTime != default(DateTime) && lastErrorTime + timeout > DateTime.Now;
        }


        public bool SendMessage(string to, string subject, string text, int tenantId)
        {
            if (IsServiceProbablyNotAvailable()) return false;

            using (var service = new JabberServiceClientWcf())
            {
                try
                {
                    service.SendMessage(to, subject, text, tenantId);
                    return true;
                }
                catch (FaultException) { throw; }
                catch (CommunicationException) { lastErrorTime = DateTime.Now; }
                catch (TimeoutException) { lastErrorTime = DateTime.Now; }
            }

            return false;
        }
    }
}
