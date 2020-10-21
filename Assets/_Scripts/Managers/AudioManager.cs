using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private AudioClip heroActionsSound = null;
    [SerializeField] private AudioClip ImpactBetween_ProjectileAndCharacter_Sound = null;

    [SerializeField] private AudioClip song1 = null;
    [SerializeField] private AudioClip song2 = null;

    [SerializeField] private AudioClip bossFiringSmallSound = null;
    [SerializeField] private AudioClip bossFiringBigSound = null;
    [SerializeField] private AudioClip bossFiringJammedGunSound = null;
    [SerializeField] private AudioClip bossDefeatSound = null;

    [SerializeField] private AudioClip heroAttackSound = null;
    [SerializeField] private AudioClip heroAttackSound2 = null;
    [SerializeField] private AudioClip gameOverSound = null;
    [SerializeField] private AudioClip newHighScoreSound = null;

    [SerializeField] private AudioClip buttonClickSound = null;

    [SerializeField] private AudioSource sfxSource_SFX1 = null;
    [SerializeField] private AudioSource sfxSource_SFX2 = null;
    [SerializeField] private AudioSource sfxSource_SFX3 = null;
    [SerializeField] private AudioSource sfxSource_Music1 = null;
    [SerializeField] private AudioSource sfxSource_Music2 = null;

    private float maxMusicVolume = 0.75f; // 0.75f
    private bool bossMusicIsActive;
    private bool fadeIsAllowed;

    #endregion

    // ---------- INITIALIZATIONS ----------------------------------------------------+

    #region AWAKE
    public void Awake()
    {
        sfxSource_SFX1.volume = 1.0f;
        sfxSource_SFX2.volume = 0.5f;
        sfxSource_SFX3.volume = 0.35f;
        sfxSource_Music1.volume = maxMusicVolume;
        sfxSource_Music2.volume = maxMusicVolume;

        sfxSource_Music1.clip = song1;
        sfxSource_Music1.loop = true;

        sfxSource_Music2.clip = song2;
        sfxSource_Music2.loop = true;

        bossMusicIsActive = false;
        fadeIsAllowed = false;
    }
    #endregion

    // ---------- MUSIC ----------------------------------------------------+

    #region PLAY MUSIC
    public void PlayMusic(bool _newGame)
    {
        if (_newGame)
        {
            fadeIsAllowed = false;

            // Play normal music
            sfxSource_Music1.volume = maxMusicVolume; // Do not remove line
            sfxSource_Music1.Stop(); 
            sfxSource_Music1.Play();

            // Stop boss music
            sfxSource_Music2.Stop();

            // Set boss music as not active
            bossMusicIsActive = false;
        }
        else
        {
            if (bossMusicIsActive)
            {
                // Unpause boss music 
                sfxSource_Music2.UnPause();
            }
            else
            {
                // Unpause normal music
                sfxSource_Music1.UnPause();
            }
        }
    }
    #endregion

    #region STOP MUSIC
    public void StopMusic()
    {
        sfxSource_Music1.volume = 0;
        sfxSource_Music1.Stop();

        sfxSource_Music2.volume = 0;
        sfxSource_Music2.Stop();

        //fadeIsAllowed = true;

        //StartCoroutine(FadeOut(sfxSource_Music1, 0.1f));
        //StartCoroutine(FadeOut(sfxSource_Music2, 0.1f));
    }
    #endregion

    #region PAUSE MUSIC
    // Note: its necessary to stop the song that is not playing at the moment,
    // because it can still be heard if it's fading out
    public void PauseMusic()
    {
        if (bossMusicIsActive)
        {
            sfxSource_Music2.Pause();
            sfxSource_Music1.Stop();
        }
        else
        {
            sfxSource_Music1.Pause();
            sfxSource_Music2.Stop();
        }
    }
    #endregion

    #region SWITCH TO NORMAL MUSIC
    public void SwitchToNormalMusic()
    {
        fadeIsAllowed = true;
        bossMusicIsActive = false;

        StartCoroutine(FadeOut(sfxSource_Music2, 1.0f));
        StartCoroutine(FadeIn(sfxSource_Music1, 8.0f));
    }
    #endregion

    #region SWITCH TO BOSS MUSIC
    public void SwitchToBossMusic()
    {
        fadeIsAllowed = true;
        bossMusicIsActive = true;

        StartCoroutine(FadeOut(sfxSource_Music1, 1.0f));
        sfxSource_Music2.volume = maxMusicVolume;
        sfxSource_Music2.Play();
    }
    #endregion

    // ---------- FADE MUSIC ----------------------------------------------------+

    #region FADE OUT COROUTINE
    private IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        while (fadeIsAllowed && (audioSource.volume > 0))
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }

        if (fadeIsAllowed)
        {
            audioSource.Stop();
        }
    }
    #endregion

    #region FADE IN COROUTINE
    private IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        audioSource.Play();
        audioSource.volume = 0f;
        while (fadeIsAllowed && (audioSource.volume < maxMusicVolume))
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }
    #endregion

    // ---------- SFX ----------------------------------------------------+

    #region PLAY SOUND: HERO JUMP
    public void PlaySound_HeroJump()
    {
        sfxSource_SFX1.PlayOneShot(heroActionsSound);
    }
    #endregion

    #region PLAY SOUND: DODGE
    public void PlaySound_Dodge()
    {
        sfxSource_SFX2.PlayOneShot(heroActionsSound);
    }
    #endregion

    #region PLAY SOUND: STOP DODGE
    public void PlaySound_StopDodge()
    {
        sfxSource_SFX3.PlayOneShot(heroActionsSound);
    }
    #endregion

    #region PLAY SOUND: HERO ATTACK
    public void PlaySound_HeroAttack()
    {
        sfxSource_SFX1.PlayOneShot(heroAttackSound);
        sfxSource_SFX1.PlayOneShot(heroAttackSound2);
    }
    #endregion

    #region PLAY SOUND: HERO WAS HIT
    public void PlaySound_HeroWasHit()
    {
        sfxSource_SFX1.PlayOneShot(ImpactBetween_ProjectileAndCharacter_Sound);
    }
    #endregion

    #region PLAY SOUND: GAME OVER
    public void PlaySound_GameOver(int _matchScore)
    {
        sfxSource_SFX1.PlayOneShot(gameOverSound);
    }
    #endregion

    #region PLAY SOUND: GAME OVER
    public void PlaySound_GameOver()
    {
        sfxSource_SFX2.PlayOneShot(gameOverSound);
    }
    #endregion

    #region PLAY SOUND: GAME OVER
    public void PlaySound_NewHighScore()
    {
        sfxSource_SFX2.PlayOneShot(newHighScoreSound);
    }
    #endregion

    #region PLAY SOUND: BOSS FIRING
    public void PlaySound_BossFiring(bool _oneProjectileIsBig)
    {
        if (_oneProjectileIsBig)
        {
            sfxSource_SFX2.PlayOneShot(bossFiringBigSound);
        }
        else
        {
            sfxSource_SFX2.PlayOneShot(bossFiringSmallSound);
        }
    }
    #endregion

    #region PLAY SOUND: BOSS FIRING JAMMED GUN
    public void PlaySound_BossFiring_JammedGun()
    {
        sfxSource_SFX2.PlayOneShot(bossFiringJammedGunSound);
        //sfxSource_SFX2.PlayOneShot(bossFiringSound);
    }
    #endregion

    #region PLAY SOUND: BOSS WAS HIT
    public void PlaySound_BossWasHit()
    {
        sfxSource_SFX1.PlayOneShot(ImpactBetween_ProjectileAndCharacter_Sound);
    }
    #endregion

    #region PLAY SOUND: BOSS IS DEAD
    public void PlaySound_BossIsDead()
    {
        //sfxSource_SFX1.PlayOneShot(heroGroundHitSound);
        sfxSource_SFX1.PlayOneShot(bossDefeatSound);
    }
    #endregion

    #region PLAY SOUND: BUTTON CLICK
    public void PlaySound_ButtonClick()
    {
        sfxSource_SFX2.PlayOneShot(buttonClickSound);
    }
    #endregion
}
