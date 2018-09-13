using ProxyRepeater.Server.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

            private const string ParameterNameForConsumerNewMsgs = "NumThreadsToConsumeNewMsgs";
            private const string ParameterNameForNumWorkers = "NumWorkersToDeliverMsgs";
            private readonly int _numThreadsToConsumeNewMsgs;
            public readonly int _numWorkersToDeliverMsgs;
            private const int _timeToWaitWhenEmptyQueue = 5000;

            private readonly ConcurrentDictionary<Guid , MsgPendingModel> _pendingMsgs = new ConcurrentDictionary<Guid , MsgPendingModel>();
            private readonly ConcurrentQueue<Guid> _msgsToDeliver = new ConcurrentQueue<Guid>();

            public Dispatcher()
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
                    }
                }
                else await Task.Delay(_timeToWaitWhenEmptyQueue);
            }

            private async void ProcessNewMessage()
            {
                if (NewMsgArrivals.TryDequeue(out var newMsg))
                {
                    var pendingMsg = new MsgPendingModel() { Message = newMsg };
                    foreach (ExClient item in Clients.Select(i => i.Value))
                        pendingMsg.ClientsToDeliver.Enqueue(item);
                    var guid = Guid.NewGuid();
                    _pendingMsgs[guid] = pendingMsg;
                    _msgsToDeliver.Enqueue(guid);
                }
                else await Task.Delay(_timeToWaitWhenEmptyQueue);
            }

            public ConcurrentQueue<string> NewMsgArrivals { get; set; }
            public IEnumerable<(Task Task, CancellationTokenSource CancellationSource)> NewMsgsConsumerTasks { get; private set; }
            public IEnumerable<(Task Task, CancellationTokenSource CancellationSource)> DeliverWorkerTasks { get; private set; }
            public IEnumerable<KeyValuePair<string , ExClient>> Clients { get; private set; }
        }

        protected Dispatcher _dispatcher = new Dispatcher();

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
                _dispatcher = new Dispatcher();

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
