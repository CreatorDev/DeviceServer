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

namespace Imagination.DataAccess
{
	internal class RabbitMQSubscription : IDisposable
	{
		private class AckMessageInfo
		{
			public Guid DeliveryID { get; set; }
			public string QueueName { get; set; }
			public int TotalExpected { get; set; }
			public ulong DeliveryTag { get; set; }
			public int TotalAck { get; set; }
			public int TotalNack { get; set; }
			public int Total { get; set; }
			public ChannelHandler ChannelHandler { get; set; }
		}

		private class MessageQueue
		{
			public string QueueName { get; set; }
			public string RoutingKey { get; set; }
            public bool Durable { get; set; }
            public bool Temporary { get; set; }            
			public List<MessageArrivedEventHandler> Handlers { get; set; }
			public Dictionary<Guid, AckMessageInfo> Messages { get; set; }
			public string ChannelName { get; set; }
			public IModel Channel { get; set; }
			public MessageQueue()
			{
				Messages = new Dictionary<Guid, AckMessageInfo>(100);
				Handlers = new List<MessageArrivedEventHandler>();
			}

			internal void AddAckMessageInfo(Guid deliveryID, AckMessageInfo ackMessageInfo)
			{
				lock (Messages)
				{
					Messages.Add(deliveryID, ackMessageInfo);
				}
			}
		}

		private class MessageArrivedState
		{
            public AckMessageInfo AckMessageInfo { get; set; }
			public MessageArrivedEventHandler Handler { get; set; }
			public ServiceEventMessage Message { get; set; }
            public MessageQueue Queue { get; set; }

            public MessageArrivedState(MessageArrivedEventHandler handler, ServiceEventMessage message,MessageQueue queue, AckMessageInfo ackMessageInfo)
			{
                AckMessageInfo = ackMessageInfo;
				Handler = handler;
				Message = message;
                Queue = queue;
			}
		}

		private class ChannelHandler : IBasicConsumer
		{
#pragma warning disable 67
            public event EventHandler<RabbitMQ.Client.Events.ConsumerEventArgs> ConsumerCancelled;
#pragma warning restore 67
            private string _ChannelName;
			private IModel _Model;
			private Dictionary<string, MessageQueue> _Queues;
			private ConnectionFactory _ConnectionFactory;
			private RabbitMQSubscription _Subscription;
			private bool _ShutDown;
			private bool _ProcessMessages;
			private int _MessageHandlingCount;

			public ChannelHandler(string channelName, IModel channel, ConnectionFactory connectionFactory, RabbitMQSubscription subscription)
			{
				_ProcessMessages = true;
				_Queues = new Dictionary<string, MessageQueue>();
				_ChannelName = channelName;
				_Model = channel;
				_ConnectionFactory = connectionFactory;
				_Subscription = subscription;
			}

			public void AddQueue(MessageQueue queue)
			{
				lock (_Queues)
				{
					if (!_Queues.ContainsKey(queue.QueueName))
					{
						_Queues.Add(queue.QueueName, queue);
					}
				}
			}

			void IBasicConsumer.HandleBasicCancel(string consumerTag)
			{
			}

			void IBasicConsumer.HandleBasicCancelOk(string consumerTag)
			{
			}

			void IBasicConsumer.HandleBasicConsumeOk(string consumerTag)
			{
			}

