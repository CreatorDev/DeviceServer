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
using MongoDB.Bson;

namespace Imagination
{
	public class BsonHelper
	{
		public static bool GetBoolean(BsonDocument doc, string propertyName)
		{
			bool result = false;
			if (doc.Contains(propertyName))
				result = doc[propertyName].AsBoolean;
			return result;
		}

		public static DateTime GetDateTime(BsonDocument doc, string propertyName)
		{
			DateTime result = DateTime.MinValue;
			if (doc.Contains(propertyName))
				result = doc[propertyName].ToUniversalTime();
			return result;
		}

		public static DateTime? GetNullableDateTime(BsonDocument doc, string propertyName)
		{
			DateTime? result = null;
			if (doc.Contains(propertyName))
				result = doc[propertyName].ToUniversalTime();
			return result;
		}

		public static int GetInt32(BsonDocument doc, string propertyName)
		{
			int result = 0;
			if (doc.Contains(propertyName))
				result = doc[propertyName].AsInt32;
			return result;
		}

		public static long GetInt64(BsonDocument doc, string propertyName)
		{
			long result = 0;
			if (doc.Contains(propertyName))
				result = doc[propertyName].AsInt64;
			return result;
		}

		public static Guid GetGuid(BsonDocument doc, string propertyName)
		{
			Guid result = Guid.Empty;
			if (doc.Contains(propertyName))
				result = new Guid(doc[propertyName].AsByteArray);
			return result;
		}

		public static int? GetInteger(BsonDocument doc, string propertyName)
		{
			int? result = null;
			if (doc.Contains(propertyName))
				result = doc[propertyName].AsInt32;
			return result;
		}

		public static long? GetLong(BsonDocument doc, string propertyName)
		{
			long? result = null;
			if (doc.Contains(propertyName))
				result = doc[propertyName].AsInt64;
			return result;
		}

		public static string GetString(BsonDocument doc, string propertyName)
		{
			string result = null;
			if (doc.Contains(propertyName))
				result = doc[propertyName].AsString;
			return result;
		}

        public static BsonArray GetArray(BsonDocument doc, string propertyName)
        {
            BsonArray result = null;
            if (doc.Contains(propertyName))
                result = doc[propertyName].AsBsonArray;
            return result;
        }

        public static void SetValue(BsonDocument doc, string propertyName, bool value)
		{
			doc[propertyName] = value;
		}

		public static void SetValue(BsonDocument doc, string propertyName, DateTime value)
		{
			doc[propertyName] = value;
		}

		public static void SetValue(BsonDocument doc, string propertyName, DateTime? value)
		{
			if (value.HasValue)
				doc[propertyName] = value.Value;
		}

		public static void SetValue(BsonDocument doc, string propertyName, int value)
		{
			doc[propertyName] = value;
		}

		public static void SetValue(BsonDocument doc, string propertyName, int? value)
		{
			if (value.HasValue)
			{
				doc[propertyName] = value.Value;
			}
		}

		public static void SetValue(BsonDocument doc, string propertyName, long value)
		{
			doc[propertyName] = value;
		}

		public static void SetValue(BsonDocument doc, string propertyName, long? value)
		{
			if (value.HasValue)
			{
				doc[propertyName] = value.Value;
			}
		}

		public static void SetValue(BsonDocument doc, string propertyName, double value)
		{
			doc[propertyName] = value;
		}

		public static void SetValue(BsonDocument doc, string propertyName, double? value)
		{
			if (value.HasValue)
			{
				doc[propertyName] = value.Value;
			}
		}

		public static void SetValue(BsonDocument doc, string propertyName, Guid value)
		{
			doc[propertyName] = value.ToByteArray();
		}

		public static void SetValue(BsonDocument doc, string propertyName, string value)
		{
			if (value != null)
			{
				doc[propertyName] = value;
			}
		}
    }
}
