using UnityEngine;

public class GameplayCommHubb : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private GameplayManager gameplay = null;
    [SerializeField] private MenuManager menu = null;
    [SerializeField] private AudioManager myAudio = null;

    #endregion

    #region START
    void Start()
    {
        Initializations();

        menu.StartOrContinueGame += gameplay.StartOrContinueGame;
        gameplay.GameStartedOrContinued += myAudio.PlayMusic;
        
        menu.PauseGame += gameplay.PauseGame;
        menu.PauseGame += myAudio.PauseMusic;
        
        gameplay.CurrentScore_HasChanged += menu.UpdateCurrentScore_OnScreen;
        gameplay.CurrentWave_HasChanged += menu.UpdateWave_OnScreen;

        gameplay.BossFired += myAudio.PlaySound_BossFiring;
        gameplay.BossFired_JammedGun += myAudio.PlaySound_BossFiring_JammedGun;

        gameplay.HeroWasHit += menu.HidePauseButton;
        gameplay.HeroWasHit += myAudio.PlaySound_HeroWasHit;
        gameplay.HeroWasHit += myAudio.StopMusic;

        gameplay.HeroAttacked += myAudio.PlaySound_HeroAttack;
        gameplay.BossIsDead += myAudio.PlaySound_BossWasHit;
        gameplay.BossIsGone += myAudio.PlaySound_BossIsDead;

        gameplay.BossEntered += myAudio.SwitchToBossMusic;
        gameplay.BossIsDead += myAudio.SwitchToNormalMusic;

        gameplay.HeroIsDead += menu.GameOver;

        menu.HighScoreWasNotBroken += myAudio.PlaySound_GameOver;
        menu.HighScoreWasBroken += myAudio.PlaySound_NewHighScore;

        menu.ButtonWasClicked += myAudio.PlaySound_ButtonClick;

        gameplay.HeroJumped += myAudio.PlaySound_HeroJump;
        gameplay.HeroStartedDodging += myAudio.PlaySound_Dodge;
        gameplay.HeroStoppedDodging += myAudio.PlaySound_StopDodge;
    }
    #endregion

    #region INITIALIZATIONS
    private void Initializations()
    {
        menu.ShowTitleScreen();
    }
    #endregion
}
