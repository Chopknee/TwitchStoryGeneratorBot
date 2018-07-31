using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {

    //Three stages that need to be executed

    //Stage 1: chatstorming
    //Stage 2: elimination voting
    //Stage 3: end and possibly repeat

    enum BotState { IDLE, CHATSORM, ELIMINATION, END };

    ChatBot bot;

    BotState currentState = BotState.IDLE;

    //All times are in seconds
    
    float chatstormTime = 300;
    float eliminationRoundTime = 120;



	void Start () {
		
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
}
