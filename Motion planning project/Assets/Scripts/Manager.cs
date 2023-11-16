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
    private GameObject Player = null;
    public GameObject StartPoint = null;
    private List<GameObject> GeneratedObstacles = new List<GameObject>();

    private bool TooClose(GameObject obj,double x, double z, double maxdistance = 2.0f) 
    {
        if(obj == null)
            return false;
        bool result = false;
        double XPow = Math.Pow(obj.transform.position.x - x, 2);
        double ZPow = Math.Pow(obj.transform.position.z - z, 2);
        double dis = Math.Sqrt(XPow + ZPow);
        if (dis <= maxdistance)
            result = true;
        return result;
    }
    private void GenerateObstacle()
    {
        GameObject NewObscale;
        double newx = 0.0, newz = 0.0;
        int RandomIndex = UnityEngine.Random.Range(0, ObstacleType.Length);
        bool GoodResult = false;
        for (int i = 0;i < ObstacleAmount; ++i)
        {
            while (!GoodResult)
            {
                newx = UnityEngine.Random.Range(2, 38);
                newz = UnityEngine.Random.Range(2, 38);
                foreach (GameObject obj in GeneratedObstacles)
                {
                    if(TooClose(obj, newx, newz, 5.0))
                    {
                        newx = -1;
                        break;
                    }
                }
                if(newx == -1)
                {
                    continue;
                }
                if (TooClose(Player, newx, newz, 5.0) == false
                    && TooClose(Goal, newx, newz, 5.0) == false)
                {
                    GoodResult = true;
                }
            }
            GoodResult = false;
            NewObscale = Instantiate(ObstacleType[RandomIndex],
                new Vector3((float)newx, 1, (float)newz),
                new Quaternion(0, 0, 0, 0));
            RandomIndex = UnityEngine.Random.Range(0, ObstacleType.Length);
            GeneratedObstacles.Add(NewObscale);
        }
    }

    private void Reset()
    {
        //CharacterController會覆蓋Transform，所以要先關掉再改
        Player.GetComponent<CharacterController>().enabled = false;
        Player.transform.position = StartPoint.transform.position;
        Player.transform.rotation = StartPoint.transform.rotation;
        Player.GetComponent<CharacterController>().enabled = true;
        foreach (GameObject obj in GeneratedObstacles)
        {
           Destroy(obj);
        }
        GeneratedObstacles.Clear();
        GenerateObstacle();
    }

    void ArriveGoal() 
    {
        Reset();
    }

    // Start is called before the first frame update
    void Start()
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
        StartPoint = GameObject.Find("StartPoint");
        if (StartPoint == null)
        {
            Debug.LogError("Can't find StartPoint.", StartPoint);
        }
        GenerateObstacle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
