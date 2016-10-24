using System;
using System.Threading;

namespace NamedPipeWrapper.Threading
{
    class ThreadingWorker
    {
        public event PipeExceptionEventHandler Error;
        public event Action Succeeded;

        public void DoWork(Action action)
        {
            ThreadPool.QueueUserWorkItem(DoWorkImpl, action);
        }

        private void DoWorkImpl(object oAction)
        {
            var action = (Action)oAction;
            try
            {
                action();
                if (Succeeded != null)
                {
                    Succeeded();
                }
            }
            catch (Exception exception)
            {
                if (Error != null)
                {
                    Error(exception);
                }
            }
        }
    }
}
