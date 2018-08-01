using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour {


    public StateController stateControllerScript;

    public Text themeText;

	/**
     * Function: Start()
     * Purpose:
     *  Adds event handlers to the different phases in the "game"
     */
	void Start () {
        stateControllerScript.OnThemeSet += OnThemeSet;
        stateControllerScript.OnIdeaPhaseEntered += OnChatstormingEntered;
        stateControllerScript.OnIdeaPitched += OnIdeaPitched;
        stateControllerScript.OnIdeasPhaseOver += OnChatstormingEnded;
	}
	
	//Not sure if this is needed right now.
	void Update () {
		
	}

    public void OnThemeSet(string theme) { 
        themeText.text = "Current Theme: " + theme;
    }

    public void OnChatstormingEntered() {
        //A fun transition to the chatstorming phase here?
        stateControllerScript.EndTransition();
    }

    public void OnIdeaPitched(Chatter sender, string idea) {
        //A fun animation that will be entertaining to watch
    }

    public void OnChatstormingEnded() {
        //A fun transition to the end of this phase.
        stateControllerScript.EndTransition();
    }
}
