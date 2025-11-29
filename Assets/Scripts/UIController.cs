using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const float LoadingDuration = 5f;
    public static UIController UIControllerInstance { get; private set; }

    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject loadingControlsPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject controlsPanel;
    public GameObject areYouSurePanel;
    
    [Header("Loading Controls References")]
    public Slider loadingSlider;
    public TMPro.TextMeshProUGUI pressAnyButtonText;
    
    private GameObject[] _allPanels;
    private GameObject _lastPanel;
    private System.Action _onConfirmAction;
    
    private bool _waitingForAnyButton;
    private System.Action _onAnyButtonPressed;
    
    // TODO temp variable
    private bool _hasSave;

    private void Awake()
    {
        if (UIControllerInstance != null && UIControllerInstance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        UIControllerInstance = this;
        DontDestroyOnLoad(gameObject);

        _allPanels = new[] 
        { 
            titlePanel, mainMenuPanel, creditsPanel, loadingControlsPanel, 
            hudPanel, pausePanel, controlsPanel, areYouSurePanel 
        };
    }

    public void SetActivePanel(GameObject activePanel)
    {
        _lastPanel = GetCurrentActivePanel();
        
        foreach (var panel in _allPanels)
        {
            panel.SetActive(panel == activePanel);
        }
    }
    
    private GameObject GetCurrentActivePanel()
    {
        foreach (var panel in _allPanels)
        {
            if (panel.activeSelf)
            {
                return panel;
            }
        }
        return null;
    }
    
    public void WaitForAnyButton()
    {
        _waitingForAnyButton = true;
    }

    void Update()
    {
        if (_waitingForAnyButton && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            _waitingForAnyButton = false;
            
            if (GetCurrentActivePanel() == titlePanel)
            {
                SetActivePanel(mainMenuPanel);
            }
            else
            {
                ShowGame();
            }
        }
        
        if (!_waitingForAnyButton && Input.GetKeyDown(KeyCode.Escape))
        {
            if (hudPanel.activeSelf)
            {
                ShowPause();
            }
            else if (pausePanel.activeSelf)
            {
                ShowHUD();
            }
        }
    }

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }
    
    public void ShowNewGame()
    {
        if (_hasSave)
        {
            AskNewGame();
        }
        else
        {
            CreateNewGame();
        }
    }

    private void CreateNewGame()
    {
        // TODO Create new save
        _hasSave = true;
        ShowLoadingControls();
    }
    
    public void ShowLoadGame()
    {
        if (_hasSave)
        {
            ShowLoadingControls();
        }
    }

    private void ShowLoadingControls()
    {
        var mainMenuAnimator = mainMenuPanel.GetComponent<PanelAnimator>();
        if (mainMenuAnimator)
        {
            mainMenuAnimator.HideToLeft();
        }
        else
        {
            mainMenuPanel.SetActive(false);
        }
        
        loadingControlsPanel.SetActive(true);
        var loadingAnimator = loadingControlsPanel.GetComponent<PanelAnimator>();
        if (loadingAnimator)
        {
            loadingAnimator.ShowFromRight();
        }
        
        foreach (var panel in _allPanels)
        {
            if (panel != loadingControlsPanel && panel != mainMenuPanel)
            {
                panel.SetActive(false);
            }
        }
        
        StartCoroutine(LoadingSequence());
    }
    
    private IEnumerator LoadingSequence()
    {
        if (pressAnyButtonText)
        {
            pressAnyButtonText.gameObject.SetActive(false);
        }
        
        if (loadingSlider)
        {
            loadingSlider.value = 0f;
        }

        var elapsed = 0f;
        
        while (elapsed < LoadingDuration)
        {
            elapsed += Time.deltaTime;
            if (loadingSlider)
            {
                loadingSlider.value = elapsed / LoadingDuration;
            }
            yield return null;
        }
        
        if (loadingSlider)
        {
            loadingSlider.value = 1f;
        }
        
        if (pressAnyButtonText)
        {
            pressAnyButtonText.gameObject.SetActive(true);
        }
        
        WaitForAnyButton();
    }
    
    private void ShowGame() 
    {
        // TODO show game from current save
        ScenesController.Instance.ChangeScene("GameScene");
        SetActivePanel(hudPanel);
    }
    
    public void ShowCredits()
    {
        SetActivePanel(creditsPanel);
    }

    private void ShowPause()
    {
        // TODO stop in-game time
        SetActivePanel(pausePanel);
    }

    public void ShowHUD()
    {
        // TODO start in-game time
        SetActivePanel(hudPanel);
    }
    
    public void ShowControls()
    {
        SetActivePanel(controlsPanel);
    }

    public void SaveAndShowMainMenu()
    {
        // TODO save game
        ScenesController.Instance.ChangeScene("MenuBackgroundScene");
        SetActivePanel(mainMenuPanel);
    }
    
    public void ShowLastPanel()
    {
        if (_lastPanel)
        {
            SetActivePanel(_lastPanel);
        }
    }

    public void AskExitGame()
    {
        _onConfirmAction = Application.Quit;
        SetActivePanel(areYouSurePanel);
    }

    private void AskNewGame()
    {
        _onConfirmAction = CreateNewGame;
        SetActivePanel(areYouSurePanel);
    }
    
    public void OnAreYouSureYes()
    {
        _onConfirmAction?.Invoke();
        _onConfirmAction = null;
    }
    
    public void OnAreYouSureNo()
    {
        _onConfirmAction = null;
        SetActivePanel(_lastPanel);
    }
}
