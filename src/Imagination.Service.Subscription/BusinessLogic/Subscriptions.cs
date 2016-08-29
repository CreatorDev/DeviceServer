using Imagination.DataAccess;
using Imagination.LWM2M;
using Imagination.Model;
using Imagination.Model.Subscriptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.BusinessLogic
{
    internal class Subscriptions
    {

        public Subscriptions()
        {
        }

        public void StartListening()
        {
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.SUBSCRIPTION_CREATE), RouteKeys.SUBSCRIPTION_CREATE, new DataAccess.MessageArrivedEventHandler(OnCreateSubscription));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.SUBSCRIPTION_UPDATE), RouteKeys.SUBSCRIPTION_UPDATE, new DataAccess.MessageArrivedEventHandler(OnUpdateSubscription));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.SUBSCRIPTION_DELETE), RouteKeys.SUBSCRIPTION_DELETE, new DataAccess.MessageArrivedEventHandler(OnDeleteSubscription));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.OBSERVATION_NOTIFICATION), RouteKeys.OBSERVATION_NOTIFICATION, new DataAccess.MessageArrivedEventHandler(OnObservationNotify));

            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.CLIENT_CONNECTED), RouteKeys.CLIENT_CONNECTED, new DataAccess.MessageArrivedEventHandler(OnClientConnected));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.CLIENT_UPDATE), RouteKeys.CLIENT_UPDATE, new DataAccess.MessageArrivedEventHandler(OnClientUpdate));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.CLIENT_DISCONNECTED), RouteKeys.CLIENT_DISCONNECTED, new DataAccess.MessageArrivedEventHandler(OnClientDisconnected));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("Subscription.", RouteKeys.CLIENT_CONNECTION_EXPIRED), RouteKeys.CLIENT_CONNECTION_EXPIRED, new DataAccess.MessageArrivedEventHandler(OnClientConnectionExpired));
        }

        private static Subscription GetSubscriptionFromMessage(ServiceEventMessage message)
        {
            return DataAccessFactory.Subscriptions.GetSubscription(StringUtils.GuidDecode((string)message.Parameters["SubscriptionID"]));
        }

        private static void RestartObservations(ServiceEventMessage message)
        {
            Guid clientID = StringUtils.GuidDecode((string)message.Parameters["ClientID"]);
#if DEBUG
            Console.WriteLine($"Restarting observations for client {clientID}");
#endif
            List<Subscription> subscriptions = DataAccessFactory.Subscriptions.GetSubscriptions(clientID);
            foreach (Subscription subscription in subscriptions)
            {
                if (subscription.SubscriptionType == TSubscriptionType.Observation)
                {
                    Observe(subscription);
                }
            }
        }

        private static void Observe(Subscription subscription)
        {
            Client client = DataAccessFactory.Clients.GetClient(subscription.ClientID);
            if (client != null)
            {
                if (subscription.PropertyDefinitionID != Guid.Empty)
                {
                    if (subscription.NotificationParameters != null)
                    {
                        if (DataAccessFactory.Servers.SetNotificationParameters(client, subscription.ObjectDefinitionID, subscription.ObjectID, subscription.PropertyDefinitionID, subscription.NotificationParameters))
                            ApplicationEventLog.Write(LogLevel.Information, $"Updated notification parameters for resource /{subscription.ObjectDefinitionID}/{subscription.ObjectID}/{subscription.PropertyDefinitionID}");
                        else
                            ApplicationEventLog.Write(LogLevel.Warning, $"Failed to update notification parameters for resource /{subscription.ObjectDefinitionID}/{subscription.ObjectID}/{subscription.PropertyDefinitionID}");
                    }
                    DataAccessFactory.Servers.ObserveResource(client, subscription.ObjectDefinitionID, subscription.ObjectID, subscription.PropertyDefinitionID);
#if DEBUG
                    ApplicationEventLog.Write(LogLevel.Information, $"Observing resource /{subscription.ObjectDefinitionID}/{subscription.ObjectID}/{subscription.PropertyDefinitionID}");
#endif
                }
                else if (subscription.ObjectID != null)
                {
                    DataAccessFactory.Servers.ObserveObject(client, subscription.ObjectDefinitionID, subscription.ObjectID);
#if DEBUG
                    ApplicationEventLog.Write(LogLevel.Information, $"Observing object /{subscription.ObjectDefinitionID}/{subscription.ObjectID}");
#endif
                }
                else if (subscription.ObjectDefinitionID != Guid.Empty)
                {
                    DataAccessFactory.Servers.ObserveObjects(client, subscription.ObjectDefinitionID);
#if DEBUG
                    ApplicationEventLog.Write(LogLevel.Information, $"Observing objects /{subscription.ObjectDefinitionID}");
#endif
                }
                else
                {
                    ApplicationEventLog.Write(LogLevel.Warning, $"Subscription {subscription.SubscriptionID} has no definition ID");
                }
            }
            else
            {
                ApplicationEventLog.Write(LogLevel.Warning, $"No client exists for {subscription.ClientID} in subscription {subscription.SubscriptionID}");
            }
        }

        private static void CancelObservation(Subscription subscription)
        {
            Client client = DataAccessFactory.Clients.GetClient(subscription.ClientID);
            if (client != null)
            {
                if (subscription.PropertyDefinitionID != Guid.Empty)
                {
                    DataAccessFactory.Servers.CancelObserveResource(client, subscription.ObjectDefinitionID, subscription.ObjectID, subscription.PropertyDefinitionID, false);
#if DEBUG
                    ApplicationEventLog.Write(LogLevel.Information, $"Cancelled observing resource /{subscription.ObjectDefinitionID}/{subscription.ObjectID}/{subscription.PropertyDefinitionID}");
#endif
                }
                else if (subscription.ObjectID != null)
                {
                    DataAccessFactory.Servers.CancelObserveObject(client, subscription.ObjectDefinitionID, subscription.ObjectID, false);
#if DEBUG
                    ApplicationEventLog.Write(LogLevel.Information, $"Cancelled observing object /{subscription.ObjectDefinitionID}/{subscription.ObjectID}");
#endif
                }
                else if (subscription.ObjectDefinitionID != Guid.Empty)
                {
                    DataAccessFactory.Servers.CancelObserveObjects(client, subscription.ObjectDefinitionID, false);
#if DEBUG
                    ApplicationEventLog.Write(LogLevel.Information, $"Cancelled observing objects /{subscription.ObjectDefinitionID}");
#endif
                }
                else
                {
                    ApplicationEventLog.Write(LogLevel.Warning, $"Subscription {subscription.SubscriptionID} has no definition ID");
                }
            }
            else
            {
                ApplicationEventLog.Write(LogLevel.Warning, $"No client exists for {subscription.ClientID} in subscription {subscription.SubscriptionID}");
            }
        }

        private static void OnCreateSubscription(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed create subscription message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            Subscription subscription = GetSubscriptionFromMessage(message);
            if (subscription != null)
            {
                if (subscription.SubscriptionType == TSubscriptionType.Observation)
                {
                    Observe(subscription);
                }
                else
                {
                    // for server event types, we are already subscribed to RabbitMQ service messages.
                }
            }
            else
            {
                ApplicationEventLog.Write(LogLevel.Warning, $"Failed to lookup subscription: {message.Parameters["SubscriptionID"]}");
            }
        }

        private static void OnDeleteSubscription(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed delete subscription message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            Subscription subscription = GetSubscriptionFromMessage(message);
            if (subscription != null)
            {
                if (subscription.SubscriptionType == TSubscriptionType.Observation)
                {
                    CancelObservation(subscription);
                }
                DataAccessFactory.Subscriptions.SaveSubscription(subscription, TObjectState.Delete);
            }
            else
            {
                ApplicationEventLog.Write(LogLevel.Warning, $"Failed to lookup subscription: {message.Parameters["SubscriptionID"]}");
            }
        }

        private static void OnUpdateSubscription(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed update subscription message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            Subscription subscription = GetSubscriptionFromMessage(message);
            if (subscription != null)
            {
                if (subscription.SubscriptionType == TSubscriptionType.Observation)
                {
                    if (subscription.NotificationParameters != null && subscription.PropertyDefinitionID != Guid.Empty) 
                    {
                        Client client = DataAccessFactory.Clients.GetClient(subscription.ClientID);
                        if (client != null) 
                        {
                            if (DataAccessFactory.Servers.SetNotificationParameters(client, subscription.ObjectDefinitionID, subscription.ObjectID, subscription.PropertyDefinitionID, subscription.NotificationParameters))
                                ApplicationEventLog.Write(LogLevel.Information, $"Updated notification parameters for resource /{subscription.ObjectDefinitionID}/{subscription.ObjectID}/{subscription.PropertyDefinitionID}");
                            else
                                ApplicationEventLog.Write(LogLevel.Warning, $"Failed to update notification parameters for resource /{subscription.ObjectDefinitionID}/{subscription.ObjectID}/{subscription.PropertyDefinitionID}");
                        }
                        else
                        {
                            ApplicationEventLog.Write(LogLevel.Warning, $"No client exists for {subscription.ClientID} in subscription {subscription.SubscriptionID}");
                        }
                    }
                }
                DataAccessFactory.Subscriptions.SaveSubscription(subscription, TObjectState.Update);
            }
            else
            {
                ApplicationEventLog.Write(LogLevel.Warning, $"Failed to lookup subscription: {message.Parameters["SubscriptionID"]}");
            }
        }

        private static void NotifySubscribers(ServiceEventMessage message, TSubscriptionType subscriptionType)
        {
            Guid clientID = StringUtils.GuidDecode((string)message.Parameters["ClientID"]);
            int organisationID = (int)((long)message.Parameters["OrganisationID"]);
            Model.Object changedObject = null;

            List<Subscription> subscriptions = null;
            if (subscriptionType == TSubscriptionType.Observation)
            {
                changedObject = (Model.Object)message.Parameters["Object"];
                subscriptions = DataAccessFactory.Subscriptions.GetSubscriptions(clientID);
            }
            else
                subscriptions = DataAccessFactory.Subscriptions.GetSubscriptions(organisationID);

            foreach (Subscription subscription in subscriptions)
            {
                if (subscription.SubscriptionType == subscriptionType)
                {
                    if (subscriptionType == TSubscriptionType.Observation)
                    {
                        if (subscription.ObjectDefinitionID == changedObject.ObjectDefinitionID)
                        {
                            if (subscription.ObjectID == null || subscription.ObjectID == changedObject.InstanceID)
                            {
                                if (subscription.PropertyDefinitionID == Guid.Empty || changedObject.ContainsPropertyWithDefinitionID(subscription.PropertyDefinitionID))
                                {
                                    NotifySubscriber(clientID, subscription, changedObject);
                                }
                            }
                        }
                    }
                    else
                        NotifySubscriber(clientID, subscription, changedObject);
                }
            }
        }

        private static void NotifySubscriber(Guid clientID, Subscription subscription, Model.Object changedObject)
        {
#if DEBUG
            Console.WriteLine("Publishing Subscription.Webhook message for subscription " + subscription.SubscriptionID);
#endif
            ServiceEventMessage message = new ServiceEventMessage();
            message.AddParameter("AcceptContentType", subscription.AcceptContentType);
            message.AddParameter("SubscriptionID", StringUtils.GuidEncode(subscription.SubscriptionID));
            message.AddParameter("SubscriptionType", subscription.SubscriptionType.ToString());
            message.AddParameter("ClientID", StringUtils.GuidEncode(clientID));
            message.AddParameter("Url", subscription.Url);
            message.AddParameter("TimeTriggered", DateTime.Now);
            if (changedObject != null)
                message.AddParameter("Object", changedObject);
            BusinessLogicFactory.ServiceMessages.Publish(RouteKeys.SUBSCRIPTION_NOTIFICATION, message, TMessagePublishMode.Confirms);
        }

        private static void OnClientConnected(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed OnClientConnected message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            RestartObservations(message);
            NotifySubscribers(message, TSubscriptionType.ClientConnected);
        }

        private static void OnClientUpdate(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed OnClientUpdate message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            NotifySubscribers(message, TSubscriptionType.ClientUpdated);
        }

        private static void OnClientDisconnected(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed OnClientDisconnected message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            NotifySubscribers(message, TSubscriptionType.ClientDisconnected);
        }

        private static void OnClientConnectionExpired(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed OnClientConnectionExpired message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            NotifySubscribers(message, TSubscriptionType.ClientConnectionExpired);
        }

        private static void OnObservationNotify(string server, ServiceEventMessage message)
        {
#if DEBUG
            Console.WriteLine("Consumed OnObservationNotify message");
            Console.WriteLine("Server: " + server);
            Console.WriteLine("Message: " + message.ToString());
#endif
            NotifySubscribers(message, TSubscriptionType.Observation);
        }
            
    }
}
