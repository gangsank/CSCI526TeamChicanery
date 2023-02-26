using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMove : ObstacleBase
{
    public Vector3 direction;
    public float distance;
    public float duration = 1;
    public float triggerRange = 50;
    public bool repeat = false;
    public float stopTime = 2;

    private Vector3 dest;
    private Vector3 start;
    private Vector3 target;
    private IEnumerator moveAction;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        dest = transform.position;
        start = transform.position - distance * direction.normalized;
        transform.position = start;
        target = dest;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (moveAction == null && (transform.position.z - player.transform.position.z) - (size.z / 2) < triggerRange)
        {
            moveAction = MoveToDest();
            StartCoroutine(moveAction);
        }
    }

    private IEnumerator MoveToDest()
    {
        WorldController controller = player.GetComponent<WorldController>();
        do
        {
            Vector3 start = transform.position;
            for (float t = 0; t < duration; t += controller.isRotating ? 0 : Time.deltaTime)
            {
                transform.position = Vector3.Lerp(start, dest, t / duration);
                yield return null;
            }
            transform.position = dest;

            target = target == start ? dest : start;
            for (float t = 0; t < stopTime; t += (controller.isRotating ? 0 : Time.deltaTime))
            {
                yield return null;
            }
        } while (repeat);
    }

}
