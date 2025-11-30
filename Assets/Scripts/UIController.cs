using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private const float LoadingDuration = 3f;
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

    [Header("Camera Zoom")]
    [Tooltip("Reference to the MenuBackgroundScene camera zoom controller. Leave empty to auto-locate.")]
    public MenuCameraZoom menuCameraZoom;
    [Tooltip("Focus point for the camera while the loading controls are visible.")]
    public Transform loadingCameraFocus;
    
    private GameObject[] _allPanels;
    private GameObject _lastPanel;
    private System.Action _onConfirmAction;
    
    private bool _waitingForAnyButton;
    private System.Action _onAnyButtonPressed;
    
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

    private void EnsureMenuCameraReference()
    {
        if (menuCameraZoom != null)
        {
            return;
        }

        var menuScene = SceneManager.GetSceneByName("MenuBackgroundScene");
        if (menuScene.IsValid() && menuScene.isLoaded)
        {
            foreach (var root in menuScene.GetRootGameObjects())
            {
                menuCameraZoom = root.GetComponentInChildren<MenuCameraZoom>(true);
                if (menuCameraZoom != null)
                {
                    return;
                }
            }
        }

        menuCameraZoom = FindObjectOfType<MenuCameraZoom>();
    }

    private void ResetMenuCameraIfAvailable()
    {
        EnsureMenuCameraReference();
        if (menuCameraZoom != null)
        {
            menuCameraZoom.ResetZoom();
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
                ShowMainMenu();
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
        ResetMenuCameraIfAvailable();
    }
    
    public void ShowNewGame()
    {
        ShowGame();
    }

    private void ShowLoadingControls()
    {
        mainMenuPanel.SetActive(false);
        loadingControlsPanel.SetActive(true);

        foreach (var panel in _allPanels)
        {
            if (panel != loadingControlsPanel && panel != mainMenuPanel)
            {
                panel.SetActive(false);
            }
        }

        EnsureMenuCameraReference();
        if (menuCameraZoom != null)
        {
            if (loadingCameraFocus != null)
            {
                menuCameraZoom.ZoomToTarget(loadingCameraFocus);
            }
            else
            {
                menuCameraZoom.ResetZoom();
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
        ScenesController.Instance.ChangeScene("Level1");
        SetActivePanel(hudPanel);
        ResetMenuCameraIfAvailable();
    }
    
    public void ShowCredits()
    {
        SetActivePanel(creditsPanel);
        ResetMenuCameraIfAvailable();
    }

    private void ShowPause()
    {
        SetActivePanel(pausePanel);
    }

    public void ShowHUD()
    {
        SetActivePanel(hudPanel);
        ResetMenuCameraIfAvailable();
    }
    

    public void SaveAndShowMainMenu()
    {
        ScenesController.Instance.ChangeScene("MenuBackgroundScene");
        SetActivePanel(mainMenuPanel);
        ResetMenuCameraIfAvailable();
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
