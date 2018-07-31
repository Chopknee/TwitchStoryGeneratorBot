using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {

    //Three stages that need to be executed

    //Stage 1: chatstorming
    //Stage 2: elimination voting
    //Stage 3: end and possibly repeat

    enum BotState { IDLE, CHATSORM, ELIMINATION, END };

    string[] commands = { "!chatstorm", "!vote" };

    ChatBot bot;

    BotState currentState = BotState.CHATSORM;

    //All times are in seconds
    
    float chatstormTime = 300;
    float eliminationRoundTime = 120;



	void Start () {
        bot.OnCommandReceived += OnCommandReceived;
	}
	
	void Update () {
		
        switch (currentState) {
            case BotState.IDLE:
                break;
            case BotState.CHATSORM:
                break;
            case BotState.ELIMINATION:
                break;
            case BotState.END:
                break;
        }

	}

    /**
     * Runs when a command has been received from a chatter in the IRC channel.
     * 
     */
    public void OnCommandReceived(Chatter sender, string command, string[] args) {


        switch (currentState) {
            case BotState.IDLE:

                break;
            case BotState.CHATSORM:
                if (command == commands[0]) {
                    //Pitching an idea
                    bot.SendPrivateMessage("@" + sender.userName + " your pitch has been added!");
                } else {
                    bot.SendPrivateMessage("@" + sender.userName + " invalid command");
                }
                break;
            case BotState.ELIMINATION:
                if (command == commands[1]) {
                    //Voting for something
                    bot.SendPrivateMessage("Thanks @" + sender.userName + " your voice has been heard!");
                } else {
                    bot.SendPrivateMessage("@" + sender.userName + " invalid command");
                }
                break;
            case BotState.END:
                
                break;
        }
    }
}
