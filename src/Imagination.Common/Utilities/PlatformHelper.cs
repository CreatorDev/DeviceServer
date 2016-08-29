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

namespace Imagination
{
    public static class PlatformHelper
    {
        private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 30000;
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;

        private static volatile int s_processorCount;
        private static volatile int s_lastProcessorCountRefreshTicks;

        public static int ProcessorCount
        {
            get
            {
                int tickCount = Environment.TickCount;
                int num = PlatformHelper.s_processorCount;
                if (num == 0 || tickCount - PlatformHelper.s_lastProcessorCountRefreshTicks >= PROCESSOR_COUNT_REFRESH_INTERVAL_MS)
                {
                    num = (PlatformHelper.s_processorCount = Environment.ProcessorCount);
                    PlatformHelper.s_lastProcessorCountRefreshTicks = tickCount;
                }
                return num;
            }
        }
        public static bool IsSingleProcessor
        {
            get
            {
                return PlatformHelper.ProcessorCount == 1;
            }
        }

        public static int DefaultConcurrencyLevel
        {
            get
            {
                return DEFAULT_CONCURRENCY_MULTIPLIER * PlatformHelper.ProcessorCount;
            }
        }

    }
}
