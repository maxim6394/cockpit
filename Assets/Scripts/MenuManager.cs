using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public static MenuManager instance;

    public MySceneManager sceneManager = null;

    private GameObject courseFinished;
    private GameObject mainMenu;
    private TextMeshProUGUI mistakesText;
    private GameObject resetCourseButton;

    private TextMeshProUGUI instructionText;

    private TextMeshProUGUI trainingCourseText;

    private Canvas canvas = null;

    private GameObject circlingPlane, circlingPlane2;


    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);


        if (sceneManager == null)
            sceneManager = FindObjectOfType<MySceneManager>();

        if (canvas == null)
            canvas = gameObject.GetComponentInChildren<Canvas>();

        courseFinished = GameObject.Find("CourseFinished");
        mainMenu = GameObject.Find("MainMenu");
        mistakesText = GameObject.Find("MistakesText").GetComponent<TextMeshProUGUI>();
        courseFinished.SetActive(false);

        instructionText = GameObject.Find("InstructionText").GetComponent<TextMeshProUGUI>();

        circlingPlane = GameObject.Find("CirclingPlane");
        circlingPlane2 = GameObject.Find("CirclingPlane2");

//        trainingCourseText = GameObject.Find("TrainingCourseText").GetComponent<TextMeshProUGUI>();
 //       trainingCourseText.text = "";

    }


    // Update is called once per frame
    void Update()
    {
        var y = canvas.transform.position.y;
        canvas.transform.LookAt(Camera.main.transform.position);
        var forward = Camera.main.transform.forward;
        canvas.transform.position = Camera.main.transform.position + new Vector3(forward.x, 0, forward.z).normalized * 5;
        //transform.position = new Vector3(transform.position.x, y, transform.position.z);

        canvas.transform.Rotate(0, 180, 0);

        circlingPlane.transform.Rotate(0, -.03f, 0);
        circlingPlane2.transform.Rotate(0, .03f, 0);
    }

    public void SetInstructionText(string text)
    {
        instructionText.text = text;
    }

    public void CourseFinished(int mistakes)
    {
        courseFinished.SetActive(true);
        mainMenu.SetActive(false);
        mistakesText.text = "mistakes: " + mistakes;        
    }

    public void CourseStarted()
    {
        //trainingCourseText.text = "Current Course: "+MySceneManager.CurrentTrainingCourse.ToString();
        instructionText.text = "asdf";
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

    public void ToMainMenu()
    {
        courseFinished.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void ResetButtonClicked()
    {
        sceneManager.ReloadScene();
    }

    public void StartBeforeTaxiCourse()
    {
        sceneManager.StartTrainingCouse(MySceneManager.TrainingCourse.BEFORE_TAXI);
    }
    public void StartLandingCourse()
    {
        sceneManager.StartTrainingCouse(MySceneManager.TrainingCourse.LANDING);
    }
    public void StartAfterLandingCourse()
    {
        sceneManager.StartTrainingCouse(MySceneManager.TrainingCourse.AFTER_LANDING);
    }

    public void StartTestCourse()
    {
        sceneManager.StartTrainingCouse(MySceneManager.TrainingCourse.TEST);
    }
}
