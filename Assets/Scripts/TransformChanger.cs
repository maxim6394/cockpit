using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MySceneManager;

[Serializable]
public struct RotationChange
{
    public TrainingCourse course;
    public Vector3 rotation;
}

public class TransformChanger : MonoBehaviour
{
    public RotationChange[] RotationChanges;
    public RotationChange[] InitialRotations;

    private int currentState = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        var v = InitialRotations.Where(r => r.course == MySceneManager.CurrentTrainingCourse);
        
        if(v.Any())
        {
            transform.Rotate(v.First().rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeTransform()
    {
        var v = RotationChanges.Where(r => r.course == MySceneManager.CurrentTrainingCourse);

        if (v.Any())
        {
            transform.Rotate(v.First().rotation);
        }        

    }
}
