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

using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System;

namespace Imagination
{
    /// <summary>
    /// Utility class for manipulating strings.
    /// </summary>
    public class StringUtils
    {
		private static ZBase32Encoder _Base32Encoder = new ZBase32Encoder();

		public static string SeperateWordsOnCapitals(string stringValue)
		{
			int indx, found;
			char[] letters;
			letters = stringValue.ToCharArray();
			indx = 1;
			found = 0;

			while (indx < letters.Length)
			{
				if (char.IsUpper(letters[indx]))
					stringValue = stringValue.Insert(indx + found++, " ");
				indx++;
			}

			return stringValue;
		}
        
        public static byte[] Decode(string text)
        {
            byte[] result = null;
            if (!string.IsNullOrEmpty(text) && text.Length > 16)
            {
                text = text.Replace('-', '+').Replace('_', '/');
                if ((text.Length % 4) != 0)
                    text = text.PadRight(text.Length + (4 - (text.Length % 4)), '=');
                result = Convert.FromBase64String(text);
            }
            return result;
        }

        public static string Encode(byte[] data)
        {
            string result = null;
            result = Convert.ToBase64String(data);
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
            }
            return result;
        }

		public static Guid GuidDecode(string text)
		{
			Guid result = Guid.Empty;
			if (!string.IsNullOrEmpty(text) && text.Length > 16)
			{
				text = text.Replace('-', '+').Replace('_', '/');
				if ((text.Length % 4) != 0)
					text = text.PadRight(text.Length + (4 - (text.Length % 4)), '=');
				byte[] bytes = Convert.FromBase64String(text);
				if (bytes != null && bytes.Length == 16)
					result = new Guid(bytes);
			}
			return result;
		}

		public static Guid GuidDecode32(string text)
		{
			Guid result = Guid.Empty;
			if (!string.IsNullOrEmpty(text) && text.Length > 16)
			{
				byte[] bytes = _Base32Encoder.Decode(text);
				if (bytes != null && bytes.Length == 16)
					result = new Guid(bytes);
			}
			return result;
		}

		public static string GuidEncode(Guid id)
		{
			string result = null;
			result = Convert.ToBase64String(id.ToByteArray());
			if (!string.IsNullOrEmpty(result))
			{
				result = result.Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
			}
			return result;
		}

		public static string GuidEncode32(Guid id)
		{
			return _Base32Encoder.Encode(id.ToByteArray());
		}

		public static bool GuidTryDecode(string text, out Guid id)
		{
			bool result = false;
			id = Guid.Empty;
			try
			{
				id = GuidDecode(text);
				result = id != Guid.Empty;
			}
			catch
			{
			}
			return result;
		}

		public static bool GuidTryDecode32(string text, out Guid id)
		{
			bool result = false;
			id = Guid.Empty;
			try
			{
				id = GuidDecode32(text);
				result = id != Guid.Empty;
			}
			catch
			{
			}
			return result;
		}

		public static bool GuidTryParse(string text, out Guid contentItemID)
		{
			bool result = false;
			contentItemID = Guid.Empty;
			if (!string.IsNullOrEmpty(text) && (text.Length == 36) && text.Contains("-"))
			{
				try
				{
					contentItemID = new Guid(text);
					result = true;
				}
				catch
				{

				}
			}
			return result;
		}

