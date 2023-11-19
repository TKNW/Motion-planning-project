using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem.XR;

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
        PreDistance = CountDistance(this.gameObject, Goal);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Distance);
        sensor.AddObservation(Goal.transform.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation.eulerAngles.y);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Time penalty
        if(GetCumulativeReward() >= -1.0f)
            AddReward(-0.0005f);

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
        Distance = CountDistance(this.gameObject,Goal);
        if(Distance < 1.5f)
        {
            SetReward(1.0f);
            Regenerate = true;
            EndEpisode();
        }
        if (Distance < PreDistance && GetCumulativeReward() <= 0.5f)
        {
            //Debug.Log("Dis = " + Distance + " Pre = " + PreDistance);
            AddReward(0.01f);
        }
        PreDistance = Distance;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W) == true)discreteActionsOut[0] = (int)Move.Straight;
        else if (Input.GetKey(KeyCode.A) == true) discreteActionsOut[0] = (int)Move.TurnLeft;
        else if (Input.GetKey(KeyCode.D) == true) discreteActionsOut[0] =(int)Move.TurnRight;
        else discreteActionsOut[0] = (int)Move.Stop;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            AddReward(-0.5f);
            Regenerate = false;
            EndEpisode();
        }
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        //不做事，這裡只是因為用的動畫會傳這個訊息，接一下而已
    }
}
