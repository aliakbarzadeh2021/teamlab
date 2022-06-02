using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;
using System.Xml.Linq;
using log4net;

namespace ASC.Core.Billing
{
    class BillingClient : ClientBase<IService>, IDisposable
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(TariffService));


        public BillingClient()
        {
        }

        public XElement GetLatestActiveResource(int tenant)
        {
            var request = new Message { Type = MessageType.Data, Content = string.Format("<GetLatestActiveResource><PortalId>{0}</PortalId></GetLatestActiveResource>", tenant) };
            return Request(request);
        }

        public XElement GetPayments(int tenant)
        {
            var request = new Message { Type = MessageType.Data, Content = string.Format("<GetPayments><PortalId>{0}</PortalId></GetPayments>", tenant) };
            return Request(request);
        }

        private XElement Request(Message request)
        {
            log.Info("Billing service request: " + request.Type.ToString() + " " + request.Content);
            
            var responce = Channel.Request(request);
            
            log.Info("Billing service responce: " + responce.Type.ToString() + " " + responce.Content);

            if (responce.Type == MessageType.Data)
            {
                var xml = responce.Content;
                var invalidChar = ((char)65279).ToString();
                if (xml.Contains(invalidChar))
                {
                    xml = xml.Replace(invalidChar, string.Empty);
                }
                if (xml.Contains("&"))
                {
                    xml = HttpUtility.HtmlDecode(xml);
                }
                return XElement.Parse(xml);
            }
            else
            {
                throw new Exception(responce.Content);
            }
        }

        void IDisposable.Dispose()
        {
            try
            {
                Close();
            }
            catch (CommunicationException)
            {
                Abort();
            }
            catch (TimeoutException)
            {
                Abort();
            }
            catch (Exception)
            {
                Abort();
                throw;
            }
        }
    }


    [ServiceContract]
    interface IService
    {
        [OperationContract]
        Message Request(Message message);
    }

    [DataContract(Name = "Message", Namespace = "http://schemas.datacontract.org/2004/07/teamlabservice")]
    [Serializable]
    class Message
    {
        [DataMember]
        public string Content
        {
            get;
            set;
        }

        [DataMember]
        public MessageType Type
        {
            get;
            set;
        }
    }

    [DataContract(Name = "MessageType", Namespace = "http://schemas.datacontract.org/2004/07/teamlabservice")]
    enum MessageType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        Data = 1,

        [EnumMember]
        Error = 2,
    }
}
