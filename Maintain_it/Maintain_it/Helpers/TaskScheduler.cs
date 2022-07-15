using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Maintain_it.Helpers
{
    internal class TaskScheduler
    {
        public static TaskScheduler Scheduler
        {
            get => Scheduler ??= new TaskScheduler();
            private set => Scheduler = value;
        }

        private bool working = false;

        //private ConcurrentQueue< , KeyValuePair<Type, object[]>> taskQueue = new ConcurrentQueue<object, KeyValuePair<Type, object[]>>();

        //public async Task EnqueueTask( TaskPair taskPair )
        //{
        //    taskQueue.Enqueue(taskPair);
        //    await ConsumeQueue();
        //}

        private async Task<object> ConsumeQueue()
        {
            if( working) return null;

            working = true;

            //if( taskQueue.TryDequeue( out TaskPair current ) )
            //{
            //    Func<object[], Task> t = current.Key;
            //    Task<object> result = await t( current.Value ).ContinueWith( t => ConsumeQueue() );
            //    working = false;
            //    return result.Result;
            //}

            working = false;
            return null;
        }
    }
}
