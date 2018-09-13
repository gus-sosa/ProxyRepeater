using Flurl.Http;
using Polly;
using ProxyRepeater.Server.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyRepeater.Server.Implementations
{
    public class MsgClientDispatcher : IExchanger
    {
        protected class Dispatcher
        {
            private class MsgPendingModel
            {
                public string Message { get; set; }
                public Queue<ExClient> ClientsToDeliver { get; set; } = new Queue<ExClient>();
            }

            private class NonExistingClientException : Exception
            {
            }

            private const string ParameterNameForConsumerNewMsgs = "NumThreadsToConsumeNewMsgs";
            private const string ParameterNameForNumWorkers = "NumWorkersToDeliverMsgs";
            private readonly int _numThreadsToConsumeNewMsgs;
            private readonly int _numWorkersToDeliverMsgs;
            private const int _timeToWaitBeforeRetryInMs = 10000;
            private const int _numTimesToRetry = 3;
            private const int _timeToWaitWhenEmptyQueueInMs = 5000;
            private const int _callsTimeoutInMs = 30;
            private readonly HttpStatusCode[] httpStatusCodesWorthRetrying = new[]{
               HttpStatusCode.RequestTimeout, // 408
               HttpStatusCode.InternalServerError, // 500
               HttpStatusCode.BadGateway, // 502
               HttpStatusCode.ServiceUnavailable, // 503
               HttpStatusCode.GatewayTimeout // 504
            };

            private readonly ConcurrentDictionary<Guid , MsgPendingModel> _pendingMsgs = new ConcurrentDictionary<Guid , MsgPendingModel>();
            private readonly ConcurrentQueue<Guid> _msgsToDeliver = new ConcurrentQueue<Guid>();

            public Dispatcher(MsgClientDispatcher msgClientDispatcher)
            {
                void SetConfigurationValueByParameterName<T>(string parameterNameForConsumerNewMsgs , T defaultValue , ref T variable)
                {
                    variable = defaultValue;
                }

                IEnumerable<(Task, CancellationTokenSource)> CreateThreadWorkers(int numThreads , Action function)
                {
                    var list = new List<(Task, CancellationTokenSource)>();
                    for (var i = 0 ; i < numThreads ; i++)
                    {
                        var cancellationTokenSource = new CancellationTokenSource();
                        list.Add((new Task(function , cancellationTokenSource.Token), cancellationTokenSource));
                    }

                    return list;
                }

                SetConfigurationValueByParameterName(ParameterNameForConsumerNewMsgs , 1 , ref _numThreadsToConsumeNewMsgs);
                SetConfigurationValueByParameterName(ParameterNameForNumWorkers , 3 , ref _numWorkersToDeliverMsgs);

                NewMsgsConsumerTasks = CreateThreadWorkers(_numThreadsToConsumeNewMsgs , ProcessNewMessage);
                DeliverWorkerTasks = CreateThreadWorkers(_numWorkersToDeliverMsgs , DeliverMsg);
                MsgClientDispatcher = msgClientDispatcher;
            }

            private async void DeliverMsg()
            {
                if (_msgsToDeliver.TryDequeue(out Guid msgGuid))
                {
                    if (_pendingMsgs.TryGetValue(msgGuid , out MsgPendingModel msgPending))
                    {
                        ExClient clientToDeliverMsg = msgPending.ClientsToDeliver.Dequeue();
                        if (msgPending.ClientsToDeliver.Count > 0) _msgsToDeliver.Enqueue(msgGuid);
                        else _pendingMsgs.TryRemove(msgGuid , out _);

                        PolicyResult<HttpResponseMessage> httpResponseMsg = await Policy
                            .Handle<HttpRequestException>()
                            .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                            .WaitAndRetry(Enumerable.Repeat(TimeSpan.FromMilliseconds(_timeToWaitBeforeRetryInMs) , _numTimesToRetry) , (exception , retryCount , context) =>
                              {
                                  //TODO-LOG: Log error + count 
                              })
                              .ExecuteAndCaptureAsync(async () => await Task.Factory.StartNew(() =>
                              {
                                  MsgClientDispatcher.Clients.TryGetValue(clientToDeliverMsg.Name , out ExClient clientToCompare);
                                  if (clientToCompare == null || clientToCompare != clientToDeliverMsg)
                                      throw new NonExistingClientException();
                                  return SendMsg(msgPending.Message , clientToDeliverMsg).Result;
                              }));

                        if (httpResponseMsg.Outcome == OutcomeType.Failure && !(httpResponseMsg.FinalException is NonExistingClientException))
                        {
                            //TODO-LOG: Log error
                            MsgClientDispatcher.Clients.TryRemove(clientToDeliverMsg.Name , out _);
                        }
                    }
                }
                else await Task.Delay(_timeToWaitWhenEmptyQueueInMs);
            }

            private async Task<HttpResponseMessage> SendMsg(string message , ExClient client) => await $"https://{client.IpAddress.ToString()}:{client.Port}"
                    .PostJsonAsync(new { Message = message });

            private async void ProcessNewMessage()
            {
                if (NewMsgArrivals.TryDequeue(out var newMsg))
                {
                    var pendingMsg = new MsgPendingModel() { Message = newMsg };
                    foreach (ExClient item in MsgClientDispatcher.Clients.Values)
                        pendingMsg.ClientsToDeliver.Enqueue(item);
                    var guid = Guid.NewGuid();
                    _pendingMsgs[guid] = pendingMsg;
                    _msgsToDeliver.Enqueue(guid);
                }
                else await Task.Delay(_timeToWaitWhenEmptyQueueInMs);
            }

            public ConcurrentQueue<string> NewMsgArrivals { get; set; }
            public IEnumerable<(Task Task, CancellationTokenSource CancellationSource)> NewMsgsConsumerTasks { get; private set; }
            public IEnumerable<(Task Task, CancellationTokenSource CancellationSource)> DeliverWorkerTasks { get; private set; }
            public MsgClientDispatcher MsgClientDispatcher { get; }
        }

        protected Dispatcher _dispatcher;

        public ConcurrentDictionary<string , ExClient> Clients { get; set; }

        public ErrorNumber AddClient(ExClient client) => Clients.TryAdd(client.Name , client) ? ErrorNumber.NoError : ErrorNumber.NoError;

        public void ClearClients() => Clients.Clear();

        public ErrorNumber DeleteClient(ExClient client) => Clients.TryRemove(client.Name , out _) ? ErrorNumber.NoError : ErrorNumber.ClientDoesNotExist;

        public IEnumerable<ExClient> GetClients() => Clients.Values;

        public void DeliverMessage(IClientMsg msg) => _dispatcher?.NewMsgArrivals.Enqueue(msg.GetMessage());

        public void StartDeliverProcess()
        {
            void StartTasks(IEnumerable<Task> tasks)
            {
                foreach (Task item in tasks)
                    item.Start();
            }

            if (_dispatcher == null)
                _dispatcher = new Dispatcher(this);

            StartTasks(_dispatcher.NewMsgsConsumerTasks.Select(i => i.Task));
            StartTasks(_dispatcher.DeliverWorkerTasks.Select(i => i.Task));
        }

        public void StopDeliverProcess()
        {
            void StopTask(IEnumerable<CancellationTokenSource> tokens)
            {
                foreach (CancellationTokenSource item in tokens)
                    item.Cancel();
            }

            if (_dispatcher == null)
                return;

            StopTask(_dispatcher.DeliverWorkerTasks.Select(i => i.CancellationSource));
            StopTask(_dispatcher.NewMsgsConsumerTasks.Select(i => i.CancellationSource));
            _dispatcher = null;
        }

        public void RestartDeliverProcess()
        {
            StopDeliverProcess();
            StartDeliverProcess();
        }
    }
}
