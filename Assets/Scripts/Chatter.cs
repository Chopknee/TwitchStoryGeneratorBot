using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Chatter {
    [SerializeField]
    public string userName;
    [SerializeField]
    public string actualName;
    [SerializeField]
    public bool isAuthorized = false;

    public Chatter(string message) {
        string[] mess = message.Split('!');
        userName = mess[0].TrimStart(':');
        actualName = mess[1].Split(' ')[0];
    }

    public override string ToString() {
        return userName + " " + actualName;
    }
}
