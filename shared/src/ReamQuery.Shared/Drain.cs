namespace ReamQuery.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Newtonsoft.Json;

    public class Drain
    {
        public readonly Guid Id;

        Subject<Tuple<string, string>> _columns = new Subject<Tuple<string, string>>();

        Subject<object> _values = new Subject<object>();
        
        string _currentName = null;
        IList<Tuple<string, string>> _currentColumns = new List<Tuple<string, string>>();

        IList<object> _currentValues = new List<object>();

        IList<DumpResult> _dumps = new List<DumpResult>();

        public Drain(Guid id)
        {
            Id = id;
            _columns.Subscribe(column => _currentColumns.Add(column));
            _values.Subscribe(val => _currentValues.Add(val));
        }

        public void Start(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name");
            }
            if (!string.IsNullOrEmpty(_currentName))
            {
                _dumps.Add(GetCurrent());
                _currentColumns = new List<Tuple<string, string>>();
                _currentValues = new List<object>();
            }
            _currentName = name;
        }

        public void EmitColumn(Tuple<string, string> column)
        {
            _columns.OnNext(column);
        }

        public void EmitValue(object val)
        {
            _values.OnNext(val);
        }

        public void Close()
        {
            Start("dump current values");
        }

        public IEnumerable<DumpResult> GetData()
        {
            return _dumps.AsEnumerable();
        }

        DumpResult GetCurrent()
        {
            var first = _currentValues.FirstOrDefault();
            var counter = first == null ? "null" : _currentValues.Count().ToString();
            return new DumpResult
            {
                Name = string.Format("{0} ({1})", _currentName, counter),
                Columns = _currentColumns.AsEnumerable(),
                Values = _currentValues.AsEnumerable(),
            };
        }
    }
}
