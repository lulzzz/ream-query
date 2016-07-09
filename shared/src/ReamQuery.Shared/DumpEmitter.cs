namespace ReamQuery.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;

    public class DumpEmitter : IDisposable
    {
        public IObservable<Message> Messages = null;
        Subject<Message> _tables = new Subject<Message>();
        Subject<Message> _headers = new Subject<Message>();
        Subject<Message> _rows = new Subject<Message>();
        Subject<Message> _singulars = new Subject<Message>();
        Subject<Message> _nulls = new Subject<Message>();
        Subject<Message> _close = new Subject<Message>();
        int _tableCounter = 0;
        int _headerCounter = 0;
        
        public DumpEmitter()
        {
            Messages = _tables
                .Merge(_headers)
                .Merge(_rows)
                .Merge(_singulars)
                .Merge(_nulls)
                .Merge(_close);
        }

        public int Table(string title)
        {
            var id = Interlocked.Increment(ref _tableCounter);
            _tables.OnNext(new Message
            {
                Id = id,
                Type = ItemType.Table,
                Values = new object[] { title }
            });
            return id;
        }

        public int Header(IEnumerable<Column> columns, int tableId)
        {
            var id = Interlocked.Increment(ref _headerCounter);
            var msg = new Message
            {
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
            _rows.OnNext(new Message
            {
                Parent = headerId,
                Type = ItemType.Row,
                Values = values.ToArray()
            });
        }

        public void SingleAtomic(string title, object value)
        {
            _singulars.OnNext(new Message
            {
                Type = ItemType.SingleAtomic,
                Values = new object[] { title, value }
            });
        }

        public void SingleTabular(string title, IEnumerable<Column> columns, IEnumerable<object> values)
        {
            _singulars.OnNext(new Message
            {
                Type = ItemType.SingleTabular,
                Values = new object[] { title, columns, values }
            });
        }

        public void Null(string name)
        {
            _singulars.OnNext(new Message
            {
                Type = ItemType.Empty,
                Values = new object[] { name }
            });
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _close.OnNext(new Message
                    {
                        Type = ItemType.Close
                    });
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
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
