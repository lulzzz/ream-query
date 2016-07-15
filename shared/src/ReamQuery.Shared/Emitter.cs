namespace ReamQuery.Shared
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using NLog;

    public class Emitter : IDisposable
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public IObservable<Message> Messages = null;
        Subject<Message> _tables = new Subject<Message>();
        Subject<Message> _headers = new Subject<Message>();
        Subject<Message> _rows = new Subject<Message>();
        Subject<Message> _singulars = new Subject<Message>();
        Subject<Message> _nulls = new Subject<Message>();
        Subject<Message> _close = new Subject<Message>();
        int _tableCounter = 0;
        int _headerCounter = 0;
        int _dumpCount;

        int _session;

        int _emittedCount = 0;
        
        public Emitter(int session, int dumpCount)
        {
            if (dumpCount < 0)
            {
                throw new ArgumentOutOfRangeException("dumpCount");
            }
            Logger.Debug("session {0}, dumpCount {1}", session, dumpCount);
            _dumpCount = dumpCount;
            _session = session;
            Messages = _tables
                .Merge(_headers)
                .Merge(_rows)
                .Merge(_singulars)
                .Merge(_nulls)
                .Merge(_close);
        }

        public bool Complete()
        {
            var remaining = Interlocked.Decrement(ref _dumpCount);
            var done = remaining <= 0;
            if (done)
            {
                Logger.Debug("Completed, emitted {0}", _emittedCount + 1);
                _close.OnNext(new Message
                {
                    Session = _session,
                    Type = ItemType.Close,
                    Values = new object[] { _emittedCount + 1 } 
                });
            }
            return done;
        }

        public int Table(string title)
        {
            Logger.Debug("Table {0}", title);
            Interlocked.Increment(ref _emittedCount);
            var id = Interlocked.Increment(ref _tableCounter);
            _tables.OnNext(new Message
            {
                Id = id,
                Session = _session,
                Type = ItemType.Table,
                Values = new object[] { title }
            });
            return id;
        }

        public int Header(IEnumerable<Column> columns, int tableId)
        {
            Logger.Debug("Header emitted, tableId {0}", tableId);
            Interlocked.Increment(ref _emittedCount);
            var id = Interlocked.Increment(ref _headerCounter);
            var msg = new Message
            {
                Session = _session,
                Id = id,
                Parent = tableId,
                Type = ItemType.Header,
                Values = columns.Cast<object>().ToArray()
            };
            _headers.OnNext(msg);
            return id;
        }

        public void Row(IEnumerable<object> values, int headerId)
        {
            Logger.Debug("Row emitted, headerId {0}", headerId);
            Interlocked.Increment(ref _emittedCount);
            _rows.OnNext(new Message
            {
                Session = _session,
                Parent = headerId,
                Type = ItemType.Row,
                Values = values.ToArray()
            });
        }

        public void SingleAtomic(Column title, object value)
        {
            Logger.Debug("SingleAtomic emitted, title {0}", title.Name);
            Interlocked.Increment(ref _emittedCount);
            _singulars.OnNext(new Message
            {
                Session = _session,
                Type = ItemType.SingleAtomic,
                Values = new object[] { title, value }
            });
        }

        public void SingleTabular(string title, IEnumerable<Column> columns, IEnumerable<object> values)
        {
            Logger.Debug("SingleTabular emitted, title {0}", title);
            Interlocked.Increment(ref _emittedCount);
            _singulars.OnNext(new Message
            {
                Session = _session,
                Type = ItemType.SingleTabular,
                Values = new object[] { title, columns, values }
            });
        }

        public void Null(Column title)
        {
            Logger.Debug("Null emitted, title {0}", title);
            Interlocked.Increment(ref _emittedCount);
            _singulars.OnNext(new Message
            {
                Session = _session,
                Type = ItemType.Empty,
                Values = new object[] { title }
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
                    _nulls.OnCompleted();
                    _headers.OnCompleted();
                    _rows.OnCompleted();
                    _singulars.OnCompleted();
                    _tables.OnCompleted();
                    _close.Dispose();
                    _nulls.Dispose();
                    _headers.Dispose();
                    _rows.Dispose();
                    _singulars.Dispose();
                    _tables.Dispose();
                }
                _close = null;
                _nulls = null;
                _headers = null;
                _rows = null;
                _singulars = null;
                _tables = null;
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
