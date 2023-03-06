using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleRotate : ObstacleBase
{
    public Vector3 dest;

    private Quaternion endAngle;
    private Quaternion startAngle;
    private Quaternion target;


    public float rotationTime = 1f;
    public float stopTime = 1;

    public bool useTrigger = false;
    public float triggerRange = 100;
    private IEnumerator rotateAction;

    protected override void Start()
    {
        base.Start();
        startAngle = transform.rotation;
        endAngle = Quaternion.Euler(dest);
        target = endAngle;

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
            for (float t = 0; t < rotationTime; t += controller.isRotating ? 0 : Time.deltaTime)
            {
                transform.localRotation = Quaternion.Slerp(start, target, t / rotationTime);
                yield return null;
            }
            transform.localRotation = target;
            target = target == startAngle ? endAngle : startAngle;

            for (float t = 0; t < stopTime; t += (controller.isRotating ? 0 : Time.deltaTime))
            {
                yield return null;
            }
        }
    }
}
