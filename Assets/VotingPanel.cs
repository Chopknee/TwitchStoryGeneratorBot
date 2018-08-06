using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This is purely gui stuff.
//Mess with it at your own peril
public class VotingPanel : MonoBehaviour {

    public GameObject bracketPanel;
    public GameObject eliminationPanel;
    public GameObject winningPanel;

    public GameObject verticalLayoutPanels;
    public GameObject bracketItemLabel;

    public int bracketItemsPerColumn = 8;

    public List<GameObject> bracketObjects;

    public Text itemA, votesA, itemB, votesB, winningIdea;

    int avotes = 0;
    int bvotes = 0;

    public void Start() {
        bracketObjects = new List<GameObject>();
    }

    int lastCount = 0;
    int currentRound = 0;

    public void ShowBracketStatus(List<StateController.Idea> ideas) {
        currentRound++;
        if (lastCount != ideas.Count) {
            currentRound = 0;
            lastCount = ideas.Count;
        }
        HidePanels();
        bracketPanel.SetActive(true);
        KillBracketObjects();//In case anything is left from the previous run, reset all objects.
        KillBracketObjects();
        int bracketLayers = Mathf.CeilToInt(ideas.Count / 2.0f);

        int row = 0;
        GameObject currentColumnPanel = NewVerticalLayoutPanel();
        for (int i = 0; i < bracketLayers; i++) {

            //Idea 1 is simply the one at i * 2
            string idea1 = ideas[i * 2].idea;
            //Idea 2 is possibly non-existant due to the possibility of an odd number of ideas remaining
            //This ensures that it will not try to pull from an index that does not exist.
            string idea2 = (i * 2 + 1 < ideas.Count) ? "\nVS\n" + ideas[i * 2 + 1].idea : "";

            GameObject bracketItem = Instantiate(bracketItemLabel);
            bracketItem.GetComponent<Text>().text = idea1 + idea2;
            bracketItem.transform.parent = currentColumnPanel.transform;
            bracketObjects.Add(bracketItem);
            if (i == currentRound) {
                bracketItem.GetComponent<Text>().color = new Color(1, 0, 0);
            }

            //Increment the row number
            row++;
            //If the row is maxed out, create a new column
            if (row >= bracketItemsPerColumn) {
                currentColumnPanel = NewVerticalLayoutPanel();
            }
        }
    }

    //Creates a new column
    public GameObject NewVerticalLayoutPanel() {
        GameObject g = Instantiate(verticalLayoutPanels);
        g.transform.SetParent(bracketPanel.transform, true);
        bracketObjects.Add(g);
        return g;
    }

    public void KillBracketObjects() {
        for (int i = bracketObjects.Count-1; i >= 0; i--) {
            Destroy(bracketObjects[i]);
        }
        bracketObjects.Clear();
    }

    //Shows the vote options
    public void SetUpVote(string pitchA, string pitchB) {
        HidePanels();
        eliminationPanel.SetActive(true);
        itemA.text = pitchA;
        itemB.text = pitchB;
        avotes = 0;
        bvotes = 0;
        updateVoteText();

    }

    //Updates the vote counters
    private void updateVoteText() {
        votesA.text = "Votes: " + avotes;
        votesB.text = "Votes: " + bvotes;
    }

    //When a user adds their vote.
    public void VoteAdded(string sender, int choice) {
        if (choice == 1) {
            avotes++;
        } else if (choice == 2) {
            bvotes++;
        }
        updateVoteText();
    }

    //In the case where a user changes or invalidates their vote.
    public void VoteRemoved(string sender, int choice) {
        if (choice == 1) {
            avotes--;
        } else if (choice == 2) {
            bvotes--;
        }
        updateVoteText();
    }

    private void HidePanels() {
        bracketPanel.SetActive(false);
        eliminationPanel.SetActive(false);
        winningPanel.SetActive(false);
    }

    public void OnShowWinningIdea(string winner) {
        HidePanels();
        winningPanel.SetActive(true);
        winningIdea.text = winner;
    }
}
