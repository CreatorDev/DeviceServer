using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.Model.Subscriptions
{
    public enum TSubscriptionType
    {
        NotSet = 0,

        // LWM2M Observations
        Observation = 1,

        // Server Event Notifications
        ClientConnected = 2,
        ClientDisconnected = 3,
        ClientUpdated = 4,
        ClientConnectionExpired = 5,
    }
}
