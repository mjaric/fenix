using System;

namespace Fenix
{
    public class Consts
    {
        public static readonly TimeSpan TimerPeriod = TimeSpan.FromMilliseconds(200);
        
        public static readonly TimeSpan DefaultReconnectionDelay = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan DefaultQueueTimeout = TimeSpan.Zero; // Unlimited
        public static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromSeconds(7);
        public static readonly TimeSpan DefaultOperationTimeoutCheckPeriod = TimeSpan.FromSeconds(1);
    }
}