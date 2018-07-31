using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TEMPGUI : MonoBehaviour {

    public Button getNamesButton;

    public ChatBot chatbot;

    public Text connectionStatus;

    public RectTransform chattersPanel;

    private void Start() {
        chatbot.OnChatReceived += OnChat;
        chatbot.OnNewChatter += OnUserEntered;
        chatbot.ONIRCConnected += OnConnect;
        getNamesButton.onClick.AddListener(OnGetNamesClicked);

    }

    public void OnConnect() {
        //Display a message
        Debug.Log("Logged in!");
        if (connectionStatus != null) {
            connectionStatus.text = "Successfully connected to channel " + chatbot.ircChannelName + "!";
        }
    }

    public void OnUserEntered(Chatter sender) {
        //Add the name to the list
        if (chattersPanel != null) {
            GameObject text = Instantiate(connectionStatus.gameObject);
            text.GetComponent<Text>().text = sender.userName;
            text.transform.parent = chattersPanel;
        }
    }

    public void OnChat(Chatter sender, string message) {
        //Show the message
    }

    public void OnGetNamesClicked() {
        chatbot.SendPrivateMessage("No need to fear, Chopkneebot is here!");
    }
}
