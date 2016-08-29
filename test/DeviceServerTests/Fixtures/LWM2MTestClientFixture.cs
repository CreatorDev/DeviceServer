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
using Imagination.LWM2M;
using System.Reflection;
using DeviceServerTests.Utilities;

namespace DeviceServerTests.Fixtures
{
    public class LWM2MTestClientFixture : IDisposable
    {
        public Client Client { get; private set; }

        public LWM2MTestClientFixture()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine(string.Concat("TestClient Version - ", fileVersionInfo.FileVersion));
            CoAP.Log.LogManager.Level = CoAP.Log.LogLevel.None;
            Client = new Client(55863);
            Client.LoadDefaultResources();
            Client.Start();


            // TODO PSK and Certificate versions
            Client.UsePSK(TestConfiguration.TestData.Identity.PSK.Key, TestConfiguration.TestData.Identity.PSK.Secret);

            // FIXME: Should access API for bootstrap servers and PSK/Certificate
            Client.ConnectToServer(TestConfiguration.TestData.LWM2MClient.URI);

        }

        public void Dispose()
        {
            Client.Stop();
        }

        
    }
}