			void IBasicConsumer.HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
			{
				try
				{
					if (_ProcessMessages)
					{
						Interlocked.Increment(ref _MessageHandlingCount);
                        MessageFormatter messageFormatter = new MessageFormatter();
                        ServiceEventMessage message = messageFormatter.Deserialise(new MemoryStream(body));
						lock (_Queues)
						{
							if (_Queues.ContainsKey(consumerTag))
							{
								MessageQueue queue = _Queues[consumerTag];
								message.DeliveryID = Guid.NewGuid();
								AckMessageInfo ackMessageInfo = new AckMessageInfo();
								ackMessageInfo.DeliveryID = message.DeliveryID;
								ackMessageInfo.QueueName = queue.QueueName;
								ackMessageInfo.DeliveryTag = deliveryTag;
								ackMessageInfo.TotalExpected = queue.Handlers.Count;
								ackMessageInfo.ChannelHandler = this;
								queue.AddAckMessageInfo(message.DeliveryID, ackMessageInfo);
								message.Queue = queue.QueueName;
								for (int index = 0; index < queue.Handlers.Count; index++)
								{
									try
									{
										MessageArrivedEventHandler handler = queue.Handlers[index];
										handler.BeginInvoke(_ConnectionFactory.HostName, message, _Subscription.InvokeCallBack, new MessageArrivedState(handler, message, queue, ackMessageInfo));
									}
									catch (Exception ex)
									{
										_Subscription.NackMessage(message);
										ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
									}
								}
								if (queue.Handlers.Count == 0)
								{
									Thread.Sleep(500);
									_Model.BasicReject(deliveryTag, true);
									Interlocked.Decrement(ref _MessageHandlingCount);
									ApplicationEventLog.WriteEntry("Flow", string.Format("No handlers to process message {0}", consumerTag), System.Diagnostics.EventLogEntryType.Error);
								}
							}
							else
							{
								Thread.Sleep(500);
								_Model.BasicReject(deliveryTag, true);
								Interlocked.Decrement(ref _MessageHandlingCount);
								ApplicationEventLog.WriteEntry("Flow", string.Format("HandleBasicDeliver: Failed to locate queue {0}", consumerTag), System.Diagnostics.EventLogEntryType.Error);
							}
						}
					}
				}
				catch (System.Runtime.Serialization.SerializationException)
				{
					string path = GetBadMessageDirectory();
					File.WriteAllBytes(Path.Combine(path,string.Concat(consumerTag,"_", Guid.NewGuid().ToString())),body);
					_Model.BasicReject(deliveryTag, false);
					Interlocked.Decrement(ref _MessageHandlingCount);
				}
				catch (Exception ex)
				{
					Thread.Sleep(500);
					try
					{
						if (_Model != null)
							_Model.BasicReject(deliveryTag, true);
						Interlocked.Decrement(ref _MessageHandlingCount);
					}
					catch { }
					ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
				}
			}

			void IBasicConsumer.HandleModelShutdown(object sender, ShutdownEventArgs reason)
			{
				if (!_ShutDown)
				{
					_ShutDown = true;
					List<MessageQueue> queues = new List<MessageQueue>();
					lock (_Queues)
					{
						foreach (var item in _Queues)
						{
							queues.Add(item.Value);
						}
					}
					_Subscription.HandleModelShutdown(_ChannelName, queues);
				}
			}

			IModel IBasicConsumer.Model
			{
				get { return _Model; }
			}

			internal IModel Channel
			{
				get { return _Model; }
			}

			private string GetBadMessageDirectory()
			{
				string result;
				string name = ServiceConfiguration.Name;
				string tempFolder = ServiceConfiguration.TempFolder;
				if (string.IsNullOrEmpty(name))
					result = Path.Combine(Path.Combine(tempFolder, "RabbitMQ"),"BadMessage");
				else
					result = Path.Combine(Path.Combine(Path.Combine(tempFolder, "RabbitMQ"), name), "BadMessage");
				if (!Directory.Exists(result))
					Directory.CreateDirectory(result);
				return result;
			}

			public void ProcessedMessage()
			{
				Interlocked.Decrement(ref _MessageHandlingCount);
			}

			public void StopProcessingMessages()
			{
				_ProcessMessages = false;
			}


			public void WaitForIdle(TimeSpan timeout)
			{
				DateTime expiryTime = DateTime.UtcNow.Add(timeout);
				while ((_MessageHandlingCount > 0) && (expiryTime > DateTime.UtcNow))
				{
					Thread.Sleep(50);
				}
			}

		}



		private RabbitMQConnection _Server; 
		private bool _Terminate = false;
		private bool _ConnectionShutdown = false;

		private ManualResetEvent _TriggerSubscriptionRequest;
		private Thread _SubscriptionRequestThread;
		private Dictionary<string, ChannelHandler> _ChannelHandlers;
		private int _OpenChannels;

		private Queue<MessageQueue> _SubscriptionToAdd;
		private Dictionary<string, MessageQueue> _Queues;
		private Dictionary<string, MessageQueue> _UnsubscribedQueues;
		private List<string> _UnSubscribeQueues = new List<string>();
		private AsyncCallback _InvokeCallBack;

		private DALRabbitMQ _DALRabbitMQ;