        public static string HexString(byte[] data)
        {
            char[] result = new char[data.Length * 2];
            byte b;
            for (int y = 0, x = 0; y < data.Length; ++y, ++x)
            {
                b = ((byte)(data[y] >> 4));
                result[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(data[y] & 0xF));
                result[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(result);
        }

        public static byte[] HexStringToByteArray(params string[] data)
        {
            byte[] result;
            int length = 0;
            for (int index = 0; index < data.Length; index++)
            {
                int strLength = data[index].Length;
                if ((strLength % 2) == 1)
                    strLength = strLength + 1;
                length = length + (strLength / 2);
            }
            result = new byte[length];
            int resultIndex = 0;
            for (int index = 0; index < data.Length; index++)
            {
                int strLength = data[index].Length;
                int strIndex;
                if ((strLength % 2) == 1)
                    strLength = strLength + 1;
                if (strLength % 2 == 0)
                    strIndex = 0;
                else
                {
                    strIndex = 1;
                    result[resultIndex] = (byte)(Convert.ToInt32(data[index].Substring(0, 1), 16) & 0xff);
                    resultIndex++;
                }
                for (; strIndex < strLength; strIndex += 2)
                {
                    result[resultIndex] = (byte)(Convert.ToInt32(data[index].Substring(strIndex, 2), 16) & 0xff);
                    resultIndex++;
                }
            }
            return result;
        }
        
		public static bool Like(string leftString, string rightString)
		{
			bool result = false;
			int foundIndex = 0;
			if (!string.IsNullOrEmpty(leftString))
			{
				result = true;
				if (!string.IsNullOrEmpty(rightString))
				{
					leftString = leftString.ToLower();
					rightString = rightString.ToLower();
					if (rightString.Contains("%"))
					{
						//string[] searchStrings = rightString.Split(new char[] { '%' }, StringSplitOptions.None);
						//int count = searchStrings.Length;
						List<string> searchStrings = GetLikeSearchStrings(rightString);
						int count = searchStrings.Count;
						bool startWith = true;
						for (int index = 0; index < count; index++)
						{
							if (!string.IsNullOrEmpty(searchStrings[index]))
							{
								if (startWith)
								{
									result = leftString.StartsWith(searchStrings[index]);
									foundIndex = searchStrings[index].Length;
								}
								else
								{
									if (index == count)
									{
										result = (leftString.EndsWith(searchStrings[index]) && (foundIndex <= (leftString.Length - searchStrings[index].Length)));
									}
									else
									{
										foundIndex = leftString.IndexOf(searchStrings[index], foundIndex);
										if (foundIndex == -1)
											result = false;
										else
											foundIndex += searchStrings[index].Length;
									}
								}
							}
							if (!result)
								break;
							startWith = false;
						}
					}
					else
						result = leftString.CompareTo(rightString) == 0;
				}
			}
			return result;
		}

		private static List<string> GetLikeSearchStrings(string text)
		{
			List<string> result = new List<string>();
			StringBuilder searchText = new StringBuilder();
			bool addText = false;
			for (int index = 0; index < text.Length; index++)
			{
				if (text[index] == '%')
				{
					if (addText)
					{
						searchText.Append(text[index]);
						addText = false;
					}
					else
						addText = true;
				}
				else
				{
					if (addText)
					{
						result.Add(searchText.ToString());
						searchText.Length = 0;
						addText = false;
					}
					searchText.Append(text[index]);
				}
			}
			result.Add(searchText.ToString());
			if (addText)
				result.Add(string.Empty);
			return result;
		}
        
		public static string ExtractQuotedValue(string str)
		{
			StringBuilder result = new StringBuilder();
			int startPos = 0; int newStartPos;
			int pos = startPos = SkipSpace(str, startPos);
			newStartPos = -1;
			if (str.Length <= pos || str[startPos++] != '"')
				return null;
			//bool haveEscapedChars = false;
			bool inEscape = false;
			bool foundEnd = false;
			pos = startPos;
			for (; pos < str.Length; pos++)
			{
				//Locate unescaped double quote
				if (str[pos] == '\\')
				{
					if (inEscape)
					{
						result.Append(str[pos]);
						result.Append(str[pos]);
					}
					inEscape = !inEscape;
					//haveEscapedChars = true;
				}
				//else if (str[pos] == '\'')
				//{
				//    result.Append('\\');
				//    result.Append(str[pos]);
				//}
				else
				{
					if (!inEscape && str[pos] == '"')
					{
						foundEnd = true;
						break;
					}
					inEscape = false;
					result.Append(str[pos]);
				}
			}
			if (!foundEnd)
				return null;
			newStartPos = pos + 1;

			return result.ToString(); ;
		}

		private static int SkipSpace(string str, int startPos)
		{
			for (; startPos < str.Length; startPos++)
			{
				if (str[startPos] != ' ')
					break;
			}
			return startPos;
		}

		public static string Trim(string text)
		{
			string result = null;
			if (text != null)
				result = text.Trim();
			return result;
			
		}

        public static string ToSingular(string plural)
        {
            string singular;
            if (plural.EndsWith("ies"))
                singular = string.Concat(plural.Substring(0, plural.Length - 3), "y");
            else if (plural.EndsWith("es"))
                singular = plural.Substring(0, plural.Length - 2);
            else if (plural.EndsWith("s"))
                singular = plural.Substring(0, plural.Length - 1);
            else
                singular = plural;
            return singular;
        }

        public static string ToPlural(string singular)
        {
            string plural;
            if (singular.EndsWith("y"))
                plural = string.Concat(singular.Substring(0, singular.Length - 1), "ies");
            else if (singular.EndsWith("x"))
                plural = string.Concat(singular, "es");
            else
                plural = string.Concat(singular, "s");
            return plural;
        }
    }
}
