using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TEMPGUI : MonoBehaviour {

    public Button getNamesButton;

    public ChatBot chatbot;

    private void Start() {
        getNamesButton.onClick.AddListener(OnGetNamesClicked);
    }


    public void OnGetNamesClicked() {
        chatbot.GetNames();
    }
}
