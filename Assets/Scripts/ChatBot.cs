using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwitchIRCBot;
using System;

public class ChatBot : MonoBehaviour {

    public string twitchAccountName = "";
    public string ircChannelName = "";
    public string twitchOAuthKey = "";


    public string twitchIRCURL = "irc.twitch.tv";
    public int twitchIRCPortNumber = 6667;

    public int chatPollDelayMillis = 100;
    public int pingDelayMillis = 300000;

    //public float pollDelay = 0.1f;

    private IRCClient _irc;
    private ChatGetter _chatGetter;
    private PingSender _pingSender;

    public bool running = false;
    public bool runOnStartup = false;

    public Dictionary<string, Chatter> ircChatters;
    [SerializeField]
    public List<Chatter> authorizedChatters;

    public delegate void OnIRCConnected();
    public OnIRCConnected IRCConnected;

    public delegate void OnNewChatter(Chatter c);
    public OnNewChatter NewChatter;

    public delegate void OnChatReceived(Chatter sender, string messageContent);
    public OnChatReceived ChatReceived;

    public delegate void OnMessageReceived(string message);
    public OnMessageReceived MessageReceived;

	void Start () {
        ircChatters = new Dictionary<string, Chatter>();

        //Start the bot.
        if (runOnStartup) {
            StartBot();
        }
	}
	
	void Update () {
        List<string> messages = _chatGetter.GetMessages();
        if (messages.Count > 0 && !_chatGetter.Flushing()) {
            //There are messages
            //Flush old messages
            _chatGetter.FlushMessages();

            //Do stuff with new ones
            foreach (string message in messages) {
                Debug.Log(message);
                //As new messages come in, add the newly discovered users
                if (message.Contains("PRIVMSG")) {
                    //Look for the username in the message
                    int intIndexParseSign = message.IndexOf('!');
                    string userName = message.Substring(1, intIndexParseSign - 1);
                    if (!ircChatters.ContainsKey(userName)) {
                        Chatter newPerson = new Chatter(message);
                        Debug.Log("New person: " + newPerson);
                        ircChatters.Add(newPerson.userName, newPerson);
                        if (NewChatter != null) {
                            NewChatter(newPerson);
                        }
                    }
                    if (ChatReceived != null) {
                        ChatReceived(ircChatters[userName], message.Substring(message.IndexOf(" :") + 2));
                    }
                } else {
                    //General messages from the server
                    if (MessageReceived != null) {
                        MessageReceived(message);
                    }
                }
                
            }
        }
	}

    public void SendPrivateMessage(string message) {
        _irc.SendPublicChatMessage(message);
    }

    public void GetNames() {
        _irc.SendNamesCommand();
    }

    //Creates the ping and chat getter threads.
    private void StartBot() {
        Debug.Log("Creating irc object....");
        _irc = new IRCClient(twitchIRCURL, twitchIRCPortNumber, twitchAccountName, twitchOAuthKey, ircChannelName);
        Debug.Log("Creating ping thread object....");
        _pingSender = new PingSender(_irc, pingDelayMillis);
        Debug.Log("Starting ping thread....");
        _pingSender.Start();
        Debug.Log("Creating chat polling thread object....");
        _chatGetter = new ChatGetter(_irc, chatPollDelayMillis);
        Debug.Log("Starting chat polling thread....");
        _chatGetter.Start();
        if (IRCConnected != null) {
            IRCConnected();
        }
    }

    public void OnDestroy() {
        _irc.PartChannel();
        _pingSender.Stop();
        _chatGetter.Stop();
    }
}
