using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlLogSync.Tasks
{
    public abstract class SyncTask
    {
        public SyncTask()
        {
            
        }


        private Timer timer;

        public void RunWithInterval(int interval = 300) {

            timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(interval));
            
        }

        private void OnTimer(object obj) 
        {
            Task.Factory.StartNew(Run);
        }

        protected abstract void Execute();

        private bool _IsRunning;
        public bool IsRunning {
            get {
                return _IsRunning;
            }
        }

        public void Run() {
            lock (this) {
                if (_IsRunning)
                    return;
                _IsRunning = true;
            }

            try
            {
                Execute();
            }
            catch (Exception ex) {
                Utils.Log(ex.ToString());
            }
            finally { 
                lock(this){
                    _IsRunning = false;
                }
            }
        }

    }
}
