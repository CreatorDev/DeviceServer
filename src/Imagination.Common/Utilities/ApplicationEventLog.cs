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
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Imagination
{
    public class ApplicationEventLog
    {

        private static Lazy<ILogger> _Logger = new Lazy<ILogger>(() => { 
            if (ServiceConfiguration.LoggerFactory != null)
                return ServiceConfiguration.LoggerFactory.CreateLogger(nameof(ApplicationEventLog));
            else
                return new ConsoleLogger(nameof(ApplicationEventLog), ((x, y) => true), true);
        });

        private static EventLogEntryType _Level = EventLogEntryType.Information;

        public static EventLogEntryType LogLevel { get { return _Level; } set { _Level = value; MapLoggerLevels(); } }

        private static Dictionary<EventLogEntryType, Action<string>> _LogLevelMap;

        static ApplicationEventLog()
        {
            MapLoggerLevels();
        }

        private static void MapLoggerLevels()
        {
            _LogLevelMap = new Dictionary<EventLogEntryType, Action<string>>();

            if (EventLogEntryType.Information <= _Level)
                _LogLevelMap.Add(EventLogEntryType.Information, m => _Logger.Value.LogInformation("{0}", m));
            if (EventLogEntryType.Warning <= _Level)
                _LogLevelMap.Add(EventLogEntryType.Warning, m => _Logger.Value.LogWarning("{0}", m));
            if (EventLogEntryType.Error <= _Level)
                _LogLevelMap.Add(EventLogEntryType.Error, m => _Logger.Value.LogError("{0}", m));
            //_LogLevelMap.Add(EventLogEntryType.FailureAudit, m => _Logger.Value.LogInformation("{0}", m));
            //_LogLevelMap.Add(EventLogEntryType.SuccessAudit, m => _Logger.Value.LogInformation("{0}", m));
        }

        public static void WriteEntry(string message)
        {
            WriteEntry("Flow", message, EventLogEntryType.Information);
        }

        public static void WriteEntry(string message, EventLogEntryType type)
        {
            WriteEntry("Flow", message, type);
        }

        public static void WriteEntry(string source, string message)
        {
            WriteEntry(source, message, EventLogEntryType.Information);
        }

        public static void WriteEntry(string source, string message, EventLogEntryType type)
        {
            Action<string> levelLogger;
            if (_Logger != null && _LogLevelMap.TryGetValue(type, out levelLogger))
            {
                levelLogger(message);
            }
            else
            {
                Console.WriteLine($"{type.ToString()}: {message}");
            }

        }

        public static void Write(string message, Exception exception = null)
        {
            _Logger.Value.Log(Microsoft.Extensions.Logging.LogLevel.Information, 0, new Microsoft.Extensions.Logging.Internal.FormattedLogValues(message, new object[0]), exception, MessageFormatter);
        }

        public static void Write(Microsoft.Extensions.Logging.LogLevel level, string message, Exception exception = null)
        {
            _Logger.Value.Log(level, 0, new Microsoft.Extensions.Logging.Internal.FormattedLogValues(message, new object[0]), exception, MessageFormatter);
        }

        public class MultilineLogEntry : IDisposable
        {
            private ILogger _Logger;
            private LogLevel _Level;
            private Exception _Exception;
            private StringBuilder _builder;

            public MultilineLogEntry(ILogger logger, Microsoft.Extensions.Logging.LogLevel level, Exception exception = null)
            {
                _Logger = logger;
                _Level = level;
                _Exception = exception;
                _builder = new StringBuilder();
            }

            public void Write(string message)
            {
                _builder.Append(message);
            }

            public void WriteLine(string message)
            {
                _builder.AppendLine(message);
            }

            public void Dispose()
            {
                _Logger.Log(_Level, 0, new Microsoft.Extensions.Logging.Internal.FormattedLogValues(_builder.ToString(), new object[0]), _Exception, MessageFormatter);
            }
        }

        public static MultilineLogEntry StartMultiline(Microsoft.Extensions.Logging.LogLevel level = Microsoft.Extensions.Logging.LogLevel.Information, Exception exception = null)
        {
            return new MultilineLogEntry(_Logger.Value, level, exception);
        }


        private static string MessageFormatter(object state, Exception error)
        {
            return state.ToString();
        }

    }
}
