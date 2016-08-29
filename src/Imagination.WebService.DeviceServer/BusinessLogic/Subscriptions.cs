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

using Imagination.DataAccess;
using Imagination.LWM2M;
using Imagination.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.BusinessLogic
{
    internal class Subscriptions
    {
        public Subscription GetSubscription(Guid subscriptionID)
        {
            return DataAccessFactory.Subscriptions.GetSubscription(subscriptionID);
        }

        public List<Subscription> GetSubscriptions(int organisationID)
        {
            return DataAccessFactory.Subscriptions.GetSubscriptions(organisationID);
        }

        public List<Subscription> GetSubscriptions(Guid clientID)
        {
            return DataAccessFactory.Subscriptions.GetSubscriptions(clientID);
        }

        public void SaveSubscription(Subscription subscription, TObjectState state)
        {
            ServiceEventMessage message = new ServiceEventMessage();
            message.AddParameter("SubscriptionID", StringUtils.GuidEncode(subscription.SubscriptionID));

            switch (state)
            {
                case TObjectState.Add:
                    DataAccessFactory.Subscriptions.SaveSubscription(subscription, state);
                    BusinessLogicFactory.ServiceMessages.Publish(RouteKeys.SUBSCRIPTION_CREATE, message, TMessagePublishMode.Confirms);
                    break;
                case TObjectState.Update:
                    DataAccessFactory.Subscriptions.SaveSubscription(subscription, state);
                    BusinessLogicFactory.ServiceMessages.Publish(RouteKeys.SUBSCRIPTION_UPDATE, message, TMessagePublishMode.Confirms);
                    break;
                case TObjectState.Delete:
                    BusinessLogicFactory.ServiceMessages.Publish(RouteKeys.SUBSCRIPTION_DELETE, message, TMessagePublishMode.Confirms);
                    break;
                default:
                    DataAccessFactory.Subscriptions.SaveSubscription(subscription, state);
                    break;
            }
        }
    }
}
