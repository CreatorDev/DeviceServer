using Imagination.Model;
using System;
using System.Collections.Concurrent;

namespace Imagination.LWM2M
{
    internal class Identities
    {
        public PSKIdentity GetPSKIdentity(string identity)
        {
            return DataAccessFactory.Identities.GetPSKIdentity(identity);
        }
    }
}
