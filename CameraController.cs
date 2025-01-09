using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    float _currentRotationY = 0;
    bool _isCameraRotating = false;

    //this method rotates the camera for the given degrees (clockwise)
    public void RotateCameraClockwise(float degrees) {
        _isCameraRotating = true;
        //_currentRotationY += degrees;
        PlayerPrefs.SetInt("cameraDirection", GetPreviousDirection(PlayerPrefs.GetInt("cameraDirection",0)));
        transform.DOLocalRotate(new Vector3(transform.localRotation.x, transform.localEulerAngles.y + degrees, transform.localRotation.z), 0.5f)
            .OnComplete(() => {
                _isCameraRotating = false;
            });
    }

    //this method rotates the camera for the given degrees (counter clockwise)
    public void RotateCameraCounterClockwise(float degrees) {
        _isCameraRotating = true;
        //_currentRotationY -= degrees;
        PlayerPrefs.SetInt("cameraDirection", GetNextDirection(PlayerPrefs.GetInt("cameraDirection", 0)));
        transform.DOLocalRotate(new Vector3(transform.localRotation.x, transform.localEulerAngles.y - degrees, transform.localRotation.z), 0.5f)
            .OnComplete(() => {
                _isCameraRotating = false;
            });
    }

    //this method sets the roation of the camera to the given parameter
    public void SetRotation(int degrees) {
        transform.DOLocalRotate(new Vector3(transform.localRotation.x, degrees, transform.localRotation.z), 0f);
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


    //let the player readopt the camera (called after bounce animation)
    public void SetCameraControllerParentToPlayer(Transform player) {
        transform.SetParent(player);
    }

    //this method removes the parent of the camera controller
    public void RemoveParent() {
        transform.SetParent(null);
    }

    public bool IsCameraRotating() {
        return _isCameraRotating;
    }
}
