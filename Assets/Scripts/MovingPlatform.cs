using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : _Object
{
    public float speed = 1f;
    public Vector3 cyclePoint1, cyclePoint2;

    Vector3 destination;
    bool cycle;

    protected override void Awake()
    {
        base.Awake();
        
        destination = cyclePoint1;
        cycle = true;
    }

    protected override void Update()
    {
        if(Vector3.Distance(destination, transform.position) < 0.05f)
        {
            ChangeCycle();
        }

        transform.position = Vector3.MoveTowards(gameObject.transform.position, destination, Time.deltaTime * speed);
    }

    void ChangeCycle()
    {
        if(cycle)
        {
            destination = cyclePoint2;
            cycle = false;
        }
        else
        {
            destination = cyclePoint1;
            cycle = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cyclePoint1, cyclePoint2);
        Gizmos.DrawSphere(cyclePoint1, 0.05f);
        Gizmos.DrawSphere(cyclePoint2, 0.05f);
    }
}
