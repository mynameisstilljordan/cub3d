using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;

public class BoardGeneration : MonoBehaviour {
    [SerializeField] GameObject _objectPlacer; //the object placer
    [SerializeField] GameObject _block; //the block prefab
    [SerializeField] GameObject _gameBoard; //the gameboard
    [SerializeField] GameObject _player; //the player
    [SerializeField] GameObject _cameraController; //the camera controller
    CameraController _cC; //the camera controller script reference
    GameObject _playerInnerCube; //the inner cube of the player
    GameObject _currentBlock; //the current block
    float _globalScale; //the scale for the board
    int _level; //the level int
    int _units; //the length & width of the board
    int _moves; //the number of moves to be made
    int _barrierPlaceholder = -999; //the barrier placeholder
    int _currentHue; //the current hue of the tiles
    int _minimumSwipeDistance = Screen.height * 5 / 100; //the minimum distance the finger must travel for a touch to be considered a swipe
    Vector2 _endLocation; //the end location of the player
    Vector2 _touchPosition; //the touch position
    Vector2 _startTouchPosition, _endTouchPosition;
    Player _p;

    //the playerstate enum
    private enum PlayerState {
        Idle, Moving
    };

    private enum GameState {
        Generating, Ingame, Paused
    }

    private enum CameraDirections {
        CW, CCW
    }

    private PlayerState _pS; //the playerstate
    private GameState _gS; //the gamestate

    // Start is called before the first frame update
    void Start() {
        _level = PlayerPrefs.GetInt("level", 1); //get the level playerpref

        _p = _player.GetComponent<Player>(); //get the player
        _cC = _cameraController.GetComponent<CameraController>(); //get the camera controller script from the gameobject

        _currentHue = PlayerPrefs.GetInt("hue", 0); //the current saved hue

        _gS = GameState.Generating; //set the gamestate
        SetSeed(_level); //set the seed to level int

        _units = 5; //the length and width of the grid
        _moves = 3; //the nunmber of moves to make

        CreateLevel(_units, _moves); //create the level with the given units and moves

        SetCameraControllerDirection(); //set the camera controller direction

        //if transitioning from the previous level
        if (PlayerPrefs.GetInt("levelTransition") == 1) {
            _pS = PlayerState.Moving;
            Invoke(nameof(PlayFeedBacks), 0.05f); //play feedback after a short delay
        }
    }

    //this method sets the camera controller direction to the saved direction
    private void SetCameraControllerDirection() {
        //set the cammera direction to the saved one
        switch (PlayerPrefs.GetInt("cameraDirection")) {
            case 0:
                _p.SetRotation(0);
                break;
            case 1:
                _p.SetRotation(-90);
                break;
            case 2:
                _p.SetRotation(180);
                break;
            case 3:
                _p.SetRotation(90);
                break;
        }
    }

    private void SetCurrentBlock() {
        _currentBlock = _player.GetComponent<Player>().GetCurrentBlock(); //get the current block
    }

    private void CheckForCompletion() {
        //_moves--;
        if (NumberOfBlocks() == 1) {
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level", 1) + 1);
            SoundManager.PlaySound("win");
            PlayerPrefs.SetInt("levelTransition", 1); //set the level transition to true
            SceneManager.LoadScene("ingame");
        }
    }

    //this method returns the number of blocks in the scene
    private int NumberOfBlocks() {
        var blocks = GameObject.FindGameObjectsWithTag("block"); //save all blocks to array
        return blocks.Length; //return the length of the array
    }

    //this method destroys the given block
    private void DestroyBlock(GameObject block) {
        block.GetComponent<Block>().StartDestroy(); //start the destruction sequence
    }

