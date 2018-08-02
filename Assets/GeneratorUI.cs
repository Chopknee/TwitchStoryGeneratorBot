using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour {


    public StateController stateControllerScript;

    public Text themeText;
    public Text timerText;
    public GameObject ideaPitchPrefab;
    public GameObject fallingNamePrefab;
    public GameObject pitchPanel;
    public bool timerShowing = false;

    public List<GameObject> ideasObjects;

	/**
     * Function: Start()
     * Purpose:
     *  Adds event handlers to the different phases in the "game"
     */
	void Start () {

        ideasObjects = new List<GameObject>();

        stateControllerScript.OnThemeSet += OnThemeSet;
        stateControllerScript.OnIdeaPhaseEntered += OnChatstormingEntered;
        stateControllerScript.OnIdeaPitched += OnIdeaPitched;
        stateControllerScript.OnIdeasPhaseOver += OnChatstormingEnded;
	}
	
	//Not sure if this is needed right now.
	void Update () {
		if (timerShowing) {
            float remainingtime = Time.fixedTime - stateControllerScript.startTime;
            timerText.text = FormatTimeString(remainingtime);//This is actually counting up right now.
        }
	}

    public void OnThemeSet(string theme) { 
        themeText.text = "Current Theme: " + theme;
    }

    public void OnChatstormingEntered() {
        //A fun transition to the chatstorming phase here?
        stateControllerScript.EndTransition();
        timerShowing = true;
    }

    public void OnIdeaPitched(Chatter sender, string idea) {
        RectTransform pan = pitchPanel.GetComponent<RectTransform>();
        //A fun animation that will be entertaining to watch

        GameObject pitch = Instantiate(ideaPitchPrefab);
        GameObject name = Instantiate(fallingNamePrefab);
        ideasObjects.Add(pitch);
        pitch.transform.SetParent(pitchPanel.transform);
        name.transform.SetParent(pitchPanel.transform);
        Vector3 pos = new Vector3(Random.value * pan.rect.width, Random.value * pan.rect.height, 0);
        name.transform.position = pos;
        pitch.transform.position = pos;
        pitch.GetComponent<Text>().text = idea;
        name.GetComponent<Text>().text = sender.userName;
    }


    public void OnChatstormingEnded() {
        //A fun transition to the end of this phase.
        stateControllerScript.EndTransition();
        timerShowing = false;
    }

    public string FormatTimeString(float seconds) {
        string text = "";
        float secs = seconds % 60;
        float mins = Mathf.Floor(seconds / 60);
        text = secs + ":" + mins;
        return text;
    }
}

//Exposed events
//public ThemeSet OnThemeSet;
//public IdeaPhaseEntered OnIdeaPhaseEntered;
//public IdeaPitched OnIdeaPitched;
//public IdeasPhaseOver OnIdeasPhaseOver;
//public ShowBracket OnShowBracket;
//public ShowVoteItems OnShowVoteItems;
//public ShowWinningVote OnShowWinningVote;
//public VoteAdded OnVoteAdded;
//public IdeaChosen OnIdeaChosen;
