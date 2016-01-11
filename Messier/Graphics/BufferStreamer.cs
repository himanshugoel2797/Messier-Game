using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
{
    public static class BufferStreamer
    {
        static ConcurrentQueue<Tuple<Action<object>, object>> taskQ;

        static BufferStreamer()
        {
            taskQ = new ConcurrentQueue<Tuple<Action<object>, object>>();
        }

        public static void QueueTask(Action<object> t, object o)
        {
            taskQ.Enqueue(new Tuple<Action<object>, object>(t, o));
        }

        internal static void ExecuteTasks(long time)
        {
            Stopwatch s = Stopwatch.StartNew();
            while (taskQ.Count > 0 && s.ElapsedMilliseconds + time < 35)
            {
                Tuple<Action<object>, object> tmp;
                if (taskQ.TryDequeue(out tmp))
                {
                    tmp.Item1(tmp.Item2);
                }
            }

        }

    }
}