    private void Update() {
        //if there is atleast one touch on the screen
        if ((Input.touchCount > 0) && _gS == GameState.Ingame) {

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

    //this method determines if the player input was a swipe or not
    private bool WasThePlayerInputASwipe(Vector2 startPosition, Vector2 endPosition) {
        if (Mathf.Abs(startPosition.x - endPosition.x) < _minimumSwipeDistance && //if the x swipe wasnt far enough
            Mathf.Abs(startPosition.y - endPosition.y) < _minimumSwipeDistance) //if the y swipe wasnt far enough
            return false; //return false, marking the input as a tap

        else return true; //otherwise, the input was a swipe, so return true
    }

    //this method handles what happens when the player taps
    private void HandlePlayerTap(Touch touch) {
        Ray raycast = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit raycastHit;
        if (Physics.Raycast(raycast, out raycastHit)) {
            //if a block is hit
            if (raycastHit.collider.CompareTag("block") && _pS == PlayerState.Idle) {
                var blockHit = raycastHit.collider.gameObject; //save the gameobject to a variable

                //distance conditions
                if ((Mathf.Abs(_player.transform.localPosition.x - blockHit.transform.localPosition.x) <= (_globalScale + (_globalScale * 0.01f)) && //added 1% for false negatives
                    Mathf.Abs(_player.transform.localPosition.y - blockHit.transform.localPosition.y) <= (((_globalScale * 10f) * 2f) + _globalScale * 0.01f) && //added 1% for false negatives
                    Mathf.Abs(_player.transform.localPosition.z - blockHit.transform.localPosition.z) <= (_globalScale + (_globalScale * 0.01f))) && //added 1% for false negatives 

                    //preventing moving downwards (on the same x and z)
                    !(Mathf.Abs(blockHit.transform.localPosition.x - _player.transform.localPosition.x) <= _globalScale / 100f && Mathf.Abs(blockHit.transform.localPosition.z - _player.transform.localPosition.z) <= _globalScale / 100f) && //added 1% for false negatives
                    !blockHit.GetComponent<Block>().IsThereABlockAbove() && //checking if there's a block above 

                    //Debug.Log(_player.transform.localPosition.x - blockHit.transform.localPosition.x + " ")

                    //preventing diagonal jumps
                    (!Mathf.Approximately(Mathf.Abs(_player.transform.localPosition.x - blockHit.transform.localPosition.x), Mathf.Abs(_player.transform.localPosition.z - blockHit.transform.localPosition.z)))) {

                    SetCurrentBlock(); //set the current block to the block below the player
                    DestroyBlock(_currentBlock); //disable the current block
                    _currentBlock = raycastHit.collider.transform.gameObject;
                    _pS = PlayerState.Moving; //set player state to moving  
                    _player.transform.DOJump(new Vector3(blockHit.transform.position.x, blockHit.transform.position.y + (_globalScale * 5f), blockHit.transform.position.z), _globalScale * 10f, 1, 0.3f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => {
                            CheckForCompletion(); //check for the level completion
                            if (_moves > 0) {
                                _cameraController.transform.SetParent(null);
                                SoundManager.PlaySound("bounce"); //play bounce sound
                                _p.Bounce(); //player bounce
                                _currentBlock.GetComponent<Block>().Flash(); //make the block flash white
                                if (PlayerPrefs.GetInt("vibration", 1) == 1) HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play vibration
                            }
                        });
                }
            }
        }
    }

    public void HandlePostMovementActions() {
        _cameraController.transform.SetParent(_player.transform); //reattatch the controller
        _pS = PlayerState.Idle; //set player state to idle
    }

    //this method handles what happens when the player swipes
    private void HandlePlayerSwipe(Vector2 startPosition, Vector2 endPosition) {
        float xDelta = Mathf.Abs(startPosition.x - endPosition.x); //the x delta of the swipe
        float yDelta = Mathf.Abs(startPosition.y - endPosition.y); //the y delta of the swipe

        //if the x distance was greater than the y distance
        if (xDelta > yDelta && _pS == PlayerState.Idle) {
            _pS = PlayerState.Moving;
            //if the swipe was left
            if (startPosition.x > endPosition.x) {
                PlayerPrefs.SetInt("cameraDirection", GetNextDirection(PlayerPrefs.GetInt("cameraDirection", 0)));
                PlayerJumpAndRotateInPlace(-90); //rotate the camera counter clockwise
            }
            //if the swipe was right
            else {
                PlayerPrefs.SetInt("cameraDirection", GetPreviousDirection(PlayerPrefs.GetInt("cameraDirection", 0)));
                PlayerJumpAndRotateInPlace(90); //rotate the camera clockwise
            }    
        }
    }

    //get the next direction int
    private int GetNextDirection(int direction) {
        if (direction < 3) direction++; //increment direction
        else direction = 0; //set direction to 0
        return direction; //return the direction
    }

    //get the previous direction int
    private int GetPreviousDirection(int direction) {
        if (direction > 0) direction--; //decrement direction
        else direction = 3; //set direction to 3
        return direction; //return the direction
    }

    private void PlayerJumpAndRotateInPlace(int degrees) {
        _player.transform.DOLocalRotate(new Vector3(_player.transform.localRotation.x, _player.transform.localEulerAngles.y + degrees, _player.transform.localRotation.z), 0.3f);
        _player.transform.DOJump(_player.transform.position, _player.transform.localScale.x * 5f, 1, 0.3f)
            .OnComplete(() => {
                _cC.RemoveParent();
                SoundManager.PlaySound("bounce");
                _p.Bounce();
                _currentBlock.GetComponent<Block>().Flash(); //make the block flash white
            });
    }

    private void PlayFeedBacks() {
        SoundManager.PlaySound("bounce"); //play bounce sound
        _p.Bounce(); //player bounce
        SetCurrentBlock(); //set the current block
        _currentBlock.GetComponent<Block>().Flash(); //make the block flash white
        if (PlayerPrefs.GetInt("vibration",1) == 1) HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play vibration
    }

    //set the seed 
    void SetSeed(int seed) {
        if (seed == 0) {
            Random.InitState((int)System.DateTime.Now.Ticks);
            Debug.Log((int)System.DateTime.Now.Ticks);
        }
        //set random seed
        else Random.InitState(seed); //set seed to parameter
    }

    //this method creates the level with the given units and moves 
    private void CreateLevel(int units, int moves) {
        SetGlobalScale(units); //set the global scale
        BuildLevel(GenerateSchematic(InitializeBoard(_units), _moves)); //build level
    }

    //this method initializes the board
    private int[,] InitializeBoard(int units) {
        int[,] schematic = new int[units + 2, units + 2]; //set up the board with the length and width of the unit size
        //for all the rows
        for (int i = 0; i < units + 2; i++) {
            //for all the columns
            for (int j = 0; j < units + 2; j++) {
                //if on the edge of the board, place a 
                if (i == 0 || i == units + 1 || j == 0 || j == units + 1) schematic[i, j] = _barrierPlaceholder;
                //else schematic[i, j] = 1; //otherwise make it 1
            }
        }
        //PrintArray(schematic); //print the schematic
        return schematic; //return the schematic
    }

    //this method sets the global scale depending on the size of the board
    private void SetGlobalScale(int units) {
        _globalScale = 1.0f / units; //set the global scale as a % of 1
    }

    //this method generates a schematic with the given boardSchematic
    private int[,] GenerateSchematic(int[,] schematic, int moves) {
        int units = schematic.GetLength(0) - 2; //set the number of units

        Vector2[] validDirections = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) }; //a list of valid directions 
        Vector2 startLocation = new Vector2(Random.Range(1, units + 1), Random.Range(1, units + 1)); //mark the start location
        Vector2 currentLocation = startLocation; //set the current location to the start location
        Vector2 choice;
        Vector2 newLocation = Vector2.zero; //set the new location to 

        schematic[(int)startLocation.x, (int)startLocation.y]++; //set the start location to 1 higher
        int y = moves;
        //for the number of moves
        //for (int i = 0; i < moves; i++) {
            while (GetMaxHeight(schematic) < moves) { 
                validDirections = RandomizeArray(validDirections); //mix the indexes of the array

                int lowestY = int.MaxValue; //the lowest y value

                //pick a direction
                for (int j = 0; j < validDirections.Length; j++) {
                    choice = validDirections[j]; //set the choice of direction to the 
                    Vector2 projectedLocation = currentLocation + choice; //set the projected location to the sum of the current direction and location
                    if (schematic[(int)projectedLocation.x, (int)projectedLocation.y] != _barrierPlaceholder && //if the projected location is not a barrier
                        Mathf.Abs(schematic[(int)currentLocation.x, (int)currentLocation.y] - schematic[(int)projectedLocation.x, (int)projectedLocation.y]) < 2 &&  //if the height difference is 0 or 1
                        schematic[(int)projectedLocation.x, (int)projectedLocation.y] < lowestY) {
                        lowestY = schematic[(int)projectedLocation.x, (int)projectedLocation.y]; //update lowest y
                        newLocation = projectedLocation; //set the new location to the projected one
                    }
                }

                currentLocation = newLocation;

                schematic[(int)currentLocation.x, (int)currentLocation.y]++; //increment the number at the current location

            //if (i == _moves - 1) _endLocation = currentLocation; //set the end location to the current location on the final iteration
            if (GetMaxHeight(schematic) == moves) _endLocation = currentLocation;
            }
        //}

        //PrintArray(schematic);
        return schematic; //return the schematic
    }

