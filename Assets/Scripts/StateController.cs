using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {

    //Three stages that need to be executed

    //Stage 1: chatstorming
    //Stage 2: elimination voting
    //Stage 3: end and possibly repeat

    public enum BotState { IDLE, CHATSORM, ELIMINATION, END };

    string[] commands = { "!chatstorm", "!vote", "!settheme", "!begin" };


    public ChatBot bot;
    public BotState currentState = BotState.IDLE;

    //All times are in seconds
    
    public float chatstormTime = 300;
    public float ideasLimit = 100;
    public float eliminationRoundTime = 120;

    public string currentTheme = "";

    public float startTime = 0;

    public Dictionary<Chatter, List<string>> ideas;
    public int ideasCount = 0;

    public delegate void ThemeSet(string theme);
    public ThemeSet OnThemeSet;

    public delegate void IdeaPhaseEntered();
    public IdeaPhaseEntered OnIdeaPhaseEntered;

    public delegate void IdeaPitched(Chatter sender, string idea);
    public IdeaPitched OnIdeaPitched;

    public delegate void IdeasPhaseOver();
    public IdeasPhaseOver OnIdeasPhaseOver;

    bool transitioning = false;

	void Start () {
        bot.OnCommandReceived += OnCommandReceived;

        ideas = new Dictionary<Chatter, List<string>>();

	}
	
	void Update () {
		if (transitioning) {
            return;
        }
        switch (currentState) {
            case BotState.IDLE:
                break;
            case BotState.CHATSORM:
                if (ideasCount >= ideasLimit || (Time.fixedTime - startTime) >= chatstormTime) {
                    //Trigger the end of the ideas phase.
                    bot.SendPrivateMessage("Idea pitching phase is over!");
                    currentState = BotState.ELIMINATION;
                    transitioning = true;
                    if (OnIdeasPhaseOver != null) {
                        OnIdeasPhaseOver();
                    } else {
                        //Since nothing is handling this event, just move to the next phase
                        transitioning = false;
                    }
                }
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
        if (transitioning) {
            return;
        }
        switch (currentState) {
            case BotState.IDLE:
                OnCommandReceivedIdle(sender, command, args);
                break;

            case BotState.CHATSORM:
                OnCommandReceivedChatstorm(sender, command, args);
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

    /**
     * Function: OnCommandReceivedElimination(Chatter sender, string command, string[] args)
     * Purpose:
     * 
     */ 
    public void OnCommandReceivedElimination(Chatter sender, string command, string[] args) {

    }

    /**
     * Function: OnCommandReceivedChatstorm(Chatter sender, string command, string[] args)
     * Purpose:
     * Runs when a command is received during the "chatstorming" phase.
     * One possible unauthorized command -
     *  - !pitch (all args are a string of words)
     * 
     */
    public void OnCommandReceivedChatstorm(Chatter sender, string command, string[] args) {
        if (command == commands[0]) {
            //Pitching an idea
            if (args.Length > 1) {
                string idea = "";
                for (int i = 1; i < args.Length; i++) {
                    //
                    idea += args[i];
                    //Don't add a space after the last word.
                    if (i < args.Length - 1) {
                        idea += " ";
                    }
                }
                if (!ideas.ContainsKey(sender)) {
                    ideas.Add(sender, new List<string>());
                }
                ideas[sender].Add(idea);
                bot.SendPrivateMessage("@" + sender.userName + " your pitch of " + idea + " has been added!");
                OnIdeaPitched(sender, idea);
                ideasCount++;
            } else {
                bot.SendPrivateMessage("@" + sender.userName + " I could not add your idea. There was no content after the command.");
            }

            
        } else {
            bot.SendPrivateMessage("@" + sender.userName + " invalid command");
        }
    }

    /**
     * Function: OnCommandReceivedIdle(Chatter sender, string command, string[] args)
     * Purpose:
     * Runs when a command is received during the idle phase.
     * Two authorized commands - 
     *  - !settheme (all args are words that combine to make a theme)
     *  - !begin
     */ 
    public void OnCommandReceivedIdle(Chatter sender, string command, string[] args) {
        if (sender.isAuthorized) {
            //Control the state of the system
            if (command == commands[2]) {
                //Set the theme of the thing to be created
                if (args.Length > 1) {
                    //Since the theme can be multiple words the args needs to be recombinated into a string.
                    for (int i = 1; i < args.Length; i++) {
                        //
                        currentTheme += args[i];
                        //Don't add a space after the last word.
                        if (i < args.Length - 1) {
                            currentTheme += " ";
                        }
                    }
                    OnThemeSet(currentTheme);
                    bot.SendPrivateMessage("The theme has been set to " + currentTheme);
                } else {
                    bot.SendPrivateMessage("@" + sender.userName + " could not add theme, there was nothing after the command.");
                }
            } else if (command == commands[3]) {
                //Start the next phase up.
                if (currentTheme != "") {
                    currentState = BotState.CHATSORM;
                    bot.SendPrivateMessage("Chatstorming phase has begun. The theme is " + currentTheme);
                    startTime = Time.fixedTime;
                    transitioning = true;
                    if (OnIdeaPhaseEntered != null) {
                        OnIdeaPhaseEntered();
                    } else {
                        //Since nothing handles this event, move to the next phase.
                        transitioning = false;
                    }
                } else {
                    //Can't start no theme is set.
                    bot.SendPrivateMessage("@" + sender.userName + " unable to start, there is no set theme.");
                }
            } else {
                //Unknown command.
                bot.SendPrivateMessage("@" + sender.userName + " unknown command " + command + ".");
            }
        } else {
            bot.SendPrivateMessage("@" + sender.userName + " you are not authorized to run commands.");
        }
    }

    /**
     * Function: EndTransition()
     * Purpose:
     * After each phase has ended, a transitioning period begins and the script is disabled.
     * This function ends that period and allows the script to continue running.
     * 
     */
    public void EndTransition() {
        transitioning = false;
    }
}
