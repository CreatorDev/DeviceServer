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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Imagination
{
    public class Security
    {

        public static int CurrentOrganisationID
        {
            get
            {
                int result = 0;
                OrganisationIdentity organisationIdentity = CurrentOrganisation;
                if (organisationIdentity != null)
                    return result;
                return result;
            }
        }

        public static OrganisationIdentity CurrentOrganisation
        {
            get { return Thread.CurrentPrincipal?.Identity as OrganisationIdentity; }
        }

        public static string GetSalt(int base64Length = 128)
        {
            int base256Length = base64Length - base64Length % 4 - base64Length / 4;
            byte[] buffer = new byte[base256Length];
            Random rnd = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)rnd.Next(256);
            }
            return Convert.ToBase64String(buffer);
        }

        public static string EncodePassword(string password, string passwordSalt)
        {
            byte[] bRet = null;
            byte[] bIn = Encoding.Unicode.GetBytes(password);
            byte[] bSalt = Convert.FromBase64String(passwordSalt);
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            
            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            HashAlgorithm hashAlgorithm = HashAlgorithm.Create("SHA1");
            bRet = hashAlgorithm.ComputeHash(bAll);

            return Convert.ToBase64String(bRet);
        }

    }
}
