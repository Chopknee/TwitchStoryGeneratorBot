using System.Threading;

namespace TwitchIRCBot {
    /*
    * Class that sends PING to irc server every 5 minutes
    */
    public class PingSender {
        private IRCClient _irc;
        private Thread pingSender;

        private int sleepTime;

        // Empty constructor makes instance of Thread
        public PingSender(IRCClient irc, int threadSleepTime) {
            _irc = irc;
            pingSender = new Thread(new ThreadStart(this.Run));
            this.sleepTime = threadSleepTime;
        }

        // Starts the thread
        public void Start() {
            pingSender.IsBackground = true;
            pingSender.Start();
        }

        // Send PING to irc server every 5 minutes
        public void Run() {
            while (true) {
                _irc.SendIRCMessage("PING irc.twitch.tv");
                Thread.Sleep(300000); // 5 minutes
            }
        }

        public void Stop() {
            pingSender.Abort();
        }
    }
}