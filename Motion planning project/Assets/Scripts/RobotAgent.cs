using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem.XR;
using UnityEditor.Rendering.LookDev;

public class RobotAgent : Agent
{
    enum Move
    {
        Stop,
        TurnLeft,
        TurnRight,
        Straight
    }
    public GameObject Goal = null;
    public GameObject EnvManager = null;
    public float MoveSpeed = 1.0f;

    private CharacterController CController = null;
    private Animator Ani = null;
    private int RunaniID = 0;

    private float Distance = 0.0f;
    private float PreDistance = 0.0f;
    private float Angle = 0.0f;
    private bool Regenerate = true;
    
    private float CountDistance(GameObject obj1, GameObject obj2) 
    {
        if (obj1 == null || obj2 == null)
            return float.MaxValue;
        float result = 0.0f;
        double XPow = Math.Pow(obj1.transform.localPosition.x - obj2.transform.localPosition.x, 2);
        double ZPow = Math.Pow(obj1.transform.localPosition.z - obj2.transform.localPosition.z, 2);
        result = (float)Math.Sqrt(XPow + ZPow);
        return result;
    }
    private float CountAngle(GameObject obj1, GameObject obj2)
    {
        if (obj1 == null || obj2 == null)
            return float.MaxValue;
        float result = 0.0f;
        float rad = obj1.transform.localRotation.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 vec1 = obj2.transform.localPosition - obj1.transform.localPosition;
        Vector3 vec2 = new Vector3(Mathf.Sin(rad),
                                   0,
                                   Mathf.Cos(rad));
        result = Vector3.Angle(vec1, vec2);
        return result;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (CController == null) 
        { 
            CController = GetComponent<CharacterController>();
        }
        if (Ani == null) 
        {
            Ani = GetComponent<Animator>();
        }
        RunaniID = Animator.StringToHash("Run");
    }

    public override void OnEpisodeBegin()
    {
        EnvManager.SendMessage("EnvReset", Regenerate);
        Regenerate = false;
        EnvManager.SendMessage("SetResetPlayer", true);
        EnvManager.SendMessage("SetMoveGoal", false);
        PreDistance = CountDistance(this.gameObject, Goal);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(Distance);
        sensor.AddObservation(Angle);
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(Goal.transform.localPosition.x);
        sensor.AddObservation(Goal.transform.localPosition.z);
        sensor.AddObservation(Goal.transform.localPosition - transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Time penalty
        AddReward(-0.002f);

        int movement = actionBuffers.DiscreteActions[0];
        if (movement == (int)Move.TurnLeft)
        {
            Ani.SetBool(RunaniID, false);
            transform.Rotate(new Vector3(0, -1, 0));
        }
        if (movement == (int)Move.TurnRight)
        {
            Ani.SetBool(RunaniID, false);
            transform.Rotate(new Vector3(0, 1, 0));
        }
        if (movement == (int)Move.Straight)
        {
            float rad = transform.localRotation.eulerAngles.y * Mathf.Deg2Rad;
            Ani.SetBool(RunaniID, true);
            CController.Move(new Vector3(Mathf.Sin(rad) * MoveSpeed, 
                                         0, 
                                         Mathf.Cos(rad) * MoveSpeed));
        }
        if(movement == (int)Move.Stop)
        {
            Ani.SetBool(RunaniID, false);
        }
        Distance = CountDistance(this.gameObject, Goal);
        if(Distance < 1.4f)
        {
            AddReward(10.0f);
            Regenerate = true;
            Debug.Log(GetCumulativeReward());
            EnvManager.SendMessage("SetResetPlayer", false);
            EnvManager.SendMessage("SetMoveGoal", true);
            EnvManager.SendMessage("SetStartPoint", this.gameObject.transform);
            EndEpisode();
        }
        Angle = CountAngle(this.gameObject, Goal);
        if (Distance < PreDistance
            && Angle <= 45)
        {
            //Debug.Log("Dis = " + Distance + " Pre = " + PreDistance);
            AddReward(0.01f);
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W) == true)discreteActionsOut[0] = (int)Move.Straight;
        else if (Input.GetKey(KeyCode.A) == true) discreteActionsOut[0] = (int)Move.TurnLeft;
        else if (Input.GetKey(KeyCode.D) == true) discreteActionsOut[0] =(int)Move.TurnRight;
        else discreteActionsOut[0] = (int)Move.Stop;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            AddReward(-0.3f);
            //Regenerate = false;
            //EndEpisode();
        }
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        //不做事，這裡只是因為用的動畫會傳這個訊息，接一下而已
    }
}
