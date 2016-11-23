namespace ReamQuery.Core
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using ReamQuery.Core.Api;
    using NLog;

    public class Emitter : IDisposable
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public IObservable<Message> Messages = null;
        Subject<Message> _lists = new Subject<Message>();
        Subject<Message> _listClosings = new Subject<Message>();
        Subject<Message> _listValues = new Subject<Message>();
        Subject<Message> _singulars = new Subject<Message>();
        Subject<Message> _close = new Subject<Message>();
        
        static int _tableCounter = 0;

        public readonly Guid Session;

        int _emittedCount = 0;
        bool _completed = false;
        DateTime _started = DateTime.Now;
        
        public Emitter(Guid session)
        {
            Logger.Debug("session: {0}", session);
            Session = session;
            var stream = _lists
                .Merge(_listClosings)
                .Merge(_listValues)
                .Merge(_singulars)
                .Merge(_close)
                .Publish();
            
            Messages = stream;
            stream.Connect();
        }

        public void Complete()
        {
            if (_completed)
            {
                throw new InvalidOperationException("completed");
            }
            _close.OnNext(new Message
            {
                Session = Session,
                Type = ItemType.Close,
                Values = new object[] { _emittedCount + 1, (DateTime.Now - _started).TotalMilliseconds  } 
            });
        }

        public int List(object firstRow, string title)
        {
            Interlocked.Increment(ref _emittedCount);
            var id = Interlocked.Increment(ref _tableCounter);
            _lists.OnNext(new Message
            {
                Session = Session,
                Id = id,
                Type = ItemType.List,
                Title = title,
                Values = new object[] { firstRow }
            });
            return id;
        }



        public void ListValues(IEnumerable<object> rows, int listId)
        {
            Interlocked.Increment(ref _emittedCount);
            _listValues.OnNext(new Message
            {
                Session = Session,
                Id = listId,
                Type = ItemType.ListValues,
                Values = rows
            });
        }

        public void ListClose(int listId)
        {
            Interlocked.Increment(ref _emittedCount);
            _listClosings.OnNext(new Message
            {
                Session = Session,
                Id = listId,
                Type = ItemType.ListClose
            });
        }

        public void Single(object value, string title)
        {
            Interlocked.Increment(ref _emittedCount);
            _singulars.OnNext(new Message
            {
                Session = Session,
                Type = ItemType.Single,
                Title = title,
                Values = new object[] { value }
            });
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _close.OnCompleted();
                    _listValues.OnCompleted();
                    _singulars.OnCompleted();
                    _lists.OnCompleted();
                    _close.Dispose();
                    _listValues.Dispose();
                    _singulars.Dispose();
                    _lists.Dispose();
                }
                _close = null;
                _listValues = null;
                _singulars = null;
                _lists = null;
                disposedValue = true;
                Logger.Debug("Disposed ok");
            }
        }

        public void Dispose()
        {
            Logger.Debug("Disposing emitter");
            Dispose(true);
        }
    }
}
