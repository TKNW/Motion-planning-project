using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PRM : MonoBehaviour
{
    /* 地圖 4000*4000 / 25 -> 160*160
     * 起點 (200, 200) -> (8, 8)
     * 終點 (3700, 3700) -> (148, 148)
     * 障礙物 350*200 -> 14*8
     * 障礙物 350*300 -> 14*12
     */

    private int[,] Plane_map = new int[161, 161];
    public const int sampleNum = 500;
    private List<Vector2> sampleMap = new List<Vector2>();

    private List<Vector2>[] edges = new List<Vector2>[sampleNum];
    private double[] dist = new double[sampleNum];
    private int[] path = new int[sampleNum];
    public const int dis = 30;
    public const int split = 10;
    List<int> resultRRT = new List<int>();
    List<int> resultPRM = new List<int>();

    public GameObject robot;
    public Animator Ani = null;
    public GameObject cube;
    public GameObject StartPoint = null;
    public GameObject Goal = null;
    private int resultPoint;

    double rrt_dist = 0;
    
    Vector2 ConvertXY(Vector2 input)
    {
        input.x *= 4;
        input.y *= 4;
        return input;
    }
    void ResetPoint()
    {
        sampleMap.Clear();
        resultRRT.Clear();
        resultPRM.Clear();
    }
    public List<Vector3> GetRRTResult()
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 pos = new Vector3();
        for (int i = resultRRT.Count - 1; i >= 0;--i)
        {
            pos = new Vector3(sampleMap[resultRRT[i]].x * 0.25f, 0.5f, sampleMap[resultRRT[i]].y * 0.25f);
            result.Add(pos);
        }
        return result;
    }
    public List<Vector3> GetPRMResult()
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 pos = new Vector3();
        for (int i = resultPRM.Count - 1; i >= 0; --i)
        {
            pos = new Vector3(sampleMap[resultPRM[i]].x * 0.25f, 0.5f, sampleMap[resultPRM[i]].y * 0.25f);
            result.Add(pos);
        }
        return result;
    }
    // 計算障礙物位置
    void CountObstacle()
    {
        Array.Clear(Plane_map, 0, Plane_map.Length);
        foreach (GameObject Obstacle in Manager.GeneratedObstacles)
        {
            int pos_x = (int)Obstacle.transform.position.x * 4;
            int pos_z = (int)Obstacle.transform.position.z * 4;
            int x = 7, z = 4;

            if (Obstacle.name == "Box_350x250x300(Clone)") z = 6;
            x += 1; z += 1; // 避免碰到障礙物邊緣

            for (int i = pos_x - x; i <= pos_x + x; ++i)
            {
                for (int j = pos_z - z; j <= pos_z + z; ++j)
                {
                    if (i < 0 || j < 0 || i > 160 || j > 160) continue;
                    Plane_map[i, j] = 1;
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*// 操控機器人移動
        if (canMove)
        {
            Vector3 pos = new Vector3(sampleMap[resultRRT[resultPoint]].x * 0.25f, 0, sampleMap[resultRRT[resultPoint]].y * 0.25f);
            robot.transform.LookAt(new Vector3(pos.x, robot.transform.position.y, pos.z));
            robot.transform.position = Vector3.MoveTowards(robot.transform.position, pos, 15 * Time.deltaTime);
            //print("target: " + pos + " map pos: " + pos * 4);
            cube.transform.position = pos;

            if (Vector3.Distance(robot.transform.position, pos) < 0.01f)
            {
                //canMove = false;
                //print("target: " + pos + ", robot: " + robot.transform.position);
                --resultPoint;
            }           
            //if (robot.transform.position == pos) --resultPoint;
        }

        // run prm
        if (Input.GetKeyDown(KeyCode.C))
        {
            prm();
        }

        // run rrt
        if (Input.GetKeyDown(KeyCode.V))
        {
            rrt();
        }*/
    }

    void prm()
    {
        CountObstacle();
        // 隨機產生 sampleNum 個點
        sampleMap.Add(ConvertXY(new Vector2(StartPoint.transform.position.x,StartPoint.transform.position.z)));  // 起點
        sampleMap.Add(ConvertXY(new Vector2(Goal.transform.position.x, Goal.transform.position.z)));  // 終點
        while (sampleMap.Count < sampleNum)
        {
            int x = UnityEngine.Random.Range(8, 149), y = UnityEngine.Random.Range(8, 149);

            if (Plane_map[x, y] == 0)
            {
                //print("(" + x + ", " + y + ")");
                sampleMap.Add(new Vector2(x, y));
            }
        }

        // 連接點，產生連線
        int num = sampleMap.Count;

        for (int i = 0; i < sampleNum; ++i) edges[i] = new List<Vector2>();

        for (int i = 0; i < num; ++i)
        {
            for (int j = 0; j < num; ++j)
            {
                if (i != j && clacDistance(sampleMap[i], sampleMap[j]) < dis && checkPath(sampleMap[i], sampleMap[j]))
                {
                    //print(i + " " + j);
                    edges[i].Add(new Vector2(j, (float)clacDistance(sampleMap[i], sampleMap[j])));
                    //print("(" + i + ", " + j + ")" + ": " + (float)clacDistance(sampleMap[i], sampleMap[j]));
                    //print("a");
                }
            }
        }

        dijkstra(0, 1);
    }

    void rrt()
    {
        CountObstacle();
        sampleMap.Add(ConvertXY(new Vector2(StartPoint.transform.position.x, StartPoint.transform.position.z)));  // 起點
        Vector2 endPoint = ConvertXY(new Vector2(Goal.transform.position.x, Goal.transform.position.z));  // 終點
        int ClostestPoint;

        while (true)
        {
            ClostestPoint = findClostest(endPoint);
            if (checkPath(endPoint, sampleMap[ClostestPoint]))
            {
                path[sampleMap.Count] = ClostestPoint;
                sampleMap.Add(endPoint);
                break;
            }

            int x = UnityEngine.Random.Range(8, 149), y = UnityEngine.Random.Range(8, 149);
            if (Plane_map[x, y] == 0)
            {
                ClostestPoint = findClostest(new Vector2(x, y));
                if (checkPath(new Vector2(x, y), sampleMap[ClostestPoint]))
                {
                    path[sampleMap.Count] = ClostestPoint;
                    sampleMap.Add(new Vector2(x, y));
                }
            }
        }

        int t = sampleMap.Count - 1;
        while (t != 0)
        {
            resultRRT.Add(t);
            t = path[t];
        }
        
        for (int i = 1; i < resultRRT.Count; ++i)
        {
            rrt_dist += clacDistance(sampleMap[resultRRT[i]], sampleMap[resultRRT[i - 1]]);
            //print("(" + sampleMap[result[i]].x + ", " + sampleMap[result[i]].y + "): " + Plane_map[(int)sampleMap[result[i]].x, (int)sampleMap[result[i]].y]);
        }

        //print("dist: " + rrt_dist * 25);

        resultPoint = resultRRT.Count - 1;
    }

    int findClostest(Vector2 a)
    {
        double dis = Double.MaxValue;
        int num = 0;

        for (int i = 0; i < sampleMap.Count; ++i)
        {
            if (clacDistance(a, sampleMap[i]) < dis)
            {
                dis = clacDistance(a, sampleMap[i]);
                num = i;
            }
        }

        return num;
    }

    double clacDistance(Vector2 a, Vector2 b)
    {
        return Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
    }

    bool checkPath(Vector2 a, Vector2 b)
    {
        float intervalX = (b.x - a.x) / split;
        float intervalY = (b.y - a.y) / split;

        for (int i = 0; i <= split; ++i)
        {
            float newX = a.x + i * intervalX;
            float newY = a.y + i * intervalY;
            //print("(" + a.x + ", " + a.y + ")" + "(" + b.x + ", " + b.y + ")" + newX + " " + newY);
            if (Plane_map[(int)newX, (int)newY] == 1) return false;
        }

        return true;
    }

    void dijkstra(int s, int t)
    {
        List<Vector2> pq = new List<Vector2>();

        for (int i = 0; i < sampleNum; ++i) dist[i] = Double.MaxValue;

        dist[s] = 0;
        pq.Add(new Vector2(s, (float)dist[s]));

        while(pq.Count > 0)
        {
            Vector2 x = pq[0];
            pq.RemoveAt(0);
            //print(edges[(int)x.x].Count);

            for (int i = 0; i < edges[(int)x.x].Count; ++i)
            {
                Vector2 y = edges[(int)x.x][i];
                if (dist[(int)y.x] > dist[(int)x.x] + y.y)
                {
                    dist[(int)y.x] = dist[(int)x.x] + y.y;
                    path[(int)y.x] = (int)x.x;
                    pq.Add(new Vector2(y.x, (float)dist[(int)y.x]));
                    pq.Sort((a, b) => a.y.CompareTo(b.y));
                }
            }
        }

        //print("Distance: " + dist[t] * 25);

        while (t != 0)
        {
            resultPRM.Add(t);
            t = path[t];
        }

        for(int i = 0; i < resultPRM.Count; ++i)
        {
            //print("(" + sampleMap[result[i]].x + ", " + sampleMap[result[i]].y + "): " + Plane_map[(int)sampleMap[result[i]].x, (int)sampleMap[result[i]].y]);
        }

        resultPoint = resultPRM.Count - 1;
        
        //print(result.Count);
    }
}