    private Vector2[] RandomizeArray(Vector2[] array) {
        for (int i = 0; i < array.Length; i++) {
            Vector2 temp = array[i];
            int randomizeArray = Random.Range(0, i);
            array[i] = array[randomizeArray];
            array[randomizeArray] = temp; 
        }
        Debug.Log(array);
        return array;
    }

    private int GetMaxHeight(int[,] schematic) {
        int maxY = -1;
        int units = schematic.GetLength(0) - 2;
        //traversing through the schematic to find the greatest y value and saving it to maxY
        for (int i = 1; i < units + 1; i++) { for (int j = 1; j < units + 1; j++) { if (schematic[i, j] > maxY) maxY = schematic[i, j]; } }
        return maxY;
    }

    //this method builds the level from the given schematic
    private void BuildLevel(int[,] schematic) {
        int units = schematic.GetLength(0) - 2; //the units
        float boundary = CalculateOuterBounds(units); //calculate the outer bounds of the board
        int maxY = -1; //the highest height
        Vector3 finalPlayerPosition = Vector3.zero;

        //traversing through the schematic to find the greatest y value and saving it to maxY
        for (int i = 1; i < units + 1; i++) { for (int j = 1; j < units + 1; j++) { if (schematic[i, j] > maxY) maxY = schematic[i, j]; }}

        _objectPlacer.transform.localPosition = new Vector3(-boundary + -_globalScale, 0, -boundary); //set the position of the object placer
        _objectPlacer.transform.localScale = new Vector3(_globalScale, _globalScale * 10f, _globalScale); //reset x boundary position

        //for all the rows
        for (int i = 1; i < units + 1; i++) {
            _objectPlacer.transform.localPosition = new Vector3(_objectPlacer.transform.localPosition.x + _globalScale, 0, _objectPlacer.transform.localPosition.z); //shift the x
            //for all the columns
            for (int j = 1; j < units + 1; j++) {
                //for the number of blocks to be placed on the current spot
                for (int k = 0; k < schematic[i, j]; k++) {
                    _objectPlacer.transform.localPosition = new Vector3(_objectPlacer.transform.localPosition.x, (_globalScale * 10f) * k, _objectPlacer.transform.localPosition.z); //move the objectplacer up
                    var blockInstance = Instantiate(_block, Vector3.zero, Quaternion.identity); //create a block 
                    blockInstance.transform.SetParent(_gameBoard.transform); //set the parent 
                    blockInstance.transform.localRotation = Quaternion.identity; //reset the rotation
                    blockInstance.transform.localScale = new Vector3(_globalScale, _globalScale * 10f, _globalScale); //set the scale of the block
                    blockInstance.transform.localPosition = _objectPlacer.transform.localPosition; //set the position to the object placer position
                    blockInstance.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((((_currentHue-k) * 5f) / 100f) % 1f, 0.5f, 1f)); //set current color depending on hue value
                    blockInstance.GetComponent<Block>().SetParticleColorToMeshColor(); //set the particle color to current color
                }
                if (i == _endLocation.x && j == _endLocation.y) finalPlayerPosition = new Vector3(_objectPlacer.transform.localPosition.x, (_globalScale * 10f) * maxY, _objectPlacer.transform.localPosition.z); //the final player position

                _objectPlacer.transform.localPosition = new Vector3(_objectPlacer.transform.localPosition.x, 0, _objectPlacer.transform.localPosition.z + _globalScale); //shift the z
            }
            _objectPlacer.transform.localPosition = new Vector3(_objectPlacer.transform.localPosition.x, 0, -boundary); //reset z boundary position
        }
        _playerInnerCube = _player.transform.GetChild(0).gameObject; //save the first child as player inner cube
        _player.transform.DetachChildren(); //detatch all children from the player
        _player.transform.localScale = new Vector3(_globalScale, _globalScale * 10f, _globalScale); //the scale of the player
        _playerInnerCube.transform.SetParent(_player.transform); //let player adopt
        _playerInnerCube.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        _playerInnerCube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        _cameraController.transform.SetParent(_player.transform); //let player adopt
        _player.transform.localPosition = finalPlayerPosition; //set the player position
        _gS = GameState.Ingame; //update the gamestate
        if (_currentHue > 100) _currentHue %= 100; //cap the hue at 100
        PlayerPrefs.SetInt("hue", _currentHue += maxY) ; //update the hue playerpref
        _cameraController.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private float CalculateOuterBounds(int units) {
        float boundary = 0f; //the bondary (positive)
        int numberOfUnitsOnEitherSideOfZero = units / 2; //the number of values on either side of the zero

        //for the number of units on either side of the zero
        for (int i = 0; i < numberOfUnitsOnEitherSideOfZero; i++) {
            boundary += _globalScale; //add the globalscale to the running total
        }

        return boundary; //return boundary
    }

