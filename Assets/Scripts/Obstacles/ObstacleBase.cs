using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    protected GameObject player;
    protected Vector3 size;
    protected float destroyOffset = 15;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        size = transform.localScale;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        DetectUselessness();
    }

    protected void DetectUselessness()
    {
        if (player.transform.position.z >= transform.position.z + size.z / 2 + destroyOffset)
            Destroy(gameObject);
    }
}
