using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour {

    //The controller object for the logic of the interaction.
    public StateController stateControllerScript;

    public GameObject pitchingPanel;
    PitchingPanel pitchPanelScript;

    public GameObject votingPanel;
    VotingPanel votingPanelScript;

    public Text themeText;
    public Text timerText;
    public bool timerShowing = false;

    public Text stageText;

    public List<GameObject> ideasObjects;

    public AudioClip kick;

    public float chatstormEndTransitionTime = 20;//In seconds
    public float bracketShowTime = 10;//In seconds
    public float winningVoteShowTime = 6;//In seconds

    public float currentTimeLimit = 0;

	/**
     * Function: Start()
     * Purpose:
     *  Adds event handlers to the different phases in the "game"
     */
	void Start () {

        ideasObjects = new List<GameObject>();
        //Set up all event handlers
        stateControllerScript.OnThemeSet += OnThemeSet;
        stateControllerScript.OnIdeaPhaseEntered += OnChatstormingEntered;
        stateControllerScript.OnIdeaPitched += OnIdeaPitched;
        stateControllerScript.OnIdeasPhaseOver += OnChatstormingEnded;
        stateControllerScript.OnShowBracket += ShowBracketStatus;
        stateControllerScript.OnShowVoteItems += ShowVotingItems;
        stateControllerScript.OnVoteAdded += OnVoteAdded;
        stateControllerScript.OnVoteRemoved += OnVoteRemoved;
        stateControllerScript.OnShowWinningVote += ShowWinningVote;
        stateControllerScript.OnIdeaChosen += IdeaChosen;

        stageText.text = "Stage: Waiting for Streamer";

        pitchPanelScript = pitchingPanel.GetComponent<PitchingPanel>();
        votingPanelScript = votingPanel.GetComponent<VotingPanel>();
    }
	
	//Not sure if this is needed right now.
	void Update () {
		if (timerShowing) {
            float remainingtime = currentTimeLimit - (Time.fixedTime - stateControllerScript.startTime);
            timerText.text = "Time Remaining: " + FormatTimeString(remainingtime);//This is actually counting up right now.
        }
	}

    public void OnThemeSet(string theme) { 
        themeText.text = "Current Theme: " + theme;
    }

    public void OnChatstormingEntered() {
        stageText.text = "Stage: Chatstorming";
        //A fun transition to the chatstorming phase here?
        stateControllerScript.EndTransition();
        timerShowing = true;
        currentTimeLimit = stateControllerScript.chatstormTime;
        pitchingPanel.SetActive(true);
    }

    /**
     * Function: OnIdeaPitched(Chatter sender, string idea)
     * Purpose:
     *  Runs when an idea has been sent in by a chatter.
     */
    public void OnIdeaPitched(Chatter sender, string idea) {
        pitchPanelScript.AddPitch(sender.userName, idea);
        GetComponent<AudioSource>().clip = kick;
        GetComponent<AudioSource>().Play();
    }


    /**
     * Function: OnChastormingEnded()
     * Purpose:
     *  Runs at the conclusion of the chastorming phase. This gives a chance for the chat to
     *  review the ideas pitched and ready for the next phase.
     * 
     */
    public void OnChatstormingEnded() {
        //A fun transition to the end of this phase.
        timerShowing = false;
        timerText.text = "Next Phase Will Begin Shortly...";
        Invoke("EndChastormTransition", chatstormEndTransitionTime);
        stageText.text = "Stage: Chatstorm Review";
    }

    /**
     * Function: EndChastormTransition()
     * Purpose:
     *  Runs after a short delay to re-enable the control logic and ready for the next phase.
     *  This hides the pitching panel and shows the bracket panel.
     */
    public void EndChastormTransition() {
        pitchingPanel.SetActive(false);
        votingPanel.SetActive(true);
        stateControllerScript.EndTransition();
        stageText.text = "Stage: ELIMINATION";
    }

    /**
     * Function: ShowBracketStatus()
     * Purpose:
     *  Each elimination round, this gives the chatters a chance to review
     *  the current set of ideas remaining.
     */
    public void ShowBracketStatus(List<StateController.Idea> remainingIdeas) {
        votingPanelScript.ShowBracketStatus(remainingIdeas);
        Invoke("EndBracketTransition", bracketShowTime);
        timerText.text = "Voting Will Begin Shortly...";
    }

    public void EndBracketTransition() {
        //Do some more things...
        stateControllerScript.EndTransition();
    }

    /**
     * Function: ShowVotingItems()
     * Purpose:
     *  Gives the chatters a moment to see the current items for voting on.
     * 
     */
    public void ShowVotingItems(string itemA, string itemB) {
        votingPanelScript.SetUpVote(itemA, itemB);
        timerShowing = true;
        currentTimeLimit = stateControllerScript.eliminationRoundTime;
        stateControllerScript.EndTransition();
    }

    /**
     * Function: ShowWinningVote()
     * Purpose:
     *  After each elimination round, this will give the winner a chance to 'gloat'
     * 
     */
    public void ShowWinningVote(string winner) {
        timerShowing = false;
        timerText.text = "Voting ended, next round begins shortly...";
        votingPanelScript.OnShowWinningIdea(winner);
        Invoke("EndShowWinningVote", winningVoteShowTime);
    }

    public void EndShowWinningVote() {
        stateControllerScript.EndTransition();
    }

    public void IdeaChosen(Chatter sender, string idea) {
        votingPanelScript.OnShowWinningIdea(idea);
        Invoke("EndShowWinningVote", winningVoteShowTime);
    }

    /**
     * Function: OnVoteAdded()
     * Purpose:
     *  Runs each time an idea is voted on.
     * 
     */
    public void OnVoteAdded(Chatter sender, int option) {
        votingPanelScript.VoteAdded(sender.userName, option);
    }

    public void OnVoteRemoved(Chatter sender, int option) {
        votingPanelScript.VoteRemoved(sender.userName, option);
    }

    public string FormatTimeString(float seconds) {
        string text = "";
        float secs = seconds % 60;
        float mins = Mathf.Floor(seconds / 60);
        text = mins.ToString("00") + ":" + secs.ToString("00");
        return text;
    }
}

//Exposed events
//public ThemeSet OnThemeSet; --
//public IdeaPhaseEntered OnIdeaPhaseEntered; --
//public IdeaPitched OnIdeaPitched; --
//public IdeasPhaseOver OnIdeasPhaseOver; --
//public ShowBracket OnShowBracket; --
//public ShowVoteItems OnShowVoteItems; --
//public ShowWinningVote OnShowWinningVote; --
//public VoteAdded OnVoteAdded; --
//public IdeaChosen OnIdeaChosen; --