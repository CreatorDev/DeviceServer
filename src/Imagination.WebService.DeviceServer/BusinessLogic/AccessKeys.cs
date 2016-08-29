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
using Imagination.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.BusinessLogic
{
    internal class AccessKeys
    {
        public AccessKey GetAccessKey(string key)
        {
            return DataAccessFactory.AccessKeys.GetAccessKey(key);
        }

        public List<AccessKey> GetAccessKeys(int organisationID)
        {
            return DataAccessFactory.AccessKeys.GetAccessKeys(organisationID);
        }

        public void SaveAccessKey(AccessKey accessKey, TObjectState state)
        {
            if (accessKey.OrganisationID == 0)
            {
                accessKey.OrganisationID = DataAccessFactory.AccessKeys.GenerateOrganisationID();
            }
            if ((state == TObjectState.Add) && string.IsNullOrEmpty(accessKey.Key))
            {
                byte[] secretkey = new Byte[64];
                System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                Model.AccessKey existingTenantAccessKey;
                do
                {
                    rng.GetBytes(secretkey);
                    accessKey.Key = StringUtils.Encode(secretkey);
                    existingTenantAccessKey = DataAccessFactory.AccessKeys.GetAccessKey(accessKey.Key);
                } while (existingTenantAccessKey != null);
                rng.GetBytes(secretkey);
                accessKey.Secret = StringUtils.Encode(secretkey);
            }
            DataAccessFactory.AccessKeys.SaveAccessKey(accessKey, state);
        }

    }
}
