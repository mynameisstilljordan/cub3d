using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static AudioClip _bounce, _win;
    static AudioSource audioSrc;

    // Start is called before the first frame update
    void Start() {
        _bounce = Resources.Load<AudioClip>("bounce");
        _win = Resources.Load<AudioClip>("win");
        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip) {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            switch (clip) {
                case "bounce":
                    audioSrc.PlayOneShot(_bounce);
                    break;

                case "win":
                    audioSrc.PlayOneShot(_win);
                    break;
            }
        }
    }
}