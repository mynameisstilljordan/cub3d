using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Ingame : MonoBehaviour
{
    [SerializeField] TMP_Text _levelText; 
    [SerializeField] Button _restartButton, _pauseButton, _backButton, _menuButton;
    [SerializeField] Canvas _pauseCanvas;
    GameObject _player;
    BoardGeneration _bG; 

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player"); 
        _levelText.text = PlayerPrefs.GetInt("level", 1).ToString(); //set the text to the level number 
        RenderSettings.skybox.SetColor("_TopColor", Color.HSVToRGB(((PlayerPrefs.GetInt("hue")) / 100f) % 1f, 0.5f, 1f));
        RenderSettings.skybox.SetColor("_BottomColor", Color.HSVToRGB(((PlayerPrefs.GetInt("hue")) / 100f) % 1f, 0.5f, 0.2f));
        _restartButton.onClick.AddListener(OnRestartButtonPressed);
        _pauseButton.onClick.AddListener(OnPauseButtonPressed);
        _backButton.onClick.AddListener(OnBackButtonPressed);
        _menuButton.onClick.AddListener(OnMenuButtonPressed);
        _player = GameObject.FindGameObjectWithTag("Player"); //find the player gameobject
        _bG = GameObject.FindGameObjectWithTag("ingameHandler").GetComponent<BoardGeneration>(); //get the instance
    }

    //when the pause button is pressed
    void OnPauseButtonPressed() {
        _pauseCanvas.enabled = true;
    }

    //when the restart button is pressed
    void OnRestartButtonPressed() {
        _bG.RestartLevel();
    }

    void OnMenuButtonPressed() {
        PlayerPrefs.SetInt("levelTransition", 1);
        _pauseCanvas.enabled = false;
        _bG.GoBackToMenu();
    }

    void OnBackButtonPressed() {
        _pauseCanvas.enabled = false;
    }
}
