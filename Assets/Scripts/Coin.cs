using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float RotateSpeed = 180;
    public int value = 1;

    private GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, RotateSpeed * Time.deltaTime);
        Recycle();
    }

    void Recycle()
    {
        if (player.transform.position.z > transform.position.z + 10)
            Destroy(gameObject);
    }

}
