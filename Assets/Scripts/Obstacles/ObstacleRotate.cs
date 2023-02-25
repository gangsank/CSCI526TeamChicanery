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

    protected override void Start()
    {
        base.Start();
        startAngle = transform.rotation;
        endAngle = Quaternion.Euler(dest);
        target = endAngle;

        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate() {
        while (true)
        {
            Quaternion start = transform.rotation;
            for (float t = 0; t < rotationTime; t += Time.deltaTime)
            {
                transform.rotation = Quaternion.Slerp(start, target, t / rotationTime);
                yield return null;
            }
            transform.rotation = target;
            target = target == startAngle ? endAngle : startAngle;
            yield return new WaitForSeconds(stopTime);
        }
    }
}
