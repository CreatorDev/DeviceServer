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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Imagination.Tools.APIDocGenerator
{
    public static class SerialisationUtils
    {
        public static string PrettifyHttpStatusCode(HttpStatusCode statusCode)
        {
            string[] parts = Regex.Split(statusCode.ToString(), @"(?<!^)(?=[A-Z])");
            StringBuilder result = new StringBuilder();

            string lastPart = null;
            foreach (string part in parts)
            {
                if (lastPart != null && lastPart.Length > 1)
                    result.Append(" ");
                result.Append(part);
                lastPart = part;
            }
            return result.ToString();
        }

        public static List<string> SplitLines(string text)
        {
            return text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        }

        public static bool IsStandardDataExchangeFormat(TDataExchangeFormat dataExchangeFormat)
        {
            return dataExchangeFormat == TDataExchangeFormat.Json || dataExchangeFormat == TDataExchangeFormat.Xml;
        }

        public static TDataExchangeFormat GetDataExchangeFormatFromContentType(string contentType)
        {
            TDataExchangeFormat format = TDataExchangeFormat.None;
            if (contentType.Equals("application/x-www-form-urlencoded"))
            {
                format = TDataExchangeFormat.FormUrlEncoded;
            }
            else if (contentType.EndsWith("+json"))
            {
                format = TDataExchangeFormat.Json;
            }
            else if (contentType.EndsWith("+xml"))
            {
                format = TDataExchangeFormat.Xml;
            }
            return format;
        }

        internal static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode < 300;
        }
    }
}
