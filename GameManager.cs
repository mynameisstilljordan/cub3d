using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance; //the instance

    private void Awake() {
        if (Instance == null) Instance = this; //if instance is null, make this the instance
        else Destroy(gameObject); //otherwise, destroy self
        DontDestroyOnLoad(this); //dont destroy this gameobject on load
    }

    // Start is called before the first frame update
    void Start() {
        //set user consent etc...   
        PlayerPrefs.SetInt("levelTransition", 0);
    }

}