        public RabbitMQSubscription(RabbitMQConnection server, DALRabbitMQ dalRabbitMQ)
		{
            _DALRabbitMQ = dalRabbitMQ;
            _Server = server;
			_Queues = new Dictionary<string, MessageQueue>();
			_UnsubscribedQueues = new Dictionary<string, MessageQueue>();
			_SubscriptionToAdd = new Queue<MessageQueue>();
			_ChannelHandlers = new Dictionary<string, ChannelHandler>();
			_TriggerSubscriptionRequest = new ManualResetEvent(false);
			_InvokeCallBack = new AsyncCallback(InvokeCallBack);
		}

		public void AckMessage(ServiceEventMessage message)
		{
			MessageQueue messageQueue;
			if (_Queues.TryGetValue(message.Queue, out messageQueue))
			{
				AckMessageInfo ackMessageInfo;
				lock (messageQueue.Messages)
				{
					if (messageQueue.Messages.TryGetValue(message.DeliveryID, out ackMessageInfo))
					{
						lock (ackMessageInfo)
						{
							ackMessageInfo.TotalAck = ackMessageInfo.TotalAck + 1;
						}
					}
				}
			}
		}

		public void Dispose()
		{
			_Terminate = true;
			_TriggerSubscriptionRequest.Set();
			lock (_ChannelHandlers)
			{
				foreach (KeyValuePair<string, ChannelHandler> item in _ChannelHandlers)
				{
					item.Value.StopProcessingMessages();
				}
			}
			if (_SubscriptionRequestThread != null)
			{
				_TriggerSubscriptionRequest.Set();
				_SubscriptionRequestThread.Join();
				_SubscriptionRequestThread = null;
			}
		}

		private void InvokeCallBack(IAsyncResult asyncResult)
		{
			MessageArrivedState state = asyncResult.AsyncState as MessageArrivedState;
			if (state != null)
			{
                ServiceEventMessage message = state.Message;
				try
				{
					state.Handler.EndInvoke(asyncResult);
				}
				catch (Exception ex)
				{
					ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
					message.QueueAfterTime = DateTime.UtcNow.AddSeconds(1.0);
					NackMessage(message);
				}
                lock (state.AckMessageInfo)
                {
                    state.AckMessageInfo.Total = state.AckMessageInfo.Total + 1;
					if (state.AckMessageInfo.Total == state.AckMessageInfo.TotalExpected)
					{
						RespondToMessage(message, state.Queue, state.AckMessageInfo);
						state.AckMessageInfo.ChannelHandler.ProcessedMessage();
					}
                }
            }
		}

		public void NackMessage(ServiceEventMessage message)
		{
			MessageQueue messageQueue;
			if (_Queues.TryGetValue(message.Queue, out messageQueue))
			{
				AckMessageInfo ackMessageInfo;
				lock (messageQueue.Messages)
				{
					if (messageQueue.Messages.TryGetValue(message.DeliveryID, out ackMessageInfo))
					{
                        lock (ackMessageInfo)
                        {
                            ackMessageInfo.TotalNack = ackMessageInfo.TotalNack + 1;
                        }
					}
				}
			}
		}

