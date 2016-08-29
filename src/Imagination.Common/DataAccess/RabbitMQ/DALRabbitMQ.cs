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
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using RabbitMQ.Client;
using Imagination.Model;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace Imagination.DataAccess
{
	public delegate void MessageArrivedEventHandler(string server,ServiceEventMessage message);

	public class DALRabbitMQ : IDisposable
	{

		private class QueuedMessage
		{
            public string RoutingKey { get; set; }
            public ServiceEventMessage Message { get; set; }
            public bool Requeued { get; set; }
        }

		internal const string EXCHANGE_NAME = "Imaginationservices.exchange";
		private string _TransactionalMessagePath;
		private string _TransactionalRequeueMessagePath;
		private string _ConfirmsMessagePath;
		private string _ConfirmsRequeueMessagePath;
		private string _RejectedMessagePath;
		
		private bool _Terminate = false;
		//private bool _ConnectionShutdown = false;
		
		private ManualResetEvent _TriggerPublishTransactionalMessages;
		private Thread _PublishTransactionalMessagesThread;

		private ManualResetEvent _TriggerPublishFireAndForgetMessages;
		private Thread _PublishFireAndForgetMessagesThread;
		
		private ManualResetEvent _TriggerPublishConfirmMessages;
		private Thread _PublishConfirmMessagesThread;

		private RabbitMQSubscription[] _Subscriptions;

		private List<RabbitMQConnection> _Servers;
        private int _ServerCount;
        private int _CurrentHostIndex;

		private Queue<QueuedMessage> _FireAndForgetMessages;
		private Dictionary<ulong, string> _ConfirmMessagesWaitingAck;
		private ulong _LastConfirmSeqNo;

		public DALRabbitMQ(List<RabbitMQConnection> servers) 
		{
			_FireAndForgetMessages = new Queue<QueuedMessage>(10000);
			_ConfirmMessagesWaitingAck = new Dictionary<ulong,string>(10000);
			_TriggerPublishTransactionalMessages = new ManualResetEvent(false);
			_TriggerPublishConfirmMessages = new ManualResetEvent(false);
			_TriggerPublishFireAndForgetMessages = new ManualResetEvent(false);
            _Servers = servers;
            _ServerCount = _Servers.Count;

            SetMessageDirectory();
			_Subscriptions = new RabbitMQSubscription[_ServerCount];
			for (int index = 0; index < _ServerCount; index++)
			{
				_Subscriptions[index] = new RabbitMQSubscription(_Servers[index],this);
			}
			Random rnd = new Random();
			_CurrentHostIndex = rnd.Next(_ServerCount);
			_ConfirmsMessagePath = Path.Combine(_TransactionalMessagePath, "Confirms");
            _RejectedMessagePath = Path.Combine(_TransactionalMessagePath, "Rejected");
            _TransactionalRequeueMessagePath = Path.Combine(_TransactionalMessagePath, "Requeue");
            _ConfirmsRequeueMessagePath = Path.Combine(_ConfirmsMessagePath, "Requeue");
            if (!Directory.Exists(_TransactionalMessagePath))
                Directory.CreateDirectory(_TransactionalMessagePath);
            if (!Directory.Exists(_ConfirmsMessagePath))
				Directory.CreateDirectory(_ConfirmsMessagePath);
            if (!Directory.Exists(_RejectedMessagePath))
                Directory.CreateDirectory(_RejectedMessagePath);
            if (!Directory.Exists(_TransactionalRequeueMessagePath))
                Directory.CreateDirectory(_TransactionalRequeueMessagePath);
            if (!Directory.Exists(_ConfirmsRequeueMessagePath))
                Directory.CreateDirectory(_ConfirmsRequeueMessagePath);

			CheckForUnSentMessage(_ConfirmsMessagePath);
			CheckForUnSentMessage(_ConfirmsRequeueMessagePath);

			CheckForUnSentMessage(_TransactionalMessagePath);
			CheckForUnSentMessage(_TransactionalRequeueMessagePath);

			if (Directory.GetFiles(_TransactionalMessagePath, "*.msg").Length > 0)
				StartTransactionalThread();
			if (Directory.GetFiles(_ConfirmsMessagePath, "*.msg").Length > 0)
				StartConfirmsThread();
			ReadQueueFromDisk(_FireAndForgetMessages, Path.Combine(_TransactionalMessagePath, "FireAndForget"));
			if (_FireAndForgetMessages.Count > 0)
				StartFireAndForgetThread();
		}


		 ~DALRabbitMQ()
		{
			Dispose();
		}

		public void AckMessage(ServiceEventMessage message)
		{
			for (int index = 0; index < _ServerCount; index++)
			{
				_Subscriptions[index].AckMessage(message);
			}
		}

		private void CheckForUnSentMessage(string path)
		{
			foreach (string filename in Directory.GetFiles(path, "*.trx"))
			{
				string messageFileName = Path.ChangeExtension(filename, ".msg");
				if (File.Exists(messageFileName))
					DeleteMessageFile(filename);
				else
					File.Move(filename, messageFileName);
			}
		}

		public void Dispose()
		{
			_Terminate = true;
			_TriggerPublishFireAndForgetMessages.Set();
			_TriggerPublishConfirmMessages.Set();
			_TriggerPublishTransactionalMessages.Set();
			for (int index = 0; index < _ServerCount; index++)
			{
				_Subscriptions[index].Dispose();
			}
			try
			{
				if (_PublishFireAndForgetMessagesThread != null)
				{
					if (_PublishFireAndForgetMessagesThread.IsAlive)
					{
						_PublishFireAndForgetMessagesThread.Join();
						_PublishFireAndForgetMessagesThread = null;
					}
				}
				if (_PublishConfirmMessagesThread != null)
				{
					if (_PublishConfirmMessagesThread.IsAlive)
					{
						_PublishConfirmMessagesThread.Join();
						_PublishConfirmMessagesThread = null;
					}
				}
				if (_PublishTransactionalMessagesThread != null)
				{
					if (_PublishTransactionalMessagesThread.IsAlive)
					{
						_PublishTransactionalMessagesThread.Join();
						_PublishTransactionalMessagesThread = null;
					}
				}
			}
			catch
			{

			}
			WriteQueueToDisk(_FireAndForgetMessages, Path.Combine(_TransactionalMessagePath, "FireAndForget"));
		}

		public void NackMessage(ServiceEventMessage message)
		{
            if (!message.QueueAfterTime.HasValue)
                message.QueueAfterTime = DateTime.UtcNow.AddSeconds(1.0);
            for (int index = 0; index < _ServerCount; index++)
			{
				_Subscriptions[index].NackMessage(message);
			}
		}

		public void NackMessage(ServiceEventMessage message, TimeSpan delayQueueingFor)
		{
			message.QueueAfterTime = DateTime.UtcNow.Add(delayQueueingFor);
			NackMessage(message);
		}


		public void Publish(string routingKey, ServiceEventMessage message, TMessagePublishMode publishMode)
		{
			if (message.MessageID == Guid.Empty)
				message.MessageID = Guid.NewGuid();
			if (message.TimeStamp == 0)
				message.TimeStamp = DateTime.UtcNow.Ticks;
            message.MessagePublishMode = publishMode;
			if (publishMode == TMessagePublishMode.FireAndForget && (!_Terminate))
			{
				StartFireAndForgetThread();
				lock (_FireAndForgetMessages)
				{
					_FireAndForgetMessages.Enqueue(new QueuedMessage() { RoutingKey = routingKey,  Message = message});
				}
				_TriggerPublishFireAndForgetMessages.Set();
			}
			else if (publishMode == TMessagePublishMode.Confirms && (!_Terminate))
			{
				StartConfirmsThread();
				WriteMessageToDisk(routingKey, message, _ConfirmsMessagePath);
				_TriggerPublishConfirmMessages.Set();
			}
			else
			{
				StartTransactionalThread();
				WriteMessageToDisk(routingKey,message,_TransactionalMessagePath);
				_TriggerPublishTransactionalMessages.Set();
			}
		}
		
		private void PublishConfirmMessages()
		{
			bool connectionError = false;
			bool abort = false;
			MemoryStream stream = new MemoryStream(32768);
			while (!_Terminate)
			{
				try
				{
					if (connectionError)
					{
						CheckForUnSentMessage(_ConfirmsMessagePath);
						CheckForUnSentMessage(_ConfirmsRequeueMessagePath);
					}
					connectionError = false;
					RabbitMQConnection server = _Servers[_CurrentHostIndex];
					_CurrentHostIndex = (_CurrentHostIndex + 1) % _ServerCount;
					ConnectionFactory connectionFactory = new ConnectionFactory();
                    connectionFactory.uri = server.Uri;
					if (!string.IsNullOrEmpty(server.Username))
						connectionFactory.UserName = server.Username;
					if (!string.IsNullOrEmpty(server.Password))
						connectionFactory.Password = server.Password;
                    string hostname = server.Uri.DnsSafeHost;
                    _LastConfirmSeqNo = 0;
					using (IConnection connection = connectionFactory.CreateConnection())
					{
						using (IModel confirmChannel = connection.CreateModel())
						{
							confirmChannel.BasicAcks += new EventHandler<BasicAckEventArgs>(ConfirmChannel_BasicAcks);
							confirmChannel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, true, false, null);
							confirmChannel.ConfirmSelect();
							while (!_Terminate)
							{
								_TriggerPublishConfirmMessages.Reset();
								PublishConfirmMessages(confirmChannel, hostname, EXCHANGE_NAME, _ConfirmsMessagePath , false);
								TimeSpan delay = PublishConfirmMessages(confirmChannel, hostname, string.Empty, _ConfirmsRequeueMessagePath, true);
								if (!_Terminate)
								{
									if (delay == TimeSpan.MaxValue)
										_TriggerPublishConfirmMessages.WaitOne();
									else
										_TriggerPublishConfirmMessages.WaitOne(delay);
								}
							}
							confirmChannel.Close(200, "Goodbye");
						}
						connection.Close();
					}
				}
				catch (ThreadAbortException)
				{
					abort = true;
				}
				catch (Exception ex)
				{
					connectionError = true;
					ApplicationEventLog.WriteEntry("Flow", string.Format("DALRabbitMQ::PublishMessages - Exception\n{0}", ex), System.Diagnostics.EventLogEntryType.Error);
				}
				if (abort)
					break;
				if (connectionError)
					Thread.Sleep(500);
			}
		}

		private TimeSpan PublishConfirmMessages(IModel confirmChannel, string hostname, string exchange, string path, bool requeue)
        {
			TimeSpan result = TimeSpan.MaxValue;
            MessageFormatter messageFormatter = new MessageFormatter();
            string[] files = Directory.GetFiles(path, "*.msg");
			int skipCount = 0;
			while (files.Length > skipCount)
            {
				skipCount = 0;
                foreach (string item in files)
                {
                    if (File.Exists(item))
                    {
                        byte[] data = null;
                        try
                        {
                            data = File.ReadAllBytes(item);
                        }
                        catch
                        {

                        }
                        if (data != null)
                        {
							if (requeue)
							{
								ServiceEventMessage message = messageFormatter.Deserialise(new MemoryStream(data));
								if ((message != null) && (message.QueueAfterTime.HasValue) && (message.QueueAfterTime.Value > DateTime.UtcNow))
								{
									TimeSpan delay = message.QueueAfterTime.Value.Subtract(DateTime.UtcNow);
									if (delay < result)
										result = delay;
									skipCount++;
									continue;
								}
							}
                            IBasicProperties properties = confirmChannel.CreateBasicProperties();
                            properties.CorrelationId = hostname;
                            properties.DeliveryMode = 2;
                            properties.ContentType = messageFormatter.ContentType;
                            string filename = Path.GetFileNameWithoutExtension(item);
                            string routingKey = string.Empty;
                            int index = filename.IndexOf('_');
                            if (index != -1)
                            {
                                routingKey = filename.Substring(0, index);
                                int nextIndex = filename.IndexOf('_', index + 1);
                                if (nextIndex == -1)
                                    properties.MessageId = filename.Substring(index + 1);
                                else
                                    properties.MessageId = filename.Substring(index + 1, nextIndex - index - 1);
                            }
                            ulong messageKey = confirmChannel.NextPublishSeqNo;
                            string messageFilename = Path.ChangeExtension(item, ".trx");
							if (File.Exists(messageFilename))
								DeleteMessageFile(messageFilename);
                            File.Move(item, messageFilename);
                            lock (_ConfirmMessagesWaitingAck)
                            {
                                _ConfirmMessagesWaitingAck[messageKey] = messageFilename;
                            }
                            confirmChannel.BasicPublish(exchange, routingKey, properties, data);
                        }
                    }
                }
                files = Directory.GetFiles(path, "*.msg");
            }
			return result;
        }

		private void PublishFireAndForgetMessages()
		{
			bool connectionError = false;
			MemoryStream stream = new MemoryStream(32768);
			while (!_Terminate)
			{
				try
				{
					connectionError = false;
                    RabbitMQConnection server = _Servers[_CurrentHostIndex];
                    _CurrentHostIndex = (_CurrentHostIndex + 1) % _ServerCount;
					ConnectionFactory connectionFactory = new ConnectionFactory();
                    connectionFactory.uri = server.Uri;
                    if (!string.IsNullOrEmpty(server.Username))
                        connectionFactory.UserName = server.Username;
                    if (!string.IsNullOrEmpty(server.Password))
                        connectionFactory.Password = server.Password;
                    string hostname = server.Uri.DnsSafeHost;

                    MessageFormatter messageFormatter = new MessageFormatter(stream);
					using (IConnection connection = connectionFactory.CreateConnection())
					{
						using (IModel asyncChannel = connection.CreateModel())
						{
							//asynchronousChannel.BasicReturn += new RabbitMQ.Client.Events.BasicReturnEventHandler(Channel_BasicReturn);
							asyncChannel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, true, false, null);
							while (!_Terminate)
							{
								_TriggerPublishFireAndForgetMessages.Reset();
								int skipCount = 0;
								TimeSpan delay = TimeSpan.MaxValue;
								Dictionary<Guid, Object> skippedMessages = new Dictionary<Guid, object>();
								while (_FireAndForgetMessages.Count > skipCount)
								{
									QueuedMessage queuedMessage;
									lock (_FireAndForgetMessages)
									{
										queuedMessage = _FireAndForgetMessages.Dequeue();
									}
									if (queuedMessage.Message.QueueAfterTime.HasValue && (queuedMessage.Message.QueueAfterTime.Value > DateTime.UtcNow))
									{
										TimeSpan messageDelay = queuedMessage.Message.QueueAfterTime.Value.Subtract(DateTime.UtcNow);
										if (messageDelay < delay)
											delay = messageDelay;
										lock (_FireAndForgetMessages)
										{
											_FireAndForgetMessages.Enqueue(queuedMessage);
										}
										if (skippedMessages.ContainsKey(queuedMessage.Message.MessageID))
										{
											skippedMessages.Add(queuedMessage.Message.MessageID, null);
											skipCount++;
										}
										continue;
									}
									stream.Position = 0;
                                    messageFormatter.Serialise(queuedMessage.Message);
                                    stream.SetLength(stream.Position);
									stream.Position = 0;
									byte[] data = stream.ToArray();
									IBasicProperties properties = asyncChannel.CreateBasicProperties();
									properties.CorrelationId = hostname;
									properties.DeliveryMode = 2;
									properties.MessageId = queuedMessage.Message.MessageID.ToString();
                                    properties.ContentType = messageFormatter.ContentType;

									//Transaction was only about ~14 messages a second whereas non is ~1300
                                    if (queuedMessage.Requeued)
                                        asyncChannel.BasicPublish(string.Empty, queuedMessage.RoutingKey, properties, data);
                                    else
									    asyncChannel.BasicPublish(EXCHANGE_NAME, queuedMessage.RoutingKey, properties, data);
								}
								if (!_Terminate)
								{
									if (delay == TimeSpan.MaxValue)
										_TriggerPublishFireAndForgetMessages.WaitOne();
									else
										_TriggerPublishFireAndForgetMessages.WaitOne(delay);
								}
							}
							asyncChannel.Close(200, "Goodbye");
						}
						connection.Close();
					}
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex)
				{
					connectionError = true;
					ApplicationEventLog.WriteEntry("Flow", string.Format("DALRabbitMQ::PublishMessages - Exception\n{0}", ex), System.Diagnostics.EventLogEntryType.Error);
				}
				if (connectionError)
					Thread.Sleep(500);
			}
		}

		private void PublishTransactionalMessages()
		{
			bool connectionError = false;
			MemoryStream stream = new MemoryStream(32768);
			while (!_Terminate)
			{
				try
				{
					if (connectionError)
					{
						CheckForUnSentMessage(_TransactionalMessagePath);
						CheckForUnSentMessage(_TransactionalRequeueMessagePath);
					}
					connectionError = false;
                    RabbitMQConnection server = _Servers[_CurrentHostIndex];
                    _CurrentHostIndex = (_CurrentHostIndex + 1) % _ServerCount;
					ConnectionFactory connectionFactory = new ConnectionFactory();
                    connectionFactory.uri = server.Uri;
                    if (!string.IsNullOrEmpty(server.Username))
                        connectionFactory.UserName = server.Username;
                    if (!string.IsNullOrEmpty(server.Password))
                        connectionFactory.Password = server.Password;
                    string hostname = server.Uri.DnsSafeHost;
                    using (IConnection connection = connectionFactory.CreateConnection())
					{
						using (IModel transactionalChannel = connection.CreateModel())
						{
							transactionalChannel.BasicReturn += new EventHandler<BasicReturnEventArgs>(Channel_BasicReturn);
							transactionalChannel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, true, false, null);
							while (!_Terminate)
							{
								_TriggerPublishTransactionalMessages.Reset();
								PublishTransactionalMessages(transactionalChannel, hostname, EXCHANGE_NAME, _TransactionalMessagePath, false);
								TimeSpan delay = PublishTransactionalMessages(transactionalChannel, hostname, string.Empty, _TransactionalRequeueMessagePath, true);
								if (!_Terminate)
								{
									if (delay == TimeSpan.MaxValue)
										_TriggerPublishTransactionalMessages.WaitOne();
									else
										_TriggerPublishTransactionalMessages.WaitOne(delay);
								}
							}
							transactionalChannel.Close(200, "Goodbye");
						}
						connection.Close();
					}
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex)
				{
					connectionError = true;
					ApplicationEventLog.WriteEntry("Flow", string.Format("DALRabbitMQ::PublishMessages - Exception\n{0}", ex), System.Diagnostics.EventLogEntryType.Error);
				}
				if (connectionError)
					Thread.Sleep(500);
			}
		}

		private TimeSpan PublishTransactionalMessages(IModel transactionalChannel, string hostname, string exchange, string path, bool requeue)
        {
			TimeSpan result = TimeSpan.MaxValue;
            MessageFormatter messageFormatter = new MessageFormatter();
            string[] files = Directory.GetFiles(path, "*.msg");
			int skipCount = 0;
			while (files.Length > skipCount)
            {
				skipCount = 0;
                foreach (string item in files)
                {
                    if (File.Exists(item))
                    {
                        byte[] data = null;
						try
						{
							data = File.ReadAllBytes(item);
						}
						catch
						{

						}
                        if (data != null)
                        {
							if (requeue)
							{
								ServiceEventMessage message = messageFormatter.Deserialise(new MemoryStream(data));
								if ((message != null) && (message.QueueAfterTime.HasValue) && (message.QueueAfterTime.Value > DateTime.UtcNow))
								{
									TimeSpan delay = message.QueueAfterTime.Value.Subtract(DateTime.UtcNow);
									if (delay < result)
										result = delay;
									skipCount++;
									continue;
								}
							}
							string messageFilename = Path.ChangeExtension(item, ".trx");
							if (File.Exists(messageFilename))
								DeleteMessageFile(messageFilename);
							File.Move(item, messageFilename);
                            IBasicProperties properties = transactionalChannel.CreateBasicProperties();
                            properties.CorrelationId = hostname;
                            properties.DeliveryMode = 2;
                            properties.ContentType = messageFormatter.ContentType;
                            string filename = Path.GetFileNameWithoutExtension(item);
                            string routingKey = string.Empty;
                            int index = filename.IndexOf('_');
                            if (index != -1)
                            {
                                routingKey = filename.Substring(0, index);
                                int nextIndex = filename.IndexOf('_', index + 1);
                                if (nextIndex == -1)
                                    properties.MessageId = filename.Substring(index + 1);
                                else
                                    properties.MessageId = filename.Substring(index + 1, nextIndex - index - 1);
                            }
                            transactionalChannel.TxSelect();
                            transactionalChannel.BasicPublish(exchange, routingKey, properties, data);
                            transactionalChannel.TxCommit();
							DeleteMessageFile(messageFilename);
                        }
                    }
                }
                files = Directory.GetFiles(path, "*.msg");
            }
			return result;
        }

		private void ReadQueueFromDisk(Queue<QueuedMessage> queue, string path)
		{
			if (Directory.Exists(path))
			{
                MessageFormatter messageFormatter = new MessageFormatter();
                foreach (string item in Directory.GetFiles(path, "*.msg"))
				{
					if (File.Exists(item))
					{
						ServiceEventMessage message = null;
						using (FileStream stream = File.OpenRead(item))
						{
							message = messageFormatter.Deserialise(stream);
							stream.Close();
						}
						string filename = Path.GetFileNameWithoutExtension(item);
						string routingKey = string.Empty;
						int index = filename.IndexOf('_');
						if (index != -1)
						{
							routingKey = filename.Substring(0, index);
						}
						if (!string.IsNullOrEmpty(routingKey))
							queue.Enqueue(new QueuedMessage() { RoutingKey = routingKey, Message = message });
						DeleteMessageFile(item);
					}
				}
			}
		}

        internal void Reject(ServiceEventMessage message)
        {
            WriteMessageToDisk(message.Queue, message, _RejectedMessagePath);  
        }

        internal void Requeue(ServiceEventMessage message)
        {
            if (message.MessagePublishMode == TMessagePublishMode.FireAndForget && (!_Terminate))
            {
                StartFireAndForgetThread();
                lock (_FireAndForgetMessages)
                {
                    _FireAndForgetMessages.Enqueue(new QueuedMessage() { RoutingKey = message.Queue, Message = message, Requeued = true });
                }
                _TriggerPublishFireAndForgetMessages.Set();
            }
            else if (message.MessagePublishMode == TMessagePublishMode.Confirms && (!_Terminate))
            {
                StartConfirmsThread();
                WriteMessageToDisk(message.Queue, message, _ConfirmsRequeueMessagePath);
                _TriggerPublishConfirmMessages.Set();
            }
            else
            {
                StartTransactionalThread();
                WriteMessageToDisk(message.Queue, message, _TransactionalRequeueMessagePath);
                _TriggerPublishTransactionalMessages.Set();
            }            
        }

		private void SetMessageDirectory()
		{
			string name = ServiceConfiguration.Name;
            string tempFolder = ServiceConfiguration.TempFolder;
			if (string.IsNullOrEmpty(name))
				_TransactionalMessagePath = Path.Combine(tempFolder, "RabbitMQ");
			else
				_TransactionalMessagePath = Path.Combine(Path.Combine(tempFolder, "RabbitMQ"), name);
			if (!Directory.Exists(_TransactionalMessagePath))
				Directory.CreateDirectory(_TransactionalMessagePath);
		}

        public void Subscribe(string queueName, bool durable, bool temporary, string routingKey, MessageArrivedEventHandler handler)
		{
			for (int index = 0; index < _ServerCount; index++)
			{
                _Subscriptions[index].Subscribe(queueName, durable, temporary, routingKey, handler);
			}
		}

		public void UnSubscribe(List<string> queueNames)
		{
			for (int index = 0; index < _ServerCount; index++)
			{
				_Subscriptions[index].UnSubscribe(queueNames);
			}
		}

		public void ReSubscribe(List<string> queueNames)
		{
			for (int index = 0; index < _ServerCount; index++)
			{
				_Subscriptions[index].ReSubscribe(queueNames);
			}
		}

		private void StartConfirmsThread()
		{
			if (_PublishConfirmMessagesThread == null)
			{
				lock (this)
				{
					if (_PublishConfirmMessagesThread == null)
					{
						_PublishConfirmMessagesThread = new Thread(new ThreadStart(PublishConfirmMessages));
						if (_PublishConfirmMessagesThread.Name == null)
							_PublishConfirmMessagesThread.Name = "PublishConfirmMessages";
						_PublishConfirmMessagesThread.IsBackground = true;
						_PublishConfirmMessagesThread.Start();
					}
				}
			}
		}

		private void StartFireAndForgetThread()
		{
			if (_PublishFireAndForgetMessagesThread == null)
			{
				lock (this)
				{
					if (_PublishFireAndForgetMessagesThread == null)
					{
						_PublishFireAndForgetMessagesThread = new Thread(new ThreadStart(PublishFireAndForgetMessages));
						if (_PublishFireAndForgetMessagesThread.Name == null)
							_PublishFireAndForgetMessagesThread.Name = "PublishFireAndForgetMessages";
						_PublishFireAndForgetMessagesThread.IsBackground = true;
						_PublishFireAndForgetMessagesThread.Start();
					}
				}
			}
		}

		private void StartTransactionalThread()
		{
			if (_PublishTransactionalMessagesThread == null)
			{
				lock (this)
				{
					if (_PublishTransactionalMessagesThread == null)
					{
						_PublishTransactionalMessagesThread = new Thread(new ThreadStart(PublishTransactionalMessages));
						if (_PublishTransactionalMessagesThread.Name == null)
							_PublishTransactionalMessagesThread.Name = "PublishTransactionalMessages";
						_PublishTransactionalMessagesThread.IsBackground = true;
						_PublishTransactionalMessagesThread.Start();
					}
				}
			}
		}

		private void WriteMessageToDisk(string routingKey, ServiceEventMessage message, string path)
		{
			string filename = Path.Combine(path, string.Concat(routingKey, "_", message.MessageID, ".lck"));
			lock (this)
			{
				string messageFilename = Path.ChangeExtension(filename, ".msg");
				if (!File.Exists(messageFilename))
				{
                    using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.None))
					{
                        MessageFormatter messageFormatter = new MessageFormatter(stream);
                        messageFormatter.Serialise(message);
						stream.Flush();
						stream.Close();
					}
					File.Move(filename, messageFilename);
				}
			}
		}

		private void WriteQueueToDisk(Queue<QueuedMessage> queue, string path)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
            MessageFormatter messageFormatter = new MessageFormatter();
            while (queue.Count > 0)
			{
				QueuedMessage queuedMessage;
				lock (queue)
				{
					queuedMessage = queue.Dequeue();
				}
				string routingKey = queuedMessage.RoutingKey;
				ServiceEventMessage message = queuedMessage.Message;
				if (message.MessageID == Guid.Empty)
					message.MessageID = Guid.NewGuid();
				if (message.TimeStamp == 0)
					message.TimeStamp = DateTime.UtcNow.Ticks;
				string filename = Path.Combine(path, string.Format("{0}_{1}.lck", routingKey, message.MessageID));
				string messageFilename = Path.ChangeExtension(filename, ".msg");
				if (!File.Exists(messageFilename))
				{
					using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.None))
					{
                        messageFormatter.Serialise(stream, message);
					}
					File.Move(filename, messageFilename);
				}
			}
		}

		private void ConfirmChannel_BasicAcks(object sender, BasicAckEventArgs args)
		{
			string fileName;
			lock (_ConfirmMessagesWaitingAck)
			{
				if (args.Multiple)
				{
                    for (ulong index = args.DeliveryTag; index > 0; index--)
                    {
                        if (_ConfirmMessagesWaitingAck.TryGetValue(index, out fileName))
                        {
                            _ConfirmMessagesWaitingAck.Remove(index);
                            DeleteMessageFile(fileName);
                        }
                        else
                        {
                            break;
                        }
                    }
                    _LastConfirmSeqNo = args.DeliveryTag + 1;
                }
				else
				{
					if (_ConfirmMessagesWaitingAck.TryGetValue(args.DeliveryTag, out fileName))
					{
						_ConfirmMessagesWaitingAck.Remove(args.DeliveryTag);
						DeleteMessageFile(fileName);
					}
                    if (_LastConfirmSeqNo == args.DeliveryTag)
                        _LastConfirmSeqNo = args.DeliveryTag + 1;
                }
			}
		}

		
        private void DeleteMessageFile(string fileName)
        {
            bool deleted = false;
            try
            {
                File.Delete(fileName);
                deleted = true;
            }
            catch
            {
            }
            if (!deleted)
            {
                try
                {
                    string messageFilename = Path.ChangeExtension(fileName, ".del");
                    File.Move(fileName, messageFilename);
                }
                catch (Exception ex)
                {
					ApplicationEventLog.WriteEntry("Flow", string.Format("DALRabbitMQ::DeleteMessageFile - Exception\n{0}", ex), System.Diagnostics.EventLogEntryType.Error);
                }
            }
        }

		public void DeleteQueue(string queueName)
		{
			for (int index = 0; index < _ServerCount; index++)
			{
				try
				{
					ConnectionFactory connectionFactory = new ConnectionFactory();
                    RabbitMQConnection server = _Servers[index];
                    connectionFactory.uri = server.Uri;
                    if (!string.IsNullOrEmpty(server.Username))
                        connectionFactory.UserName = server.Username;
                    if (!string.IsNullOrEmpty(server.Password))
                        connectionFactory.Password = server.Password;
                    using (IConnection connection = connectionFactory.CreateConnection())
					{
						using (IModel channel = connection.CreateModel())
						{
							channel.QueueDelete(queueName);
						}
					}
				}
				catch
				{

				}
			}
		}

		private void Channel_BasicReturn(object sender, RabbitMQ.Client.Events.BasicReturnEventArgs args)
		{
			lock (this)
			{
				string filename = Path.Combine(_TransactionalMessagePath, string.Format("{0}_{1}.lck", args.RoutingKey, args.BasicProperties.MessageId));
				string messageFilename = Path.ChangeExtension(filename, ".msg");
				if (!File.Exists(messageFilename))
				{
					File.WriteAllBytes(filename, args.Body);
					File.Move(filename, messageFilename);
				}
			}
		}

    }
}
