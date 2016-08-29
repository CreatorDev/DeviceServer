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
using System.Runtime.Serialization;
using System.IO;

namespace Imagination.Model
{
    public class NotificationParameters
    {
        public int? MinimumPeriod { get; set; }

        public int? MaximumPeriod { get; set; }

        public double? GreaterThan { get; set; }

        public double? LessThan { get; set; }

        public double? Step { get; set; }

        public void Serialise(Stream stream)
        {
            IPCHelper.Write(stream, MinimumPeriod);
            IPCHelper.Write(stream, MaximumPeriod);
            IPCHelper.Write(stream, GreaterThan);
            IPCHelper.Write(stream, LessThan);
            IPCHelper.Write(stream, Step);
            stream.Flush();
        }

        public static NotificationParameters Deserialise(Stream stream)
        {            
            NotificationParameters result = new NotificationParameters();
            result.MinimumPeriod = IPCHelper.ReadInt32Nullable(stream);
            result.MaximumPeriod = IPCHelper.ReadInt32Nullable(stream);
            result.GreaterThan = IPCHelper.ReadDoubleNullable(stream);
            result.LessThan = IPCHelper.ReadDoubleNullable(stream);
            result.Step = IPCHelper.ReadDoubleNullable(stream);           
            return result;
        }

    }
}