		private void ProcessSubscriptionRequests()
		{
			bool connectionError = false;
			DateTime? nextErrorLogTime = null;
			while (!_Terminate)
			{
				try
				{
					connectionError = false;
					_ConnectionShutdown = false;
					ConnectionFactory connectionFactory = new ConnectionFactory();
                    connectionFactory.uri = _Server.Uri;
                    if (!string.IsNullOrEmpty(_Server.Username))
                        connectionFactory.UserName = _Server.Username;
                    if (!string.IsNullOrEmpty(_Server.Password))
                        connectionFactory.Password = _Server.Password;
                    _OpenChannels = 0;
					using (IConnection connection = connectionFactory.CreateConnection())
					{

						while (!_Terminate && !_ConnectionShutdown)
						{
							_TriggerSubscriptionRequest.Reset();
							while (_SubscriptionToAdd.Count > 0)
							{
								lock (_ChannelHandlers)
								{
									 ChannelHandler handler;
									IModel channel;
									MessageQueue queue;
									lock (_Queues)
									{
										queue = _SubscriptionToAdd.Dequeue();
									}
									if (queue != null)
									{
										if (!_ChannelHandlers.TryGetValue(queue.ChannelName, out handler))
										{
											channel = connection.CreateModel();
											channel.BasicQos(0, 1, false);
											channel.ExchangeDeclare(DALRabbitMQ.EXCHANGE_NAME, ExchangeType.Topic, true, false, null);
											handler = new ChannelHandler(queue.ChannelName, channel, connectionFactory, this);
											_ChannelHandlers.Add(queue.ChannelName, handler);
											Interlocked.Increment(ref _OpenChannels);
										}
										channel = handler.Channel;
										queue.Channel = channel;
										handler.AddQueue(queue);
										channel.QueueDeclare(queue.QueueName, queue.Durable, queue.Temporary, false, null);
										channel.QueueBind(queue.QueueName, DALRabbitMQ.EXCHANGE_NAME, queue.RoutingKey);
										channel.BasicConsume(queue.QueueName, false, queue.QueueName, handler);
									}
								}
							}
							_TriggerSubscriptionRequest.WaitOne();
						}

						// Handle shutdown
						lock (_ChannelHandlers)
						{
							foreach (ChannelHandler item in _ChannelHandlers.Values)
							{
								item.StopProcessingMessages();
								item.WaitForIdle(TimeSpan.FromSeconds(10));
								if (item.Channel.IsOpen)
									item.Channel.Close(200, "Goodbye");
							}
							_ChannelHandlers.Clear();
						}
						if (connection.IsOpen)
							connection.Close();
					}
					if (_ConnectionShutdown)
						connectionError = true;
				}
				catch (ThreadAbortException)
				{
					break;
				}
				catch (Exception ex)
				{
					connectionError = true;
					if (!nextErrorLogTime.HasValue || nextErrorLogTime.Value < DateTime.UtcNow)
					{
						StringBuilder errorMessage = new StringBuilder();
						errorMessage.AppendLine(ex.ToString());
						if (ex.InnerException != null)
						{
							errorMessage.AppendLine("\r\nInnerException:");
							errorMessage.AppendLine(ex.InnerException.ToString());
						}
						ApplicationEventLog.WriteEntry("Flow", string.Concat("ProcessSubscriptionRequests (", _Server.Uri.DnsSafeHost, "):\r\n", errorMessage.ToString()), System.Diagnostics.EventLogEntryType.Error);
						nextErrorLogTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(60));
					}
				}
				if (connectionError)
				{
					lock (_Queues)
					{
						_SubscriptionToAdd.Clear();
						foreach (KeyValuePair<string, MessageQueue> item in _Queues)
						{
							_SubscriptionToAdd.Enqueue(item.Value);
						}
					}
					lock (_ChannelHandlers)
					{
						_ChannelHandlers.Clear();
					}
					Thread.Sleep(500);
				}
			}
		}

		private void RespondToMessage(ServiceEventMessage message, MessageQueue messageQueue, AckMessageInfo ackMessageInfo)
		{
			try
			{
				messageQueue.Messages.Remove(ackMessageInfo.DeliveryID);
				if (ackMessageInfo.TotalNack == 0)
				{
					if (ackMessageInfo.TotalAck == 0)
						_DALRabbitMQ.Reject(message);
					if (messageQueue.Channel != null && messageQueue.Channel.IsOpen)
						messageQueue.Channel.BasicAck(ackMessageInfo.DeliveryTag, false);
				}
				else
				{
					if (message.Parameters.ContainsKey("RequeueCount"))
						message.Parameters["RequeueCount"] = (int)((long)(message.Parameters["RequeueCount"])) + 1;
					else
						message.Parameters.Add("RequeueCount", (int)1);
					_DALRabbitMQ.Requeue(message);
					if (!message.QueueAfterTime.HasValue)
						Thread.Sleep(50);
					if (messageQueue.Channel != null && messageQueue.Channel.IsOpen)
						messageQueue.Channel.BasicAck(ackMessageInfo.DeliveryTag, false);
					//_Model.BasicReject(ackMessageInfo.DeliveryTag, true);
				}
			}
			catch (Exception ex)
			{
				_ConnectionShutdown = true;
				_TriggerSubscriptionRequest.Set();
				ApplicationEventLog.WriteEntry("Flow", string.Concat("Failed to Ack/Nack", messageQueue.QueueName, ":\r\n", ex.ToString()), System.Diagnostics.EventLogEntryType.Error);
			}
		}

