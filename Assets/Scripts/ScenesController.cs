using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    public static ScenesController Instance { get; private set; }
    private string _currentBackgroundScene;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneCoroutine(sceneName));
    }
    
    public static void LoadSceneAdditive(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.LoadSceneAdditiveCoroutine(sceneName));
        }
    }
    
    private IEnumerator LoadSceneAdditiveCoroutine(string sceneName)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (loadOp != null && !loadOp.isDone)
            yield return null;
        
        _currentBackgroundScene = sceneName;
    }
    
    private IEnumerator ChangeSceneCoroutine(string sceneName)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (loadOp != null && !loadOp.isDone)
            yield return null;
        
        if (!string.IsNullOrEmpty(_currentBackgroundScene))
        {
            yield return SceneManager.UnloadSceneAsync(_currentBackgroundScene);
        }
        
        _currentBackgroundScene = sceneName;
    }
}
