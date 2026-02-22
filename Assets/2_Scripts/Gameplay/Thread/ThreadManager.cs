using System.Threading;

namespace YGZFrameWork
{
    /// <summary>
    /// 线程管理
    /// ljs
    /// </summary>
    /// 
    public class ThreadManager
    {
        private static ThreadManager _instance = null;
        public static ThreadManager GetInstance()
        {
            if (_instance == null)
                _instance = new ThreadManager();
            return _instance;
        }
        public Thread StartThread(ThreadStart start)
        {
            Thread thread = new Thread(start);
            thread.Start();
            return thread;
        }
        public void StopThread(Thread thread)
        {
            if (thread != null) thread.Abort();
        }
    }
}