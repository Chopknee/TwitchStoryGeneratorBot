using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {

    //Three stages that need to be executed

    //Stage 1: chatstorming
    //Stage 2: elimination voting
    //Stage 3: end and possibly repeat

    public enum BotState { IDLE, CHATSORM, ELIMINATION, END };

    string[] commands = { "!pitch", "!vote", "!settheme", "!begin" };

    public ChatBot bot;
    public BotState currentState = BotState.IDLE;
    //All times are in seconds
    public float chatstormTime = 300;
    public float ideasLimit = 100;
    public float eliminationRoundTime = 120;
    public float stageTimeRemaining = 0;
    public string currentTheme = "";
    public float startTime = 0;
    public int eliminationRoundNumber = 0;
    public int eliminationRoundState = 0;
    public int eliminationRoundMax = 0;

    //public Dictionary<Chatter, List<string>> ideas;
    public List<Idea> ideasList;
    public List<Idea> deletedIdeas;

    public Dictionary<Chatter, int> votes;

    public delegate void ThemeSet(string theme);
    public ThemeSet OnThemeSet;

    public delegate void IdeaPhaseEntered();
    public IdeaPhaseEntered OnIdeaPhaseEntered;

    public delegate void IdeaPitched(Chatter sender, string idea);
    public IdeaPitched OnIdeaPitched;

    public delegate void IdeasPhaseOver();
    public IdeasPhaseOver OnIdeasPhaseOver;

    public delegate void ShowBracket(List<Idea> remainingIdeas);
    public ShowBracket OnShowBracket;

    public delegate void ShowVoteItems(string itemA, string itemB);
    public ShowVoteItems OnShowVoteItems;

    public delegate void ShowWinningVote(string winner);
    public ShowWinningVote OnShowWinningVote;

    public delegate void VoteAdded(Chatter sender, int option);
    public VoteAdded OnVoteAdded;

    public delegate void IdeaChosen(Chatter sender, string idea);
    public IdeaChosen OnIdeaChosen;

    bool transitioning = false;

	void Start () {
        bot.OnCommandReceived += OnCommandReceived;

        //ideas = new Dictionary<Chatter, List<string>>();
        ideasList = new List<Idea>();
        votes = new Dictionary<Chatter, int>();
        deletedIdeas = new List<Idea>();

	}
	
	void Update () {
		if (transitioning) {
            return;
        }
        switch (currentState) {
            case BotState.IDLE:
                //Nothing needs to happen here.
                break;
            case BotState.CHATSORM:
                stageTimeRemaining = (Time.fixedTime - startTime);
                //This is triggering the end of the chatstorming phase
                if (ideasList.Count > 0 && (ideasList.Count >= ideasLimit || stageTimeRemaining >= chatstormTime)) {
                    //Trigger the end of the ideas phase.
                    bot.SendPrivateMessage("Idea pitching phase is over!");
                    currentState = BotState.ELIMINATION;
                    if (OnIdeasPhaseOver != null) {
                        transitioning = true;
                        OnIdeasPhaseOver();
                    }
                }
                break;
            case BotState.ELIMINATION:
                switch (eliminationRoundState) {
                    case 0:
                        bot.SendPrivateMessage("Setting up bracket layer.");
                        //Initial beginning for calculating the constraints of the elimination round
                        eliminationRoundMax = Mathf.CeilToInt(ideasList.Count / 2.0f);
                        eliminationRoundState++;
                        break;
                    case 1:
                        eliminationRoundState++;
                        //Spend a moment to show the remaining bracket
                        if (OnShowBracket != null) {
                            transitioning = true;
                            OnShowBracket(ideasList);
                        }
                        break;
                    case 2:
                        //Once this has been reached, select 2 items to be voted for.
                        // Spend a moment to show the selections, then move on.
                        string itemA = ideasList[eliminationRoundNumber*2].idea;
                        string itemB = "";
                        if (eliminationRoundNumber * 2 + 1 < ideasList.Count) {
                            itemB = ideasList[eliminationRoundNumber * 2 + 1].idea;
                        }
                        bot.SendPrivateMessage("Voting for " + itemA + " or " + itemB);
                        startTime = Time.fixedTime;

                        eliminationRoundState++;
                        if (OnShowVoteItems != null) {
                            transitioning = true;
                            OnShowVoteItems(itemA, itemB);
                        }
                        break;
                    case 3:
                        stageTimeRemaining = (Time.fixedTime - startTime);
                        // Giving chat a chance to vote on the winning item here
                        // This is waiting to trigger the end of voting state
                        // and will show the winner and wait for a bit.
                        if (stageTimeRemaining >= eliminationRoundTime) {
                            int ones = 0;
                            int twos = 0;
                            //Tally the votes
                            foreach (KeyValuePair<Chatter, int> vote in votes) {
                                if (vote.Value == 1) {
                                    ones++;
                                } else if (vote.Value == 2) {
                                    twos++;
                                }
                            }

                            eliminationRoundState++;
                            eliminationRoundNumber++;

                            int winningIndex = -1;
                            int loosingIndex = -1;

                            if (ones > twos) {
                                winningIndex = eliminationRoundNumber * 2;
                                loosingIndex = eliminationRoundNumber * 2 + 1;

                            } else if (twos > ones) {
                                winningIndex = eliminationRoundNumber * 2 + 1;
                                loosingIndex = eliminationRoundNumber * 2;
                            }
                            if (winningIndex != -1) {
                                deletedIdeas.Add(ideasList[loosingIndex]);
                                Debug.Log("IM ACTUALLY RUNNING THIS, TWITCH IS BEING A TWAT. (A VOTE WON)");
                                bot.SendPrivateMessage("Voting has ended, the idea " + ideasList[winningIndex].idea + " pitched by " + ideasList[winningIndex].sender.userName + " has won the vote and moved on.");
                                //Need to delete the vote from the dictionary now...
                                if (OnShowWinningVote != null) {
                                    transitioning = true;
                                    OnShowWinningVote(ideasList[winningIndex].idea);//This needs to be the winning vote.
                                }
                            } else {
                                //Tiebreaker.
                                Debug.Log("IM ACTUALLY RUNNING THIS, TWITCH IS BEING A TWAT. (THERE WAS A TIE)");
                                bot.SendPrivateMessage("THERE WAS A TIE, CANNOT DEAL WITH SITUATION. SHUTTING DOWN....");
                                currentState = BotState.END;
                            }
                        }
                        break;
                    case 4:
                        if (eliminationRoundNumber >= eliminationRoundMax) {
                            // This layer of the elimination round is done with.
                            foreach (Idea idea in deletedIdeas) {
                                ideasList.Remove(idea);
                            }
                            deletedIdeas.Clear();

                            if (ideasList.Count > 1) {
                                eliminationRoundState = 0;
                            } else {
                                eliminationRoundState = 5;
                            }
                        } else {
                            // We are not done with the layer, go back to state 1
                            eliminationRoundState = 1;
                        }
                        break;
                    case 5:
                        //An idea was singled out, show it then end this!
                        currentState = BotState.END;
                        eliminationRoundState = 0;

                        if (OnIdeaChosen != null) {
                            transitioning = true;
                            OnIdeaChosen(ideasList[0].sender, ideasList[0].idea);
                        }
                        break;
                }
                break;
            case BotState.END:
                //At this point, an idea has won out over other ideas.
                //The streamer can choose to end this, or make another run with a different theme.
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
                OnCommandReceivedElimination(sender, command, args);
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
        //There are several sub-stages to this part.

        //0 - Set up the current bracket layer
            //1 - Show all remaining submissions for this part
            //2 - Select 2 submissions (in order of the list) to be voted against one-another
            //3 - Vote for the winner
            //4 - Show the winner
            //5 - If no items left in current bracket layer go to 6 else goto 1
        //6 if one item left in list go to 7 else go to 0
        //Repeat until only 1 submission remains. All others are erased from existance.

        //Simulating an elimination bracket....
        // Divide the number of submissions by two (rounded up)
        // Use that as the max number of elimination rounds.
        switch (eliminationRoundState) {
            case 1:
                //No commands can be received here.

                break;
            case 2:
                break;
            case 3:
                //Voting state.
                if (command == "!vote") {
                    if (!votes.ContainsKey(sender)) {
                        votes.Add(sender, -1);
                    }
                    if (args[1].ToLower() == "one" || args[1] == "1") {
                        //Voted a
                        votes[sender] = 1;
                        bot.SendPrivateMessage("@" + sender.userName + " has voted for option 1");
                    } else if (args[1].ToLower() == "two" || args[1] == "2") {
                        //Voted b
                        votes[sender] = 2;
                        bot.SendPrivateMessage("@" + sender.userName + " has voted for option 2");
                    } else {
                        //Invalid vote, do not set it? ( by default the vote is invalid )
                        //votes[sender] = -1;
                        bot.SendPrivateMessage("@" + sender.userName + " invalid vote argument either ( 1, 2, one, or two).");
                    }
                    //Report to the gui that a vote was added.
                    if (OnVoteAdded != null) {
                        OnVoteAdded(sender, votes[sender]);
                    }
                } else {
                    bot.SendPrivateMessage("@" + sender.userName + " invalid command.");
                }
                break;
        }
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
                ideasList.Add(new Idea(sender, idea));
                bot.SendPrivateMessage("@" + sender.userName + " your pitch of <i>\"" + idea + "\"</i> has been added!");
                OnIdeaPitched(sender, idea);
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
                    if (OnIdeaPhaseEntered != null) {
                        transitioning = true;
                        OnIdeaPhaseEntered();
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


    public struct Idea {
        public Chatter sender;
        public string idea;

        public Idea(Chatter sender, string idea) {
            this.sender = sender;
            this.idea = idea;
        }
    }
}
