using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

public class MySceneManager : XRInteractionManager
{

    public static MySceneManager instance;
    private static TrainingCourse currentTrainingCourse = TrainingCourse.NONE;

    public enum TrainingCourse { NONE, BEFORE_TAXI };

    private readonly Dictionary<TrainingCourse, string[]> trainingProcesses = new Dictionary<TrainingCourse, string[]>()
    {
        {
            TrainingCourse.BEFORE_TAXI, new [] { "SeatbeltSignSwitch", "ElevatorTrimLever", "FlapsLever", "FlightControls", "TaxiLightsSwitch", "ParkingBrake"}
        },
        {
            TrainingCourse.NONE, new [] { "LandingGear" }
        }
    };

    private readonly Dictionary<string, string> animationMapping = new Dictionary<string, string>()
    {
        {"SpeedBrake", "SPEED-BRAKE|SPEED-BRAKE-IDLE" },
        {"ParkingBrake", "PARKING-BRAKE|PARKING-BRAKEAction" },
        {"LandingGear", "LANDING-GEAR|LANDING-GEAR-UP" },
    };


    private Animation cockpitAnimations; 

    private List<string> selectedObjects = new List<string>();
    private int currentStage = 0;

    GameObject UIController;
    GameObject CockpitController; 

    private bool menuButtonPressed = false;

    public void StartTrainingCouse(TrainingCourse  process)
    {
        currentTrainingCourse = process;
        ReloadScene();
    }

    public void ReloadScene()
    {
        Scene scene = SceneManager.GetActiveScene();        
        SceneManager.LoadScene(scene.name);
    }
   

    private string getCorrectObjectName()
    {
        return trainingProcesses[currentTrainingCourse][currentStage];
    }

    protected void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        CockpitController = GameObject.Find("RightHandCockpitController");
        UIController = GameObject.Find("RightHandUIController");
        cockpitAnimations = GameObject.Find("Cockpit").GetComponent<Animation>();

    }


    protected void ShowMenu()
    {
        MenuManager.instance.Show();
        CockpitController.SetActive(false);
        UIController.SetActive(true);
    }

    protected void HideMenu()
    {
        MenuManager.instance.Hide();
        CockpitController.SetActive(true);
        UIController.SetActive(false);
    }


    protected override void Update()
    {
        base.Update();
        var device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if(device != null)
        {
            if(device.TryGetFeatureValue(CommonUsages.primaryButton, out bool val)) {
                if(!val && menuButtonPressed)
                {
                    if (MenuManager.instance.IsOpen)
                        HideMenu();
                    else
                        ShowMenu();

                }
                menuButtonPressed = val;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        if (currentTrainingCourse == TrainingCourse.NONE)
            ShowMenu();
        else
            HideMenu();

        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            Debug.Log($"XRGeneralSettings is null.");
            return;
        }

        var xrManager = xrSettings.Manager;
        if (xrManager == null)
        {
            Debug.Log($"XRManagerSettings is null.");
            return;
        }

        var xrLoader = xrManager.activeLoader;
        if (xrLoader == null)
        {
            Debug.Log($"XRLoader is null.");
            return;
        }


        var xrInput = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
        Debug.Log($"XRInput: {xrInput != null}");

        if (xrInput != null)
        {            
            xrInput.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            xrInput.TryRecenter();
            
        }
    }

    private void FinishTraining()
    {

    }

    private void resetOutlines()
    {
        foreach(var obj in FindObjectsOfType<XRBaseInteractable>())
        {
            if(!selectedObjects.Contains(obj.name))
                obj.GetComponent<Outline>().OutlineColor = Color.yellow;
        }
    }

    public override void RegisterInteractable(XRBaseInteractable interactable)
    {
        base.RegisterInteractable(interactable);
        if (interactable.GetComponent<Outline>() == null)
        {
            var outline = interactable.gameObject.AddComponent<Outline>();
            outline.OutlineWidth = 5;
            outline.OutlineColor = Color.yellow;
            outline.enabled = false;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }
    }   

    public override void HoverEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        base.HoverEnter(interactor, interactable);        
        var outline = interactable.GetComponent<Outline>();
        if (outline)
            outline.enabled = true;
    }

    public override void HoverExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        base.HoverExit(interactor, interactable);
        var outline = interactable.GetComponent<Outline>();
        if (!selectedObjects.Contains(interactable.name))
            outline.enabled = false;
    }

    public override void SelectEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        base.SelectEnter(interactor, interactable);
        var outline = interactable.GetComponent<Outline>();
        if (getCorrectObjectName() == interactable.name)
        {
            selectedObjects.Add(interactable.name);
            resetOutlines();
            outline.OutlineColor = Color.green;

            if(animationMapping.TryGetValue(interactable.name, out string animationName))
            {
                cockpitAnimations.Play(animationName);
                Debug.Log("Playing Animation " + animationName);
            }

            if (++currentStage == trainingProcesses[currentTrainingCourse].Length)
            {
                FinishTraining();
                return;
            }
        }
        else if(!selectedObjects.Contains(interactable.name))
        {
            outline.OutlineColor = Color.red;
        }
    }

    public override void SelectExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        base.SelectExit(interactor, interactable);
        //interactable.GetComponent<Outline>().enabled = false;
    }


}
