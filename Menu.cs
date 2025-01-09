using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using MoreMountains.Feedbacks;
public class Menu : MonoBehaviour
{
    [SerializeField] TMP_Text _menuTitle;
    CameraController _cC;
    GameObject _player;
    Player _p;
    Block _b;
    Vector2 _startTouchPosition, _endTouchPosition;
    int _minimumSwipeDistance = Screen.height * 5 / 100; //the minimum distance the finger must travel for a touch to be considered a swipe
    int _savedCameraDirection;
    enum MenuOption {
        Play, Theme, Settings, Credits
    }
    MenuOption _mO; 

    enum PlayerState {
        Idle, Jumping
    }
    PlayerState _pS;


    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(PlayBounceEffects), 0.05f); //play feedback after a short delay
        RenderSettings.skybox.SetColor("_TopColor", Color.HSVToRGB(((PlayerPrefs.GetInt("hue")) / 100f) % 1f, 0.5f, 0.8f));
        RenderSettings.skybox.SetColor("_BottomColor", Color.HSVToRGB(((PlayerPrefs.GetInt("hue")) / 100f) % 1f, 0.5f, 0.2f));
        _savedCameraDirection = PlayerPrefs.GetInt("cameraDirection", 0);
        _player = GameObject.FindGameObjectWithTag("Player");
        _cC = GameObject.FindGameObjectWithTag("cameraController").GetComponent<CameraController>();
        _p = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _b = GameObject.FindGameObjectWithTag("menuBlock").GetComponent<Block>();

        _b.AdoptSavedHue();
    }

    void PlayBounceEffects() {
        if (PlayerPrefs.GetInt("levelTransition", 0) == 1) {
            _p.Bounce();
            _p.GetCurrentBlock().GetComponent<Block>().Flash();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if there is atleast one touch on the screen
        if ((Input.touchCount > 0)) {

            Touch touch = Input.GetTouch(0); //save the touch 

            //if the touch is in the began phase
            if (touch.phase == TouchPhase.Began) {
                _startTouchPosition = touch.position; //save the position of where the touch started 
            }

            if (touch.phase == TouchPhase.Ended) {
                _endTouchPosition = touch.position; //save the end touch position

                //if the input was a swipe
                if (WasThePlayerInputASwipe(_startTouchPosition, _endTouchPosition)) {
                    HandlePlayerSwipe(_startTouchPosition, _endTouchPosition); //handle the player swipe
                }
                //if the input was a tap
                else {
                    HandlePlayerTap(touch); //handle the player tap
                }
            }
        }
    }

    public void NextOption() {
        if (_mO > (MenuOption)0) {
            _mO--;
        }
        else _mO = (MenuOption)3;

        PlayerJumpAndRotateInPlace(90);
        UpdateMenu(); //update the menu option
    }

    public void PreviousOption() {
        if (_mO < (MenuOption)3) {
            _mO++;
        }
        else _mO = (MenuOption)0;

        PlayerJumpAndRotateInPlace(-90);
        UpdateMenu(); //update the menu option
    }

    //update the menu to reflect the current option
    private void UpdateMenu() {
        switch (_mO) {
            case MenuOption.Play:
                _menuTitle.text = "PLAY";
                break;
            case MenuOption.Settings:
                _menuTitle.text = "CONFIG";
                break;
            case MenuOption.Theme:
                _menuTitle.text = "THEME";
                break;
            case MenuOption.Credits:
                _menuTitle.text = "EXTRA";
                break;
        }
    }

    private void OnPlayButtonPressed() {
        PlayerPrefs.SetInt("levelTransition", 1);
        _pS = PlayerState.Jumping;
        _player.transform.DOLocalRotate(new Vector3(_player.transform.rotation.x, (float)ConvertSavedDirectionToDegrees(), _player.transform.rotation.z), 0.3f)
            .SetEase(Ease.Linear);
        _player.transform.DOJump(_player.transform.position, _player.transform.localScale.x * 5f, 1, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                SceneManager.LoadScene("ingame");
            });
    }

    private int ConvertSavedDirectionToDegrees() {
        //set the cammera direction to the saved one
        switch (_savedCameraDirection) {
            case 0:
                return 0;
            case 1:
                return -90;
            case 2:
                return 180;
            case 3:
                return 90;
        }
        return 0;
    }

    private void OnSettingsButtonPressed() {

    }

    private void OnThemesButtonPressed() {

    }

    //this method handles what happens when the player taps
    private void HandlePlayerTap(Touch touch) {
        Ray raycast = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit raycastHit;
        if (Physics.Raycast(raycast, out raycastHit)) {
            //if a block is hit
            if (raycastHit.collider.CompareTag("menuBlock")) {
                switch (_mO) {
                    case MenuOption.Play:
                        OnPlayButtonPressed();
                        break;
                    case MenuOption.Settings:
                        OnSettingsButtonPressed();
                        break;
                    case MenuOption.Theme:
                        OnThemesButtonPressed();
                        break;
                }
            }
        }
    }

    //this method handles what happens when the player swipes
    private void HandlePlayerSwipe(Vector2 startPosition, Vector2 endPosition) {
        float xDelta = Mathf.Abs(startPosition.x - endPosition.x); //the x delta of the swipe
        float yDelta = Mathf.Abs(startPosition.y - endPosition.y); //the y delta of the swipe

        //if the x distance was greater than the y distance
        if (xDelta > yDelta && _pS == PlayerState.Idle) {
            _pS = PlayerState.Jumping;
            //if the swipe was left
            if (startPosition.x > endPosition.x) {
                PreviousOption(); //rotate the camera counter clockwise
            }
            //if the swipe was right
            else {
                NextOption(); //rotate the camera clockwise
            }
        }
    }

    //this method determines if the player input was a swipe or not
    private bool WasThePlayerInputASwipe(Vector2 startPosition, Vector2 endPosition) {
        if (Mathf.Abs(startPosition.x - endPosition.x) < _minimumSwipeDistance && //if the x swipe wasnt far enough
            Mathf.Abs(startPosition.y - endPosition.y) < _minimumSwipeDistance) //if the y swipe wasnt far enough
            return false; //return false, marking the input as a tap

        else return true; //otherwise, the input was a swipe, so return true
    }

    public void HandlePostBounceActions() {
        _cC.SetCameraControllerParentToPlayer(_p.transform);
        _pS = PlayerState.Idle;
    }

    private void PlayerJumpAndRotateInPlace(int degrees) {
        _player.transform.DOLocalRotate(new Vector3(_player.transform.localRotation.x, _player.transform.localEulerAngles.y + degrees, _player.transform.localRotation.z), 0.3f);
        _player.transform.DOJump(_player.transform.position, _player.transform.localScale.x * 5f, 1, 0.3f)
            .OnComplete(() => {
                _cC.RemoveParent();
                SoundManager.PlaySound("bounce");
                _p.Bounce();
                _p.GetCurrentBlock().GetComponent<Block>().Flash(); //make the block flash white
            });
    }
}
