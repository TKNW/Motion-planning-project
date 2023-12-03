using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
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
    public GameObject StartPoint = null;
    public PRM PrmScript = null;
    public bool GenerateObstacleOnStart = false;
    public bool MoveGoal = false;
    public bool UsePRM = false;
    public bool UseRRT = false;
    public bool UseDij = false;
    public Material RRTColor;
    public Material PRMColor;
    private LineRenderer RRTlineRenderer = null;
    private LineRenderer PRMLineRenderer = null;
    static public List<GameObject> GeneratedObstacles = new List<GameObject>();

    private List<Vector3> RRTPoint = new List<Vector3>();
    private List<Vector3> PRMPoint = new List<Vector3>();
    private bool TooClose(GameObject obj,double x, double z, double maxdistance = 2.0f) 
    {
        if(obj == null)
            return false;
        bool result = false;
        double XPow = Math.Pow(obj.transform.localPosition.x - x, 2);
        double ZPow = Math.Pow(obj.transform.localPosition.z - z, 2);
        double dis = Math.Sqrt(XPow + ZPow);
        if (dis <= maxdistance)
            result = true;
        return result;
    }
    public void GenerateObstacle()
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

    private void EnvReset(bool KillObstacle = true)
    {
        //CharacterController會覆蓋Transform，所以要先關掉再改
        if (MoveGoal == false)
        {
            Player.GetComponent<CharacterController>().enabled = false;
            Player.transform.localPosition = StartPoint.transform.localPosition;
            Player.transform.localRotation = StartPoint.transform.localRotation;
            Player.GetComponent<CharacterController>().enabled = true;
        }
        if(MoveGoal == true)
        {
            RegenerateGoal();
        }
        if (KillObstacle == true)
        {
            foreach (GameObject obj in GeneratedObstacles)
            {
                Destroy(obj);
            }
            GeneratedObstacles.Clear();
            GenerateObstacle();
        }
        if(UseRRT == true)
        {
            if (RRTlineRenderer != null)
            {
                Destroy(RRTlineRenderer.gameObject);
            }
            PrmScript.SendMessage("ResetPoint");
            PrmScript.SendMessage("rrt");
            RRTPoint = PrmScript.GetRRTResult();
            RRTPoint.Insert(0, StartPoint.transform.position);
            DrawLine(RRTPoint, ref RRTlineRenderer, RRTColor);

        }
        if(UsePRM == true)
        {
            if(PRMLineRenderer != null)
            { 
                Destroy(PRMLineRenderer.gameObject);
            }
            PrmScript.SendMessage("ResetPoint");
            PrmScript.SendMessage("prm");
            PRMPoint = PrmScript.GetPRMResult();
            PRMPoint.Insert(0, StartPoint.transform.position);
            DrawLine(PRMPoint, ref PRMLineRenderer, PRMColor);
        }
    }
    void DrawLine(List<Vector3> vectors, ref LineRenderer renderer, Material material)
    {
        renderer = new GameObject("Line").AddComponent<LineRenderer>();
        renderer.material = material;
        renderer.startWidth = 0.25f;
        renderer.endWidth = 0.25f;
        renderer.positionCount = vectors.Count;
        renderer.useWorldSpace = true;
        for(int i = 0; i < vectors.Count; i++)
        {
            renderer.SetPosition(i, vectors[i]);
        }
    }
    void RegenerateGoal()
    {
        bool GoodResult = false;
        double newx = 0.0, newz = 0.0;
        while (!GoodResult)
        {
            newx = UnityEngine.Random.Range(2, 38);
            newz = UnityEngine.Random.Range(2, 38);
            foreach (GameObject obj in GeneratedObstacles)
            {
                if (TooClose(obj, newx, newz, 5.0))
                {
                    newx = -1;
                    break;
                }
            }
            if (newx == -1)
            {
                continue;
            }
            if (TooClose(Player, newx, newz, 20.0) == false)
            {
                GoodResult = true;
            }
        }
        Goal.transform.position = new Vector3((float)newx, 0.3f, (float)newz);
    }
    void ArriveGoal() 
    {
        EnvReset();
    }
    void SetMoveGoal(bool set)
    {
        MoveGoal = set;
    }
    void SetStartPoint(Transform newTransfrom)
    {
        if(MoveGoal == false) { return; }
        StartPoint.transform.position = newTransfrom.position;
        StartPoint.transform.rotation = newTransfrom.rotation;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(GenerateObstacleOnStart == true)
        {
            GenerateObstacle();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
