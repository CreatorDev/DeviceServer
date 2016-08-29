/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Imagination.DataAccess;
using Imagination.Model;

namespace Imagination.BusinessLogic
{
    public class ServiceMessages
    {
		protected class Subscription
        {
            public bool Durable { get; set; }
            public string QueueName { get; set; }
            public string RoutingKey { get; set; }
            public MessageArrivedEventHandler Handler { get; set; }
            public bool Temporary { get; set; }
        }

        protected DALRabbitMQ _DALRabbitMQ;
		protected List<Subscription> _Subscriptions;


		public ServiceMessages()
        {
            _Subscriptions = new List<Subscription>();
        }

        public void AckMessage(ServiceEventMessage message)
        {
            CheckRabbitMQ();
            _DALRabbitMQ.AckMessage(message);
        }

		protected virtual void CheckRabbitMQ()
        {
            if (_DALRabbitMQ == null)
            {
                lock (this)
                {
                    if (_DALRabbitMQ == null)
                    {
						_DALRabbitMQ = new DALRabbitMQ(ServiceConfiguration.RabbitMQConnections);
                        if (_Subscriptions.Count > 0)
                        {
                            lock (_Subscriptions)
                            {
                                foreach (Subscription item in _Subscriptions)
                                {
                                    _DALRabbitMQ.Subscribe(item.QueueName, item.Durable, item.Temporary, item.RoutingKey, item.Handler);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void NackMessage(ServiceEventMessage message, TimeSpan? delayQueueingFor = null)
        {
            CheckRabbitMQ();
            if (delayQueueingFor.HasValue)
                _DALRabbitMQ.NackMessage(message, delayQueueingFor.Value);
            else
                _DALRabbitMQ.NackMessage(message);
        }

        public void Publish(string routingKey, TMessagePublishMode publishMode)
        {
            CheckRabbitMQ();
            ServiceEventMessage message = new ServiceEventMessage();
			if (Security.CurrentOrganisation != null && !message.Parameters.ContainsKey("OrganisationID"))
				message.Parameters.Add("OrganisationID", Security.CurrentOrganisation.OrganisationID);
            _DALRabbitMQ.Publish(routingKey, message, publishMode);
        }

        public void Publish(string routingKey, ServiceEventMessage message, TMessagePublishMode publishMode)
        {
            CheckRabbitMQ();
            if (Security.CurrentOrganisation != null && !message.Parameters.ContainsKey("OrganisationID"))
                message.Parameters.Add("OrganisationID", Security.CurrentOrganisation.OrganisationID);
            _DALRabbitMQ.Publish(routingKey, message, publishMode);
        }

		public void Publish(string routingKey, ServiceEventMessage message, int organisationID, string locale, TMessagePublishMode publishMode)
		{
            CheckRabbitMQ();
			if (!message.Parameters.ContainsKey("OrganisationID"))
			{
                message.Parameters.Add("OrganisationID", organisationID);
            }
			_DALRabbitMQ.Publish(routingKey, message, publishMode);
		}

        public void Start()
        {
            CheckRabbitMQ();
        }

        public void Stop()
        {
            DALRabbitMQ dalRabbitMQ = _DALRabbitMQ;
            if (dalRabbitMQ != null)
            {
                _DALRabbitMQ = null;
                dalRabbitMQ.Dispose();
                dalRabbitMQ = null;
            }
        }

        public void Subscribe(string queueName, string routingKey, MessageArrivedEventHandler handler)
        {
            Subscribe(false, queueName, true, false, routingKey, handler);
        }

        public void Subscribe(string queueName, bool durable, bool temporary, string routingKey, MessageArrivedEventHandler handler)
        {
            Subscribe(false, queueName, durable, temporary, routingKey, handler);
        }

        public void Subscribe(bool machineSpecific, string queueName, bool durable, bool temporary, string routingKey, MessageArrivedEventHandler handler)
        {
            CheckRabbitMQ();
            if (machineSpecific)
                queueName = string.Concat(Environment.MachineName, ".", queueName);
            if (temporary)
                queueName = string.Concat(queueName, ".", Guid.NewGuid().ToString());
            _DALRabbitMQ.Subscribe(queueName, durable, temporary, routingKey, handler);
            lock (_Subscriptions)
            {
                _Subscriptions.Add(new Subscription() { Durable = durable, Handler = handler, QueueName = queueName, RoutingKey = routingKey, Temporary = temporary });
            }
        }

		public void UnSubscribe(string queueName)
		{
			List<string> queueNames = new List<string>() { queueName };
			UnSubscribe(queueNames);
		}

		public void UnSubscribe(List<string> queueNames)
		{
			CheckRabbitMQ();
			_DALRabbitMQ.UnSubscribe(queueNames);
		}

		public void ReSubscribe(string queueName)
		{
			List<string> queueNames = new List<string>() { queueName };
			ReSubscribe(queueNames);
		}

		public void ReSubscribe(List<string> queueNames)
		{
			CheckRabbitMQ();
			_DALRabbitMQ.ReSubscribe(queueNames);
		}

    }
}
