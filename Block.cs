using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Block : MonoBehaviour
{
    public ParticleSystem _pS; //the particle system
    MeshRenderer _mR; //the mesh renderer

    //this method determmines if there is a block above
    public bool IsThereABlockAbove() {

        DisableCollider(); //disable the collider

        RaycastHit hit; //the raycast hit

        //raycast up 
        if (Physics.Raycast(transform.position, Vector3.up, out hit, Mathf.Infinity)) {
            if (hit.collider == null) {
                EnableCollider(); //enable the collider
                return false;
            }
            //if a block was hit
            else {
                if (hit.collider.CompareTag("block")) {
                    EnableCollider(); //enable the collider
                    return true;
                }
            }
        }
        EnableCollider(); //enable the collider
        return false;
    }

    private void EnableCollider() {
        GetComponent<BoxCollider>().enabled = true;
    }

    private void DisableCollider() {
        GetComponent<BoxCollider>().enabled = false;
    }

    public void StartDestroy() {
        _pS.Play(); //play particle burst
        transform.DetachChildren(); //detatch all children
        Destroy(gameObject); //destroy self
    }

    public void SetParticleColorToMeshColor() {
        _pS = transform.GetChild(0).GetComponent<ParticleSystem>(); //get the particle system reference
        _mR = GetComponent<MeshRenderer>(); //get the mesh renderer
        var main = _pS.main;
        main.startColor = _mR.material.color;
        //var _settings = _pS.main;
        //_settings.startColor = Color.red;//new ParticleSystem.MinMaxGradient(new Color(_mR.material.color.r, _mR.material.color.g, _mR.material.color.b, 255));
    }

    public void Flash() {
        var meshRenderer = GetComponent<MeshRenderer>();
        var currentColor = meshRenderer.material.color;
        var currentLocation = transform.localPosition.y;

        transform.DOLocalMoveY(currentLocation - (transform.localScale.y * 0.25f), 0.1f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                transform.DOLocalMoveY(currentLocation, 0.1f);
            });

        meshRenderer.material.DOColor(Color.white, 0.1f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                meshRenderer.material.DOColor(currentColor, .5f);
            });
    }

    //this method sets the block color to the saved hue
    public void AdoptSavedHue() {
        _mR = GetComponent<MeshRenderer>();
        _mR.material.SetColor("_Color", Color.HSVToRGB(((PlayerPrefs.GetInt("hue",0) * 1f) / 100f) % 1f, 0.5f, 1f)); //set current color depending on hue value
    }
}
