using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruction : MonoBehaviour {

    public float destroySeconds = 10;

	// Use this for initialization
	void Start () {
        Invoke("Kill", destroySeconds);
	}
	
	void Kill() {
        Destroy(gameObject);
    }
}
