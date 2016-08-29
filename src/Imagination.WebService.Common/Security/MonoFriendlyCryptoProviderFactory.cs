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

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;

namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// Workaround  for Mono because it cannot load assembly for AsymmetricSignatureProvider which has windows specific marshalling
    /// </summary>
    public class MonoFriendlyCryptoProviderFactory : CryptoProviderFactory
    {
        private readonly ILogger _Logger;

        public MonoFriendlyCryptoProviderFactory(ILogger logger)
        {
            _Logger = logger;
        }

        public override SignatureProvider CreateForSigning(SecurityKey key, string algorithm)
        {
            return CreateProvider(key, algorithm, true);
        }

        public override SignatureProvider CreateForVerifying(SecurityKey key, string algorithm)
        {
            return CreateProvider(key, algorithm, false);
        }

        private SignatureProvider CreateProvider(SecurityKey key, string algorithm, bool willCreateSignatures)
        {
            _Logger?.LogDebug($"Creating {algorithm} provider for {key.KeyId} for {(willCreateSignatures ? "signing" : "verifying")}");
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(algorithm))
                throw new ArgumentNullException(nameof(algorithm));

            //AsymmetricSecurityKey asymmetricSecurityKey = key as AsymmetricSecurityKey;
            //if (asymmetricSecurityKey != null)
            //    return new AsymmetricSignatureProvider(asymmetricSecurityKey, algorithm, willCreateSignatures, this.AsymmetricAlgorithmResolver);
            SymmetricSecurityKey symmetricSecurityKey = key as SymmetricSecurityKey;
            if (symmetricSecurityKey != null)
                return new SymmetricSignatureProvider(symmetricSecurityKey, algorithm);
            JsonWebKey jsonWebKey = key as JsonWebKey;
            if (jsonWebKey != null && jsonWebKey.Kty != null)
            {
                //if (jsonWebKey.Kty == "RSA" || jsonWebKey.Kty == "EC")
                //    return new AsymmetricSignatureProvider(key, algorithm, willCreateSignatures, this.AsymmetricAlgorithmResolver);
                if (jsonWebKey.Kty == "oct")
                    return new SymmetricSignatureProvider(key, algorithm);
            }
            throw new ArgumentException($"{typeof(SignatureProvider)} supports: '{typeof(SecurityKey)}' of types: '{typeof(AsymmetricSecurityKey)}' or '{typeof(AsymmetricSecurityKey)}'. SecurityKey received was of type: '{key.GetType()}'.");
        }
    }
}
