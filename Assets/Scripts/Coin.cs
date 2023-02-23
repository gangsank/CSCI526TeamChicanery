using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float RotateSpeed = 180;
    private bool startRotate = false;

    private void Start()
    {
        Invoke("Rotate", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (startRotate)
            transform.Rotate(transform.up, RotateSpeed * Time.deltaTime, Space.Self);
    }

    private void Rotate()
    {
        startRotate = true;
    }

        private void OnTriggerEnter(Collider other)
    {
    }
}
