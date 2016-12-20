using Imagination.DataAccess;
using Imagination.LWM2M;
using Imagination.Model;
using Imagination.Model.Subscriptions;
using Imagination.ServiceModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Imagination.BusinessLogic
{
    internal class Subscriptions
    {
        private const int MAX_POST_ATTEMPTS = 10;
        private const string ACCEPT_TYPE = "*/*";
        public Subscriptions()
        {

        }

        public void StartListening()
        {
            BusinessLogicFactory.ServiceMessages.Subscribe(String.Concat("Webhook.", RouteKeys.SUBSCRIPTION_NOTIFICATION), RouteKeys.SUBSCRIPTION_NOTIFICATION, new DataAccess.MessageArrivedEventHandler(OnSubscriptionNotify));            
        }

        private static async void OnSubscriptionNotify(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed OnSubscriptionNotify message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            string url = (string)message.Parameters["Url"];
            string clientID = (string)message.Parameters["ClientID"];

            Model.WebhookNotification notification = new Model.WebhookNotification();
            MemoryStream stream = new MemoryStream(16384);
            new ServiceModels.WebhookNotification(notification, message).Serialise(stream);
            string payload = new StreamReader(stream).ReadToEnd();

            bool retry = message.Parameters.ContainsKey("RequeueCount");
            long requeueCount = retry ? (long)message.Parameters["RequeueCount"] : 0;
            bool dropMessage = false;

#if DEBUG
            Console.WriteLine($"Sending payload to {url}: \n" + payload);
#endif

            try
            {
                RESTClient.RESTResponse response = await RESTClient.PostAsync(url, ACCEPT_TYPE, null, notification.AcceptContentType, payload);

                switch ((HttpStatusCode)response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        ApplicationEventLog.Write(LogLevel.Information, $"Webhook post notification from client {clientID} to {url} successful.");
                        break;
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.MethodNotAllowed:
                        // No retry for these failures
                        ApplicationEventLog.Write(LogLevel.Warning, $"Non-recoverable HTTP Status code from{url}:{response.StatusCode}, discarding message.");
                        dropMessage = true;
                        break;
                    default:
                        ApplicationEventLog.Write(LogLevel.Warning, $"Unexpected HTTP Status code from{url}:{response.StatusCode}");
                        break;
                }
            }
            catch(Exception ex)
            {
                ApplicationEventLog.Write(LogLevel.Error, "Failed to post webhook", ex);
            }

            if ((MAX_POST_ATTEMPTS <= 0 || requeueCount < MAX_POST_ATTEMPTS) && !dropMessage)
            {
                BusinessLogicFactory.ServiceMessages.NackMessage(message);
            }
        }
    }
}