        public void Subscribe(string queueName, bool durable, bool temporary, string routingKey, MessageArrivedEventHandler handler)
		{
			lock (_Queues)
			{
				if (_SubscriptionRequestThread == null)
				{
					_SubscriptionRequestThread = new Thread(new ThreadStart(ProcessSubscriptionRequests));
					if (_SubscriptionRequestThread.Name == null)
						_SubscriptionRequestThread.Name = "ProcessMQSubscriptionRequests";
					_SubscriptionRequestThread.IsBackground = true;
					_SubscriptionRequestThread.Start();
				}
				if (_Queues.ContainsKey(queueName))
				{
					MessageQueue queue = _Queues[queueName];
					queue.Handlers.Add(handler);
				}
				else
				{
					MessageQueue queue = new MessageQueue();
                    queue.Durable = durable;
					queue.QueueName = queueName;
					queue.RoutingKey = routingKey;
                    queue.Temporary = temporary;
					queue.ChannelName = queue.QueueName.Split('.')[0];
					queue.Handlers.Add(handler);
					_Queues.Add(queueName, queue);
					_SubscriptionToAdd.Enqueue(queue);
					_TriggerSubscriptionRequest.Set();
				}
			}
		}

		public void ReSubscribe(List<string> queueNames)
		{
			lock (_Queues)
			{
				foreach (string queueName in queueNames)
				{
					MessageQueue queue;
					if (_UnsubscribedQueues.TryGetValue(queueName, out queue))
					{
						_UnsubscribedQueues.Remove(queueName);
						_Queues.Add(queueName, queue);
						_SubscriptionToAdd.Enqueue(queue);
					}
				}
			}
			_TriggerSubscriptionRequest.Set();
		}

		public void UnSubscribe(List<string> queueNames)
		{
			lock (_UnSubscribeQueues)
			{
				_UnSubscribeQueues.AddRange(queueNames);
			}
			foreach (string queueName in queueNames)
			{
				MessageQueue queue;
				if (_Queues.TryGetValue(queueName, out queue))
				{
					ChannelHandler handler;
					lock (_ChannelHandlers)
					{
						if (_ChannelHandlers.TryGetValue(queue.ChannelName, out handler))
						{
							handler.StopProcessingMessages();
						}
					}
				}
			}
			ThreadPool.QueueUserWorkItem(new WaitCallback(UnSubscribe));
		}

		private void UnSubscribe(object state)
		{
			List<string> queueNames = new List<string>();
			lock (_UnSubscribeQueues)
			{
				queueNames.AddRange(_UnSubscribeQueues);
				_UnSubscribeQueues.Clear();
			}
			List<string> deletedChannels = new List<string>();
			lock (_Queues)
			{
				foreach (string queueName in queueNames)
				{
					MessageQueue queue;
					if (_Queues.TryGetValue(queueName, out queue))
					{
						_UnsubscribedQueues.Add(queueName, queue);
						_Queues.Remove(queueName);
						ChannelHandler handler = null;
						lock (_ChannelHandlers)
						{
							if (_ChannelHandlers.TryGetValue(queue.ChannelName, out handler))
							{
								_ChannelHandlers.Remove(queue.ChannelName);
							}
						}
						if (handler != null)
						{
							handler.WaitForIdle(TimeSpan.FromSeconds(10));
							handler.Channel.Close();
							//channel.Dispose();
							if (!deletedChannels.Contains(queue.ChannelName))
								deletedChannels.Add(queue.ChannelName);
						}
					}
				}

				// Re-enable any queues that weren't unsubscribed from the same channel
				foreach (MessageQueue queue in _Queues.Values)
				{
					if (deletedChannels.Contains(queue.ChannelName))
						_SubscriptionToAdd.Enqueue(queue);
				}
				_TriggerSubscriptionRequest.Set();
			}
		}

		private void HandleModelShutdown(string channelName, List<MessageQueue> queues)
		{
			if (!_Terminate)
			{
				ChannelHandler handler = null;
				lock (_ChannelHandlers)
				{
					if (_ChannelHandlers.TryGetValue(channelName, out handler))
					{
						lock (_Queues)
						{
							foreach (MessageQueue item in queues)
							{
								_SubscriptionToAdd.Enqueue(item);
							}
						}
						_ChannelHandlers.Remove(channelName);
					}
				}
				_TriggerSubscriptionRequest.Set();
			}
			int openChannel = Interlocked.Decrement(ref _OpenChannels);
			if (openChannel == 0)
			{
				ApplicationEventLog.WriteEntry("Flow", string.Concat("RabbitMQSubscription - HandleModelShutdown recovery, channel = ", channelName), System.Diagnostics.EventLogEntryType.Warning);
				_ConnectionShutdown = true;
				_TriggerSubscriptionRequest.Set();
			}
		}

	}
}
