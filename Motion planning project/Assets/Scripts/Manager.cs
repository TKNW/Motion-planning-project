using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    [Header("Environment setting")]
    [Tooltip("The number of obstacles")]
    [Range(5,30)]
    public int ObstacleAmount = 10;
    public GameObject[] ObstacleType = new GameObject[2];
    public GameObject Goal = null;
    public GameObject Player = null;


    private bool TooClose(GameObject obj1, GameObject obj2, double maxdistance = 2.0f) 
    {
        if(obj1 == null || obj2 == null)
            return false;
        bool result = false;
        double XPow = Math.Pow(obj1.transform.position.x - obj2.transform.position.x, 2);
        double ZPow = Math.Pow(obj1.transform.position.z - obj2.transform.position.z, 2);
        double dis = Math.Sqrt(XPow + ZPow);
        //Debug.Log(dis);
        if (dis <= maxdistance)
            result = true;
        return result;
    }
    private void GenerateObstacle()
    {
        GameObject NewObscale = null;
        int RandomIndex = UnityEngine.Random.Range(0, ObstacleType.Length);
        for (int i = 0;i < ObstacleAmount; ++i)
        {
            NewObscale = Instantiate(ObstacleType[RandomIndex],
                new Vector3(UnityEngine.Random.Range(2, 38),1, UnityEngine.Random.Range(2, 38)),
                new Quaternion(0,0,0,0));
            if (TooClose(Player, NewObscale) || TooClose(Goal, NewObscale)) 
            {
                Debug.Log("Find too close");
                Destroy(NewObscale);
                --i;
            }
            RandomIndex = UnityEngine.Random.Range(0, ObstacleType.Length);
        }
    }

    void ArriveGoal() 
    {
        Debug.Log("Manager received.");
        SceneManager.LoadScene("Path");
    }
    void Awake()
    {
        Goal = GameObject.Find("Goal");
        if (Goal == null)
        {
            Debug.LogError("Can't find Goal.", Goal);
        }
        Player = GameObject.Find("PlayerArmature");
        if (Player == null)
        {
            Debug.LogError("Can't find Player.", Player);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateObstacle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
