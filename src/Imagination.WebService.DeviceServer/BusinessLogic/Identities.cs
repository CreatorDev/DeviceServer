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

using DTLS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Imagination.Model;
using Imagination.DataAccess;

namespace Imagination.BusinessLogic
{
    public class Identities
    {
        private CertificateInfo _IssuerCA;

        public Identities()
        {
            if (File.Exists("CA.pem"))
                _IssuerCA = Certificates.GetCertificateInfo(File.ReadAllBytes("CA.pem"), DTLS.TCertificateFormat.PEM);
        }

        public string CreateCertificate(int organisationID)
        {
            CertificateSubject subject = new CertificateSubject()
            {
                CommonName = Guid.NewGuid().ToString(),
                Organistion = organisationID.ToString()
            };
            DateTime startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDate = startDate.AddYears(3);
            byte[] certificate = Certificates.GenerateCertificate(subject, _IssuerCA, startDate, endDate, new SignatureHashAlgorithm() { Hash = THashAlgorithm.SHA256, Signature = TSignatureAlgorithm.ECDSA }, DTLS.TCertificateFormat.PEM);
            return System.Text.Encoding.UTF8.GetString(certificate);
        }

        public void SavePSKIdentity(PSKIdentity pskIdentity, TObjectState state)
        {
            if (pskIdentity.OrganisationID == 0)
            {
                throw new NotSupportedException();
            }
            if (state == TObjectState.Add)
            {
                pskIdentity.Identity = StringUtils.GuidEncode(Guid.NewGuid());

                byte[] secretkey = new Byte[32];
                System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                rng.GetBytes(secretkey);
                pskIdentity.Secret = StringUtils.HexString(secretkey);
            }
            DataAccessFactory.Identities.SavePSKIdentity(pskIdentity, state);
        }

        public PSKIdentity GetPSKIdentity(string identity)
        {
            return DataAccessFactory.Identities.GetPSKIdentity(identity);
        }

        public List<PSKIdentity> GetPSKIdentities(int organisationID)
        {
            return DataAccessFactory.Identities.GetPSKIdentities(organisationID);
        }
    }
}
