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

    public static TrainingCourse CurrentTrainingCourse { get { return currentTrainingCourse; } }

    public enum TrainingCourse { NONE, TEST, BEFORE_TAXI, LANDING, AFTER_LANDING };

    private float timeUntilEnd = -1f;

    public AudioSource ErrorSound;
    public AudioSource SuccessSound;

    private GameObject xrRig;

    private GameObject ground;

    private readonly Dictionary<TrainingCourse, string[]> trainingProcesses = new Dictionary<TrainingCourse, string[]>()
    {
        {
            TrainingCourse.BEFORE_TAXI, new [] { "SEA-BELT-SIGN-ON", "StabTrim", "FlapsLever", "FlightControls", "TAXI-LIGHTS", "Transponder-ALT", "PARKING-BRAKE" }
        },
        {
            TrainingCourse.LANDING, new [] { "SPEED-BRAKE", "FlapsLever", "LANDING-GEAR" }
        },
        {
            TrainingCourse.AFTER_LANDING, new [] { "FlapsLever", "SPEED-BRAKE", "LANDING-LIGHTS4" }
        },
        {
            TrainingCourse.NONE, new [] { "LANDING-GEAR", "TAXI-LIGHTS" }
        },
        {
            TrainingCourse.TEST, new [] { "FlightControls" }
        }
    };

    private Dictionary<string, string> instructions = new Dictionary<string, string>()
    {
        { "SEA-BELT-SIGN-ON", "Switch Seat Belt Sign on" },
        { "StabTrim", "Adjust Stab Trim" },
        { "FlapsLever", "Set Flaps as required" },
        { "FlightControls", "Check Flight Controls" },
        { "TAXI-LIGHTS", "Switch Taxi Lights on" },
        { "Transponder-ALT", "Set Transponder to ALT OFF" },
        { "PARKING-BRAKE", "Parking Brake Off" },

        { "SPEED-BRAKE", "Set Speed Brakes as required" },
        { "LANDING-GEAR", "Put Landing Gear Down" },
        { "LANDING-LIGHTS4", "Switch Landing Lights off"}
    };

    private readonly List<string> wrongSelections = new List<string>();

   

    private List<string> selectedObjects = new List<string>();
    private int currentStage = 0;
    
    private GameObject environment;

    private int mistakes = 0;

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

    private string getInstructionText()
    {
        if (instructions.TryGetValue(getCorrectObjectName(), out string text))
            return text;
        return "";
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

        xrRig = GameObject.Find("XR Rig");
        CockpitController = GameObject.Find("RightHandCockpitController");
        UIController = GameObject.Find("RightHandUIController");        
        
        environment = GameObject.Find("Environment");
        ground = GameObject.Find("Ground");
    }


    protected void ShowMenu()
    {
        MenuManager.instance.Show();
        CockpitController.SetActive(false);
        UIController.SetActive(true);
        environment.SetActive(false);
    }

    protected void HideMenu()
    {
        MenuManager.instance.Hide();
        CockpitController.SetActive(true);
        UIController.SetActive(false);
        environment.SetActive(true);
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

        if (timeUntilEnd > 0)
        {
            timeUntilEnd -= Time.deltaTime;

            if(timeUntilEnd <= 0)
            {
                FinishTraining();
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {

        if (currentTrainingCourse == TrainingCourse.NONE)
            ShowMenu();
        else
        {
            MenuManager.instance.CourseStarted();
            HideMenu();
            MenuManager.instance.SetInstructionText(getInstructionText());

            if(currentTrainingCourse == TrainingCourse.LANDING)
            {
                ground.transform.position = new Vector3(0, -22, 64);
            }
        }

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

        xrRig.transform.position = new Vector3(-0.22f, 1.53f, -0.474f);
        
        if (xrInput != null)
        {                        
            xrInput.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            xrInput.TryRecenter();
        }

        
    }

    private void FinishTraining()
    {
        MenuManager.instance.CourseFinished(mistakes);
        if (SuccessSound != null)
            SuccessSound.Play();
        ShowMenu();
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
        if (interactable is XRSimpleInteractable)
        {
            if (interactable.GetComponent<Rigidbody>() == null)
            {
                var r = interactable.gameObject.AddComponent<Rigidbody>();
                r.isKinematic = true;
                r.useGravity = false;
            }
        }

        if (interactable.GetComponent<Outline>() == null)
        {
            var outline = interactable.gameObject.AddComponent<Outline>();
            outline.OutlineWidth = 5;
            outline.OutlineColor = Color.yellow;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.enabled = false;
            //outline.OutlineMode = Outline.Mode.OutlineVisible;
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
            wrongSelections.Clear();
            resetOutlines();
            outline.OutlineColor = Color.green;
            

            var sound = interactable.GetComponent<AudioSource>();
            if (sound != null)
                sound.Play();

            var transformChanger = interactable.GetComponent<TransformChanger>();
            if (transformChanger != null)
            {
                transformChanger.ChangeTransform();
            }

            if (++currentStage == trainingProcesses[currentTrainingCourse].Length)
            {
                timeUntilEnd = 1f;
                return;
            }
            else
            {
                MenuManager.instance.SetInstructionText(getInstructionText());
            }
        }
        else if(!selectedObjects.Contains(interactable.name))
        {

            if (ErrorSound != null)
                ErrorSound.Play();
            if(!wrongSelections.Contains(interactable.name))
            {
                wrongSelections.Add(interactable.name);
                outline.OutlineColor = Color.red;
                mistakes++;
            }
        }
    }

    public override void SelectExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        base.SelectExit(interactor, interactable);
        //interactable.GetComponent<Outline>().enabled = false;
    }


}
