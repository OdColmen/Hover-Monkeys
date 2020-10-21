using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    #region VARIABLES

    // ----- EVENTS -----

    // High score was broken
    public delegate void ButtonWasClicked_EventHandler();
    public event ButtonWasClicked_EventHandler ButtonWasClicked;

    // Start or continue game
    public delegate void StartOrContinueGame_EventHandler();
    public event StartOrContinueGame_EventHandler StartOrContinueGame;

    // Pause game
    public delegate void PauseGame_EventHandler();
    public event PauseGame_EventHandler PauseGame;

    // High score was not broken
    public delegate void HighScoreWasNotBroken_EventHandler();
    public event HighScoreWasNotBroken_EventHandler HighScoreWasNotBroken;

    // High score was broken
    public delegate void HighScoreWasBroken_EventHandler();
    public event HighScoreWasBroken_EventHandler HighScoreWasBroken;

    // ----- UI -----

    [SerializeField] private GameObject titlePanel = null;
    [SerializeField] private GameObject gameplayPanel = null;
    [SerializeField] private GameObject homePanel = null;
    [SerializeField] private GameObject tutorialPanel = null;
    
    // UI inside gameplay panel
    [SerializeField] private Button pauseButton = null;
    [SerializeField] private GameObject highScoreSubPanel = null;

    // UI inside home panel
    [SerializeField] private Text gamePausedText = null;
    [SerializeField] private Text gameOverText = null;
    [SerializeField] private GameObject gameplayInstructionsSubPanel = null;
    [SerializeField] private GameObject homeMainButtons = null;
    [SerializeField] private GameObject homeExtraButtons = null;
    [SerializeField] private GameObject menuInstructionsScreen = null;
    [SerializeField] private GameObject creditsScreen = null;

    [SerializeField] private Button expandMenuButton = null;
    [SerializeField] private Button collapseMenuButton = null;

    // Block jump button panel (invisible button for blocking the jump button)
    [SerializeField] private GameObject blockJumpButtonPanel = null;

    // ----- HIGH SCORE -----

    [SerializeField] private HighScoreManager highScoreManager = null;
    [SerializeField] private Image highScoreIcon = null;
    [SerializeField] private Image newRecordIcon = null;
    [SerializeField] private Material newRecordMaterial = null;
    [SerializeField] private Text highScoreText = null;
    [SerializeField] private Text currentScoreText = null;
    [SerializeField] private Text currentWaveText = null;

    // ----- GAMEPLAY FLAGS -----

    private bool gamePaused;
    private bool gameOver;

    #endregion

    // ---------- INITIALIZATIONS ----------------------------------------------------+

    #region AWAKE
    private void Awake()
    {
        // Hide every panel
        titlePanel.SetActive(false);
        homePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        highScoreSubPanel.SetActive(false);
        gameplayInstructionsSubPanel.SetActive(false);

        // Hide home/menu buttons
        homeMainButtons.SetActive(false);
        homeExtraButtons.SetActive(false);
        expandMenuButton.gameObject.SetActive(false);
        collapseMenuButton.gameObject.SetActive(false);

        // Hide instructions and credits images
        menuInstructionsScreen.SetActive(false);
        creditsScreen.SetActive(false);

        // Activate correct high score icon
        highScoreIcon.gameObject.SetActive(true);
        newRecordIcon.gameObject.SetActive(false);

        // Hide block jump button panel
        blockJumpButtonPanel.SetActive(false);

        // Initialize high score manager
        highScoreManager.InitializeHighScore();

        // Show gameplay labels
        UpdateCurrentScore_OnScreen(0);
        UpdateHighScore_OnScreen(highScoreManager.GetHighScore(), true);
        UpdateWave_OnScreen(0);
    }
    #endregion

    // ---------- KEYBOARD CONTROLS ----------------------------------------------------+

    #region UPDATE
    public void Update()
    {
        DetectKeyboardInput();
    }
    #endregion

    #region DETECT KEYBOARD INPUT
    public void DetectKeyboardInput()
    {
        // Go to home screen from title screen
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (titlePanel.activeSelf)
            {
                ExecuteHomeButton_FromTitleScreen();
            }
        }

        // Pause/unpause game when typing P
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (homePanel.activeSelf)
            {
                ExecutePlayButton();
            }
            
            else if (pauseButton.gameObject.activeSelf)
            {
                ExecutePauseButton();
            }
        }
    }
    #endregion

    // ---------- SWITCH BETWEEN SCREENS ----------------------------------------------------+

    #region SHOW TITLE SCREEN
    public void ShowTitleScreen()
    {
        // Show title panel
        titlePanel.SetActive(true);

        // Set game as not running
        gamePaused = false;
        gameOver = false;
    }
    #endregion

    #region SHOW HOME SCREEN
    private void ShowHomeScreen()
    {
        // Show home panel
        homePanel.SetActive(true);

        // Show gameplay panel
        gameplayPanel.SetActive(true);
        highScoreSubPanel.SetActive(true);

        // Hide pause button
        pauseButton.gameObject.SetActive(false);

        // Show main home buttons
        homeMainButtons.SetActive(true);
        // Show correct menu button
        expandMenuButton.gameObject.SetActive(true);
        collapseMenuButton.gameObject.SetActive(false);
    }
    #endregion

    #region SHOW HOME TEXT
    private void ShowHomeText(bool _firstMatchSinceStart, bool _newHighScore)
    {
        // If this is the first match since the user opened the game
        if (_firstMatchSinceStart)
        {
            // Show gameplay instructions
            gamePausedText.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            gameplayInstructionsSubPanel.SetActive(true);
        }

        // If game paused
        else if (gamePaused)
        {
            // Show game paused text
            gamePausedText.gameObject.SetActive(true);
            gameOverText.gameObject.SetActive(false);
            gameplayInstructionsSubPanel.SetActive(false);
        }

        // If game over
        else if (gameOver)
        {
            // Show game over text
            gamePausedText.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(true);
            gameplayInstructionsSubPanel.SetActive(false);

            if (_newHighScore)
            {
                gameOverText.text = "New Record!!";
            }
            else
            {
                gameOverText.text = "Game Over";
            }
        }

    }
    #endregion

    #region SHOW HOME EXTRA BUTTONS
    private void ShowHomeExtraButtons()
    {
        // Show extra home buttons
        homeExtraButtons.SetActive(true);
        // Show correct menu button
        expandMenuButton.gameObject.SetActive(false);
        collapseMenuButton.gameObject.SetActive(true);

        // Hide block jump button panel
        blockJumpButtonPanel.SetActive(false);
    }
    #endregion

    #region HIDE HOME EXTRA BUTTONS
    private void HideHomeExtraButtons()
    {
        // Hide extra home buttons
        homeExtraButtons.SetActive(false);
        // Show correct menu button
        expandMenuButton.gameObject.SetActive(true);
        collapseMenuButton.gameObject.SetActive(false);

        // Show block jump button panel
        blockJumpButtonPanel.SetActive(true);
    }
    #endregion

    #region SHOW MENU SCREEN
    private void ShowMenuScreen(bool _showMenuInstructions)
    {
        if (_showMenuInstructions)
        {
            menuInstructionsScreen.SetActive(true);
            creditsScreen.SetActive(false);
        }

        else
        {
            menuInstructionsScreen.SetActive(false);
            creditsScreen.SetActive(true);
        }
    }
    #endregion

    #region HIDE MENU SCREEN
    private void HideMenuScreen()
    {
        menuInstructionsScreen.SetActive(false);
        creditsScreen.SetActive(false);
    }
    #endregion

    #region EXECUTE HOME BUTTON - FROM TITLE SCREEN
    public void ExecuteHomeButton_FromTitleScreen()
    {
        // Hide title panel
        titlePanel.SetActive(false);

        // If player IS NOT noob: 
        if (highScoreManager.GetHighScore() > 0)
        {
            // Show home screen
            ShowHomeScreen();
            // Show home text
            ShowHomeText(true, false);
        }

        // If player IS noob
        else
        {
            // Show tutorial panel
            tutorialPanel.SetActive(true);
        }

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region EXECUTE EXPAND MENU BUTTON
    public void ExecuteExpandMenuButton()
    {
        // Show home extra buttons
        ShowHomeExtraButtons();

        // Show menu screen (instructions screen)
        ShowMenuScreen(true);

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region EXECUTE COLLAPSE MENU BUTTON
    public void ExecuteCollapseMenuButton()
    {
        // Hide home extra buttons
        HideHomeExtraButtons();

        // Hide menu screen
        HideMenuScreen();

        // Deny game over, so it wont show again
        gameOver = false;

        // Show home text
        ShowHomeText(false, false);

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region EXECUTE CREDITS BUTTON
    public void ExecuteMenuInstructionsButton()
    {
        // Show menu screen (instructions screen)
        ShowMenuScreen(true);

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region EXECUTE CREDITS BUTTON
    public void ExecuteCreditsButton()
    {
        // Show menu screen (credits screen)
        ShowMenuScreen(false);

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region EXECUTE PLAY BUTTON - FROM TUTORIAL SCREEN
    public void ExecutePlayButton_FromTutorialScreen()
    {
        // Hide tutorial panel
        tutorialPanel.SetActive(false);

        // Show
        gameplayPanel.SetActive(true);
        highScoreSubPanel.gameObject.SetActive(true);

        // Execute play button
        ExecutePlayButton();

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region EXECUTE TUTORIAL BUTTON
    public void ExecuteTutorialButton()
    {
        /*
        // Hide home panel
        homePanel.SetActive(false);

        // Hide gameplay panel
        gameplayPanel.SetActive(false);
        highScoreSubPanel.SetActive(false);

        // Show tutorial panel
        tutorialPanel.SetActive(true);
        */
    }
    #endregion

    #region EXECUTE PLAY BUTTON
    public void ExecutePlayButton()
    {
        // Hide home panel
        homePanel.SetActive(false);

        // Show pause button
        pauseButton.gameObject.SetActive(true);

        // Restore high score original graphics
        RestoreHighScore_OriginalGraphics();

        // Fire Event: StartOrContinueGame 
        StartOrContinueGame?.Invoke();

        // Set game as running
        gamePaused = false;
        gameOver = false;
    }
    #endregion

    #region EXECUTE PAUSE BUTTON
    public void ExecutePauseButton()
    {
        // Set game as paused
        gamePaused = true;

        // Hide menu screen
        HideMenuScreen();
        // Hide home extra buttons
        HideHomeExtraButtons();

        // Show home screen
        ShowHomeScreen();
        // Show home text
        ShowHomeText(false, false);

        // Fire Event: PauseGame
        PauseGame?.Invoke();

        // Fire event: Button was clicked
        ButtonWasClicked?.Invoke();
    }
    #endregion

    #region HIDE PAUSE BUTTON
    public void HidePauseButton()
    {
        // Hide pause button
        pauseButton.gameObject.SetActive(false);
    }
    #endregion

    #region GAME OVER
    public void GameOver(int _matchScore)
    {
        // Set game as over
        gameOver = true;

        // Local variable
        bool newHighScore = false;

        // If new high score: update high score on screen
        if (highScoreManager.SaveNewHighScore(_matchScore))
        {
            // Update high score on screen
            UpdateHighScore_OnScreen(_matchScore, false);

            // Set new high score as true
            newHighScore = true;

            // Fire event: High score was broken
            HighScoreWasBroken?.Invoke();
        }

        else
        {
            // Fire event: High score was not broken
            HighScoreWasNotBroken?.Invoke();
        }

        // Hide menu screen
        HideMenuScreen();
        // Hide home extra buttons
        HideHomeExtraButtons();

        // Show home screen
        ShowHomeScreen();
        // Show home text
        ShowHomeText(false, newHighScore);
    }
    #endregion

    #region EXECUTE RATE BUTTON
    public void ExecuteRateButton()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.animacvi.hovermonkeys");
    }
    #endregion

    // ---------- SCREEN TEXT UPDATES ----------------------------------------------------+

    #region RESTORE HIGH SCORE ORIGINAL GRAPHICS
    private void RestoreHighScore_OriginalGraphics()
    {
        // Change high score color
        highScoreText.material = null;

        // Show correct high score icon
        highScoreIcon.gameObject.SetActive(true);
        newRecordIcon.gameObject.SetActive(false);
    }
    #endregion

    #region UPDATE HIGH SCORE ON SCREEN
    private void UpdateHighScore_OnScreen(int _highScore, bool _firstMatchSinceStart)
    {
        // Set high score text
        highScoreText.text = _highScore + "";

        // If this IS NOT the first match since the user opened the game
        if (!_firstMatchSinceStart)
        {
            // Change high score color
            highScoreText.material = newRecordMaterial;

            // Show correct high score icon
            highScoreIcon.gameObject.SetActive(false);
            newRecordIcon.gameObject.SetActive(true);
        }
    }
    #endregion

    #region UPDATE CURRENT SCORE ON SCREEN
    public void UpdateCurrentScore_OnScreen(int _currentScore)
    {
        currentScoreText.text = _currentScore + "";
    }
    #endregion

    #region UPDATE WAVE ON SCREEN
    public void UpdateWave_OnScreen(int _currentWave)
    {
        currentWaveText.text = "Wave: " + _currentWave;
    }
    #endregion

    // ---------- CREDITS BUTTONS ----------------------------------------------------+

    #region EXECUTE DEVELOPER BUTTON
    public void ExecuteDeveloperButton()
    {
        Application.OpenURL("https://www.instagram.com/odacod");
    }
    #endregion

    #region EXECUTE MUSIC BUTTON
    public void ExecuteMusicButton()
    {
        Application.OpenURL("https://www.playonloop.com");
    }
    #endregion

    #region EXECUTE SFX BUTTON
    public void ExecuteSfxButton()
    {
        Application.OpenURL("https://www.bfxr.net");
    }
    #endregion
}
