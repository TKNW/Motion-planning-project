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
        EnvManager.SendMessage("Reset");
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
        double XPow = Math.Pow(Goal.transform.localPosition.x - transform.localPosition.x, 2);
        double ZPow = Math.Pow(Goal.transform.localPosition.z - transform.localPosition.z, 2);
        Distance = (float)Math.Sqrt(XPow + ZPow);
        if(Distance < 1.5f)
        {
            SetReward(1.0f);
            EndEpisode();
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            //AddReward(-0.5f);
            EndEpisode();
        }
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        //不做事，這裡只是因為用的動畫會傳這個訊息，接一下而已
    }
}
