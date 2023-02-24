using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMove : ObstacleBase
{
    public Vector3 direction;
    public float distance;
    public float duration;
    public float triggerRange = 50;

    private Vector3 dest;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        dest = transform.position;
        transform.position = transform.position - distance * direction.normalized;
        player = GameObject.FindWithTag(Config.Tag.Player);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if ((transform.position.z - player.transform.position.z) - (size.z / 2) < triggerRange)
        {
            StartCoroutine(MoveToDest());
        }
    }

    private IEnumerator MoveToDest()
    {
        Vector3 start = transform.position;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, dest, t / duration);
            yield return null;
        }
        transform.position = dest;
    }

}
