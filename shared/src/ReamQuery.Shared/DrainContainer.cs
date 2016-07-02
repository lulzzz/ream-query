namespace ReamQuery.Shared
{
    using System;
    using System.Collections.Concurrent;

    public static class DrainContainer
    {
        public static ConcurrentDictionary<Guid, Drain> Drains  = new ConcurrentDictionary<Guid, Drain>();

        public static Drain GetDrain(Guid id)
        {
            Drain drain;
            if (!Drains.ContainsKey(id))
            {
                drain = new Drain(id);
                Drains.TryAdd(id, drain);
            }
            Drains.TryGetValue(id, out drain);
            if (drain == null)
            {
                throw new Exception("drain was null");
            }
            return drain;
        }

        public static Drain CloseDrain(Guid id)
        {
            Drain drain;
            Drains.TryRemove(id, out drain);
            drain.Close();
            return drain;
        }
    }
}
