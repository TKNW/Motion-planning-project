using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalController : MonoBehaviour
{
    public GameObject Manager;
    // Start is called before the first frame update
    private void Awake()
    {
        Manager = GameObject.Find("EnvManager");
        if (Manager == null)
        {
            Debug.LogError("Can't find EnvManager.", Manager);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Manager.SendMessage("ArriveGoal");
        }
    }
}
