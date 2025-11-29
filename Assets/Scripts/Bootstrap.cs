using System.Collections;
using UnityEngine;
using static UIController;

public class Bootstrap : MonoBehaviour
{
    void Start()
    {
        ScenesController.LoadSceneAdditive("MenuBackgroundScene");
        StartCoroutine(InitializeUI());
    }
    
    private IEnumerator InitializeUI()
    {
        yield return null;
        
        if (UIControllerInstance && UIControllerInstance.titlePanel)
        {
            UIControllerInstance.SetActivePanel(UIControllerInstance.titlePanel);
            UIControllerInstance.WaitForAnyButton();
        }
    }
}
