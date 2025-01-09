using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    BoardGeneration _bG;
    Block _menuBlock;
    CameraController _cC;
    Menu _m;

    private void Start() {
        try { _bG = GameObject.FindGameObjectWithTag("ingameHandler").GetComponent<BoardGeneration>(); } catch { } //get the reference of the script
        try { _menuBlock = GameObject.FindGameObjectWithTag("menuBlock").GetComponent<Block>(); } catch { }
        try { _cC = GameObject.FindGameObjectWithTag("cameraController").GetComponent<CameraController>(); } catch { }
        try { _m = GameObject.FindGameObjectWithTag("menuHandler").GetComponent<Menu>(); } catch { }
    }

    //this method returns the current block
    public GameObject GetCurrentBlock() {
        RaycastHit hit; //the raycast hit
        //raycast down
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity)) {
            //if nothing was hit
            if (hit.collider == null) {
                return null;
            }
            else {
                //if a block was hit
                if (hit.collider.CompareTag("block") || hit.collider.CompareTag("menuBlock")) {
                    return hit.collider.gameObject;
                }
            }
        }
        return null;
    }

    public void Bounce() {
        var currentPosition = transform.localPosition.y;
        var currentScale = transform.localScale.y;

        transform.DOScaleY(currentScale * 0.95f, 0.1f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                transform.DOScaleY(currentScale, 0.1f);
            });

        transform.DOLocalMoveY(currentPosition - (transform.localScale.y * 0.25f), 0.1f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                transform.DOLocalMoveY(currentPosition, 0.1f)
                    .OnComplete(() => {
                        ReAttatchCameraController();
                    });
                
            });
    }

    //this method makes the player gameobject adopt the camera controller once again after the animation is finished
    private void ReAttatchCameraController() {
        if (SceneManager.GetActiveScene().name == "ingame") _bG.HandlePostMovementActions(); //call the reattach method in the board generation script
        else _m.HandlePostBounceActions();
    }

    //this method sets the roation of the camera to the given parameter
    public void SetRotation(int degrees) {
        transform.DOLocalRotate(new Vector3(transform.localRotation.x, degrees, transform.localRotation.z), 0f);
    }

    public void JumpAndRotateInPlace(int degrees) {
        transform.DOLocalRotate(new Vector3(transform.localRotation.x, transform.localEulerAngles.y + degrees, transform.localRotation.z), 0.3f)
            .SetEase(Ease.Linear);
        transform.DOJump(transform.position, transform.localScale.x * 5f, 1, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                _cC.RemoveParent();
                SoundManager.PlaySound("bounce");
                Bounce();
            });
    }
}
