using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    public Vector3 dest;
    public Vector3 finalScale;

    public float duration;
    private float size;

    // Start is called before the first frame update
    void Start()
    {
        size = transform.localScale.z / 2;
        if (finalScale == null)
            finalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
