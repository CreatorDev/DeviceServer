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
using System.IO;

namespace Imagination
{
	public class JsonWriter : IDisposable
	{
		private	class Level
		{
			public bool IsObject { get; set; }
			public int Count { get; set; }
		}

		private static DateTime _EpochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private TextWriter _TextWriter;
		private Level[] _Levels = new Level[255];
		private int _Level = -1;
		private const string TRUE = "true";
		private const string FALSE = "false";
		private const string NULL = "null";


		public JsonWriter(Stream stream)
		{
			_TextWriter = new StreamWriter(stream);
		}

		public JsonWriter(TextWriter writer)
		{
			_TextWriter = writer;
		}

		private void CheckNeedComma(bool isObject)
		{
			if (_Level >= 0)
			{
				if (_Levels[_Level].IsObject == isObject)
				{
					if (_Levels[_Level].Count > 0)
						_TextWriter.Write(',');
				}
				_Levels[_Level].Count++;
			}
		}

		public void Dispose()
		{
			Flush();
		}


		private void DownLevel()
		{
			_Level--;
		}

		public void Flush()
		{
			_TextWriter.Flush();
		}

		private void UpLevel(bool isObject)
		{
			_Level++;
			if (_Levels[_Level] == null)
				_Levels[_Level] = new Level();
			_Levels[_Level].IsObject = isObject;
			_Levels[_Level].Count = 0;
		}

		public void WriteArray()
		{
			_TextWriter.Write('[');
			UpLevel(false);
		}
		
		public void WriteEndArray()
		{
			_TextWriter.Write(']');
			DownLevel();
		}

		public void WriteEndObject()
		{
			_TextWriter.Write('}');
			DownLevel();
		}

		public void WriteMember(string name)
		{
			CheckNeedComma(true);
			WriteString(name);
			_TextWriter.Write(':');
		}


		public void WriteNull()	
		{
			CheckNeedComma(false);
			_TextWriter.Write(NULL);
		}

		public void WriteObject()
		{
			CheckNeedComma(false);
			_TextWriter.Write('{');
			UpLevel(true);
		}
		
		private void WriteString(string text)
		{
			_TextWriter.Write('"');
			if (!string.IsNullOrEmpty(text))
			{
				int length = text.Length;
				for (int index = 0; index < length; index++)
				{
					if ((text[index] == '\\') || (text[index] == '"')) //|| (text[index] == '/')
					{
						_TextWriter.Write('\\');
						_TextWriter.Write(text[index]);
					}
					else if (text[index] == '\b')
					{
						_TextWriter.Write('\\');
						_TextWriter.Write('b');

					}
					else if (text[index] == '\f')
					{
						_TextWriter.Write('\\');
						_TextWriter.Write('f');
					}
					else if (text[index] == '\n')
					{
						_TextWriter.Write('\\');
						_TextWriter.Write('n');

					}
					else if (text[index] == '\r')
					{
						_TextWriter.Write('\\');
						_TextWriter.Write('r');

					}
					else if (text[index] == '\t')
					{
						_TextWriter.Write('\\');
						_TextWriter.Write('t');

					}
					else
						_TextWriter.Write(text[index]);
				}
			}
			_TextWriter.Write('"');
		}

		public void WriteValue(bool value)
		{
			CheckNeedComma(false);
			if (value)
				_TextWriter.Write(TRUE);
			else
				_TextWriter.Write(FALSE);
		}

        public void WriteValue(bool? value)
        {
            CheckNeedComma(false);
            if (value != null)
            {
                if ((bool)value)
                    _TextWriter.Write(TRUE);
                else
                    _TextWriter.Write(FALSE);
            }
            else
                _TextWriter.Write(NULL);
        }

        public void WriteValue(DateTime? value)
        {
            CheckNeedComma(false);
            WriteObject();
            WriteMember("$date");
            if (value != null)
            {
                DateTime newDateTime = (DateTime)value;
                WriteValue((long)newDateTime.Subtract(_EpochDate).TotalMilliseconds);
            }
            else
                WriteNull();
            WriteEndObject();
        }

		public void WriteValue(DateTime value)
		{
            CheckNeedComma(false);
            //WriteObject();
            //WriteMember("$date");
            //WriteValue((long)value.Subtract(_EpochDate).TotalMilliseconds);					
            //WriteEndObject();
            WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
		}

		public void WriteValue(double value)
		{
			CheckNeedComma(false);
			_TextWriter.Write(value.ToString());
		}

        public void WriteValue(double? value)
        {
            CheckNeedComma(false);
            _TextWriter.Write(value.ToString());
        }
		
		public void WriteValue(int value)
		{
			CheckNeedComma(false);
			_TextWriter.Write(value.ToString());
		}

        public void WriteValue(int? value)
        {
            CheckNeedComma(false);
            if (value != null)
                _TextWriter.Write(value.ToString());
            else
                _TextWriter.Write(NULL);
        }

		public void WriteValue(long value)
		{
			CheckNeedComma(false);
			_TextWriter.Write(value.ToString());
		}

        public void WriteValue(long? value)
        {
            CheckNeedComma(false);
            _TextWriter.Write(value.ToString());
        }

        public void WriteValue(Guid value)
        {
            CheckNeedComma(false);
            _TextWriter.Write(value.ToString());
        }

        public void WriteValue(Enum value)
        {
            CheckNeedComma(false);
            _TextWriter.Write(value.ToString());
        }

		public void WriteValue(string text)
		{
			CheckNeedComma(false);
			if (text == null)
				WriteNull();
			else
				WriteString(text);
		}

	}
}
