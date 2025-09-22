using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {
    public static SoundManager instance { get; private set; }
    public int maxSimultaneousSounds;
    public Sound playerAttack, enemyAttack, treasure, gameover, lvUp, pickUp;
    [SerializeField] private AudioSource audioSource;

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySound(Sound sound) {
        if (Menu.option.isOnSE)
            StartCoroutine(CRPlaySound(sound));
    }

    IEnumerator CRPlaySound(Sound sound) {
        if (sound.simultaneousPlayCount >= maxSimultaneousSounds)
            yield break;
        sound.simultaneousPlayCount++;
        audioSource.PlayOneShot(sound.clip, sound.volume);
        yield return new WaitForSeconds(sound.clip.length); // 再生が終了するまで待つ
        sound.simultaneousPlayCount--;
    }

    public void PauseBGM() {
        audioSource.Stop();
        audioSource.PlayDelayed(3.5f);
    }
}

[System.Serializable]
public class Sound {
    public AudioClip clip;
    public float volume;
    [HideInInspector] public int simultaneousPlayCount = 0;
}