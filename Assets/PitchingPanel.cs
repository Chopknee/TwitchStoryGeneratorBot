using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PitchingPanel : MonoBehaviour {

    public GameObject ideaPitchPrefab;
    public GameObject fallingNamePrefab;

    public List<GameObject> ideasObjects;

    public void Start() {
        ideasObjects = new List<GameObject>();
    }

    public void AddPitch(string name, string content) {
        RectTransform pan = gameObject.GetComponent<RectTransform>();
        //A fun animation that will be entertaining to watch

        GameObject pitchText = Instantiate(ideaPitchPrefab);
        GameObject nameText = Instantiate(fallingNamePrefab);
        ideasObjects.Add(pitchText);
        pitchText.transform.SetParent(gameObject.transform);
        nameText.transform.SetParent(gameObject.transform);
        Vector3 pos = new Vector3(Random.value * pan.rect.width, Random.value * pan.rect.height, 0);
        nameText.transform.position = pos;
        pitchText.transform.position = pos;
        pitchText.GetComponent<Text>().text = content;
        nameText.GetComponent<Text>().text = name;
    }

    public void EndChatstorming() {

    }
}