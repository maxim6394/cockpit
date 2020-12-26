using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public static MenuManager instance;

    public MySceneManager sceneManager = null;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        if (sceneManager == null)
            sceneManager = FindObjectOfType<MySceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsOpen
    {
        get { return gameObject.activeSelf;  }
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void ResetButtonClicked()
    {
        sceneManager.ReloadScene();
    }

    public void StartBeforeTaxiCourse()
    {
        sceneManager.StartTrainingCouse(MySceneManager.TrainingCourse.BEFORE_TAXI);
    }
}
