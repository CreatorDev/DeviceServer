using Imagination.DataAccess;
using Imagination.Model;
using System;
using System.Collections.Concurrent;

namespace Imagination.BusinessLogic
{
    internal class Identities
    {
        public PSKIdentity GetPSKIdentity(string identity)
        {
            return DataAccessFactory.Identities.GetPSKIdentity(identity);
        }
    }
}
