using System;

namespace AutoTricklerGui
{
    internal class Semaphore
    {
        private int threads = 0;
        private int maxThreads;
        public Semaphore(int maxThreads) { 
            this.maxThreads = maxThreads;
        }

        public bool isThreadAvailable() {
            if(maxThreads > threads) return true;
            return false;
        }

        public void increase() {
            if(threads >= maxThreads) {
                throw new Exception("Maximale Anzahl an Threads ist erreicht!");
            }
            threads++;
        }

        public void decrease() {
            if (threads <= 0) {
                throw new Exception("Derzeit sind keine Threads gebucht!");
            }
            threads--;
        }
    }
}
