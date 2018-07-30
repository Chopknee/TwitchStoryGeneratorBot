using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TwitchIRCBot {
    /*
    * Class that sends PING to irc server every 5 minutes
    */
    public class ChatGetter {
        private IRCClient _irc;
        private Thread chatGetter;

        private List<string> messages;
        private bool clearMessages = false;

        private int sleepTime;

        // Empty constructor makes instance of Thread
        public ChatGetter(IRCClient irc, int sleepTime) {
            _irc = irc;
            chatGetter = new Thread(new ThreadStart(this.Run));
            messages = new List<string>();
            this.sleepTime = sleepTime;
        }

        // Starts the thread
        public void Start() {
            try {
                Debug.Log("Starting chat polling....");
                chatGetter.IsBackground = true;
                chatGetter.Start();
            } catch (Exception ex) {
                Debug.Log(ex.Message);
            }
        }

        // Send PING to irc server every 5 minutes
        public void Run() {
            while (true) {
                try {
                    if (clearMessages) {
                        messages.Clear();
                        clearMessages = false;
                    }

                    string message = _irc.ReadMessage();
                    messages.Add(message);
                } catch (Exception ex) {
                    Debug.Log(ex.Message);
                }
                Thread.Sleep(sleepTime); 
            }
        }

        public void Stop() {
            chatGetter.Abort();
        }

        public List<string> GetMessages() {
            return new List<string>(messages);
        }

        public void FlushMessages() {
            clearMessages = true;
        }

        public bool Flushing() {
            return clearMessages;
        }
    }
}