    //this method prints the given 2d array
    private void PrintArray(int[,] input) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < input.GetLength(0); i++) {
            for (int j = 0; j < input.GetLength(1); j++) {
                sb.Append(input[i, j]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    public void RestartLevel() {
        //_player.transform.DOLocalRotate(new Vector3(_player.transform.rotation.x, _player.transform.localRotation.y + 360, _player.transform.rotation.z), 1f)
        //    .SetEase(Ease.Linear);
        if (_pS == PlayerState.Idle) {
            _player.transform.DOJump(_player.transform.position, _globalScale * 10f, 1, 0.3f)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    SceneManager.LoadScene("ingame");
                });
        }
    }

    public void GoBackToMenu() {
        _pS = PlayerState.Moving; //set player state to moving  
        _player.transform.DOLocalRotate(new Vector3(_player.transform.rotation.x, 0, _player.transform.rotation.z), 0.3f)
            .SetEase(Ease.Linear);
        _player.transform.DOJump(_player.transform.position, _globalScale * 5f, 1, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                _cameraController.transform.SetParent(null);
                SoundManager.PlaySound("bounce"); //play bounce sound
                if (PlayerPrefs.GetInt("vibration", 1) == 1) HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play vibration
                SceneManager.LoadScene("menu");
            });
    }
}
