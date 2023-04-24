using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleRotate : ObstacleBase
{
    public List<Vector3> stops;
    //public Vector3 dest;

    //private Quaternion endAngle;
    //private Quaternion startAngle;
    //private Quaternion target;
    private List<Quaternion> angles = new List<Quaternion>();
    private int index = 0;


    public float rotationTime = 1f;
    public float stopTime = 1;

    public bool useTrigger = false;
    public float triggerRange = 100;
    private IEnumerator rotateAction;


    protected override void Start()
    {
        base.Start();
        foreach (var angle in stops)
            angles.Add(Quaternion.Euler(angle));
        angles.Add(transform.rotation);

        //startAngle = transform.rotation;
        //endAngle = Quaternion.Euler(dest);
        //target = endAngle;

        if (!useTrigger)
        {
            rotateAction = Rotate();
            StartCoroutine(rotateAction);
        }
    }


    protected override void Update()
    {
        if (useTrigger && rotateAction == null && (transform.position.z - player.transform.position.z) - (size.z / 2) < triggerRange)
        {
            rotateAction = Rotate();
            StartCoroutine(Rotate());
        }
    }

    private IEnumerator Rotate() {
        WorldController controller = player.GetComponent<WorldController>();
        while (true)
        {
            Quaternion start = transform.localRotation;
            Quaternion target = angles[index];

            for (float t = 0; t < rotationTime; t += controller.isRotating ? 0 : Time.deltaTime)
            {
                transform.localRotation = Quaternion.Slerp(start, target, t / rotationTime);
                yield return null;
            }
            transform.localRotation = target;
            index = (index + 1) % angles.Count;
            //target = target == startAngle ? endAngle : startAngle;

            for (float t = 0; t < stopTime; t += (controller.isRotating ? 0 : Time.deltaTime))
            {
                yield return null;
            }
        }
    }
}
