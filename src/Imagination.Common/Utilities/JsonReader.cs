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
	public class JsonReader : IDisposable
	{
		private TextReader _TextReader;

		private TJsonReaderState _State = TJsonReaderState.BOF;
		private bool _BooleanValue = false;
		private bool _NumberIsDecimal;
		private string _StringValue;
		private StringBuilder _TextBuffer = new StringBuilder(128);
		private bool[] _LevelIsArray = new bool[255];
		private int _Level = -1;

		public bool AsBoolean
		{
			get
			{
				return _BooleanValue;
			}
		}

		public bool NumberIsDecimal
		{
			get 
			{
				bool result = false;
				if (_State == TJsonReaderState.Number)
					result = _NumberIsDecimal;
				return result ; 
			}
		}

		public TJsonReaderState State 
		{ 
			get { return _State; } 
		}

		public string Text
		{
			get { return _StringValue; }
		}
		
		public JsonReader(Stream stream)
		{
			_TextReader = new StreamReader(stream);
		}

		public JsonReader(TextReader textReader)
		{
			_TextReader = textReader;
		}


		public void Dispose()
		{
		}

		private void DownLevel()
		{
			_Level--;
		}

		private uint ParseHexChar(char hex)
		{ 
			uint p1 = 0;
			if (hex >= '0' && hex <= '9')
				p1 = (uint)(hex - '0');
			else if (hex >= 'A' && hex <= 'F')
				p1 = (uint)((hex - 'A') + 10);
			else if (hex >= 'a' && hex <= 'f')
				p1 = (uint)((hex - 'a') + 10);
			return p1;
		}


		private string ParseNumber(int item)
		{
			_TextBuffer.Length = 0;
			_TextBuffer.Append((char)item);
			_NumberIsDecimal = false;
			while (true)
			{
				item = _TextReader.Peek();
				if ((item >= '0' && item <= '9') || item == '.' || item == '-' || item == '+' || item == 'e' || item == 'E')
				{
					if (item == '.' || item == 'e' || item == 'E')
						_NumberIsDecimal = true;
					_TextBuffer.Append((char)item);
					_TextReader.Read();
				}
				else
					break;
			}
			return _TextBuffer.ToString();
		}

		private string ParseString()
		{
			_TextBuffer.Length = 0;

			int item = _TextReader.Read();
			while (item >= 0)
			{
				if (item == '"')
				{
					break;
				}

				if (item == '\\')
				{
					item = _TextReader.Read();
					switch (item)
					{
						case '"':
							_TextBuffer.Append('"');
							break;

						case '\\':
							_TextBuffer.Append('\\');
							break;

						case '/':
							_TextBuffer.Append('/');
							break;

						case 'b':
							_TextBuffer.Append('\b');
							break;

						case 'f':
							_TextBuffer.Append('\f');
							break;

						case 'n':
							_TextBuffer.Append('\n');
							break;

						case 'r':
							_TextBuffer.Append('\r');
							break;

						case 't':
							_TextBuffer.Append('\t');
							break;

						case 'u':
							{
								char[] unicode = new char[4];
								int read = _TextReader.Read(unicode, 0, 4);
								if (read == 4)
								{
									uint codePoint = (ParseHexChar((char)unicode[0]) * 0x1000) + (ParseHexChar((char)unicode[1]) * 0x100) + (ParseHexChar((char)unicode[2]) * 0x10) + ParseHexChar((char)unicode[3]);
									_TextBuffer.Append((char)codePoint);
								}
							}
							break;
					}
				}
				else 
					_TextBuffer.Append((char)item);
				item = _TextReader.Read();
			}
			return _TextBuffer.ToString();
		}


		public bool Read()
		{
			bool result = false;
			if (_State != TJsonReaderState.EOF)
			{
				TJsonReaderState newstate = TJsonReaderState.NotSet;
				bool valueSection = false;
				bool readAgain;
				do
				{
					readAgain = false;
					SkipWhitespace();
					int item = _TextReader.Read();
					if (item < 0)
						newstate = TJsonReaderState.EOF;
					else
					{
						switch (item)
						{
							case '{':
								newstate = TJsonReaderState.Object;
								UpLevel(false);
								break;
							case '}':
								newstate = TJsonReaderState.EndObject;
								DownLevel();
								break;
							case '[':
								newstate = TJsonReaderState.Array;
								UpLevel(true);
								break;

							case ']':
								newstate = TJsonReaderState.EndArray;
								DownLevel();
								break;

							case ',':
								readAgain = true;
								break;

							case '"':
								if (valueSection || _LevelIsArray[_Level])
									newstate = TJsonReaderState.String;
								else
									newstate = TJsonReaderState.Member;
								_StringValue = ParseString();
								break;
							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9':
							case '-':
							case '+':
							case '.':
								newstate = TJsonReaderState.Number;
								_StringValue = ParseNumber(item);
								break;
							case ':':
								valueSection = true;
								readAgain = true;
								break;

							case 'f':
								item = _TextReader.Read();
								if (item == 'a')
								{
									item = _TextReader.Read();
									if (item == 'l')
									{
										item = _TextReader.Read();
										if (item == 's')
										{
											item = _TextReader.Read();
											if (item == 'e')
											{
												newstate = TJsonReaderState.Boolean;
												_BooleanValue = false;
											}
										}
									}
								}
								break;

							case 't':
								item = _TextReader.Read();
								if (item == 'r')
								{
									item = _TextReader.Read();
									if (item == 'u')
									{
										item = _TextReader.Read();
										if (item == 'e')
										{
											newstate = TJsonReaderState.Boolean;
											_BooleanValue = true;
										}
									}
								}
								break;

							case 'n':
								item = _TextReader.Read();
								if (item == 'u')
								{
									item = _TextReader.Read();
									if (item == 'l')
									{
										item = _TextReader.Read();
										if (item == 'l')
										{
											newstate = TJsonReaderState.Null;
										}
									}
								}
								break;

							default:								
#if SILVERLIGHT
								throw new ArgumentException();
#else
								throw new InvalidDataException();
#endif		
						}

					}
				} while (readAgain);
				if ((newstate != TJsonReaderState.EOF) && (newstate != TJsonReaderState.NotSet))
					result = true;
				_State = newstate;
			}
			return result;
		}

		private void SkipWhitespace()
		{
			int nextChar =	_TextReader.Peek();
			while ((nextChar <= ' ') && ((nextChar == ' ') || (nextChar == '\t') || (nextChar == '\r') || (nextChar == '\n')))
			{
				_TextReader.Read();
				nextChar = _TextReader.Peek();
			}
		}

		private void UpLevel(bool isArray)
		{
			_Level++;
			_LevelIsArray[_Level] = isArray;
		}

	}
}
