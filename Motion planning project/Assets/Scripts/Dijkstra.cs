using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using System;

public class Dijkstra : MonoBehaviour
{
    public GameObject RoutePoint = null;
    public GameObject[] Points = null;
    public GameObject StartPoint = null;
    public GameObject Goal = null;
    public int NearestSP;
    public int NearestGL;

    void GenerateDijPoint()
    {
        for (int row = 2; row <= 38; row += 2)
        {
            for (int col = 2; col <= 38; col += 2)
            {
                Instantiate(RoutePoint, new Vector3(row, 0.5f, col), transform.rotation);
            }
        }
    }
    public List<Vector3> DoDijkstra()
    {
        List<Vector3> result = new List<Vector3>();
        double XPow, ZPow, dis;
        double minDistance = double.MaxValue;
        int indexOfMinDistance = -1;
        Points = GameObject.FindGameObjectsWithTag("Point");
        int[,] Pointarray = new int[Points.Length, Points.Length];
        for (int i = 0; i < Points.Length; ++i)
        {
            XPow = Math.Pow(Points[i].transform.localPosition.x - StartPoint.transform.localPosition.x, 2);
            ZPow = Math.Pow(Points[i].transform.localPosition.z - StartPoint.transform.localPosition.z, 2);
            dis = Math.Sqrt(XPow + ZPow);
            if (dis < minDistance)
            {
                minDistance = dis;
                indexOfMinDistance = i;
            }
        }
        if (indexOfMinDistance != -1)
        {
            NearestSP = indexOfMinDistance;
        }
        minDistance = double.MaxValue;
        for (int j = 0; j < Points.Length; ++j)
        {
            XPow = Math.Pow(Points[j].transform.localPosition.x - Goal.transform.localPosition.x, 2);
            ZPow = Math.Pow(Points[j].transform.localPosition.z - Goal.transform.localPosition.z, 2);
            dis = Math.Sqrt(XPow + ZPow);
            if (dis < minDistance)
            {
                minDistance = dis;
                indexOfMinDistance = j;
            }
        }
        if (indexOfMinDistance != -1)
        {
            NearestGL = indexOfMinDistance;
        }
        for (int from = 0; from < Pointarray.GetLength(0); ++from)
        {
            for (int to = 0; to < Pointarray.GetLength(0); ++to)
            {
                if (from == to)
                {
                    Pointarray[from,to] = 0;
                    continue;
                }
                XPow = Math.Pow(Points[from].transform.localPosition.x - Points[to].transform.localPosition.x, 2);
                ZPow = Math.Pow(Points[from].transform.localPosition.z - Points[to].transform.localPosition.z, 2);
                dis = Math.Sqrt(XPow + ZPow);
                if (dis <= 2.0)
                {
                    Pointarray[from, to] = 1;
                }
                else
                {
                    Pointarray[from, to] = int.MaxValue - 10000;
                }
            }
        }
        result = dijkstra(NearestSP, NearestGL, ref Pointarray);
        result.Insert(0, new Vector3(StartPoint.transform.position.x, 0.5f, StartPoint.transform.position.z));
        result.Add(new Vector3(Goal.transform.position.x, 0.5f, Goal.transform.position.z));

        return result;
    }
    List<Vector3> dijkstra(int s, int t,ref int[,] Pointarray)
    {
        List<Vector3> result = new List<Vector3>();
        List<int> visit = new List<int>();
        int[] previous = new int[Points.Length];
        int[] dist = new int[Points.Length];
        bool[] saw = new bool[Points.Length];
        for (int i = 0; i < Points.Length; ++i) dist[i] = int.MaxValue - 10000;
        for (int i = 0;i < Points.Length; ++i) saw[i] = false;
        dist[s] = 0;
        saw[s] = true;
        previous[s] = s;
        visit.Add(s);
        while(visit.Count > 0)
        {
            int NowPlace = visit[0];
            visit.RemoveAt(0);
            for(int i = 0; i < Points.Length; ++i)
            {
                if (dist[i] > dist[NowPlace] + Pointarray[NowPlace, i]) 
                {
                    dist[i] = dist[NowPlace] + Pointarray[NowPlace, i];
                    previous[i] = NowPlace;
                    visit.Add(i);
                }
            }
        }
        while (t != s)
        {
            result.Insert(0, new Vector3(Points[t].transform.position.x, 0.5f, Points[t].transform.position.z));
            t = previous[t];
        }
        result.Insert(0, new Vector3(Points[s].transform.position.x, 0.5f, Points[s].transform.position.z));
        foreach(GameObject obj in Points)
        {
            Destroy(obj);
        }
        Array.Clear(Points, 0, Points.Length);
        return result;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
