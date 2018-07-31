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

    public delegate void IRCConnected();
    public IRCConnected ONIRCConnected;

    public delegate void NewChatter(Chatter c);
    public NewChatter OnNewChatter;

    public delegate void ChatReceived(Chatter sender, string messageContent);
    public ChatReceived OnChatReceived;

    public delegate void MessageReceived(string message);
    public MessageReceived OnMessageReceived;

    public delegate void CommandReceived(Chatter sender, string command, string[] args);
    public CommandReceived OnCommandReceived;

    public delegate void AuthorizedCommandReceived(Chatter sender, string command, string[] args);
    public AuthorizedCommandReceived OnAuthorizedCommandReceived;

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
                        if (OnNewChatter != null) {
                            OnNewChatter(newPerson);
                        }
                    }
                    string chat = message.Substring(message.IndexOf(" :") + 2);
                    if (chat.IndexOf('!') == 0) {
                        //Command
                        if (OnCommandReceived != null) {
                            string[] command = chat.Split(' ');
                            OnCommandReceived(ircChatters[userName], command[0], command);
                        }
                    } else {
                        //Regular chat
                        if (OnChatReceived != null) {
                            OnChatReceived(ircChatters[userName], chat);
                        }
                    }
                } else {
                    //General messages from the server
                    if (OnMessageReceived != null) {
                        OnMessageReceived(message);
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
        if (ONIRCConnected != null) {
            ONIRCConnected();
        }
    }

    public void OnDestroy() {
        _irc.PartChannel();
        _pingSender.Stop();
        _chatGetter.Stop();
    }

    public bool IsAuthorized(Chatter person) {
        foreach (Chatter c in authorizedChatters) {
            if (person.actualName == c.actualName) {
                return true;
            }
        }
        return false;
    }
}
