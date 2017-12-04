namespace ReamQuery.Services
{
    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;

    public class WebSocketWriter : TextWriter
    {
        int count = 0;
        private StringBuilder buffer;

        public WebSocketWriter(WebSocket socket)
        {
            buffer = new StringBuilder();
        }

        public override void Write(char value)
        {
            buffer.Append(value);
            Interlocked.Add(ref this.count, 1);
        }
                
        public override void Write(string value)
        {
            buffer.Append(value);
            Interlocked.Add(ref this.count, value.Length);
        }

        protected override void Dispose(bool val)
        {
            Console.WriteLine("finish => {0}", GetValue().Length);
        }

        public string GetValue()
        {
            return buffer.ToString();
        }

        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }
    }
}