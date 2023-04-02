using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPad : MonoBehaviour
{
    static IEnumerator action;
    public float speedDelta = 6;
    public float burstSpeed = 4;
    public float burstDuration = 3;

    private GameObject player;
    private float reuseCountdown = 0;

    private void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        var playerController = player.GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        if (reuseCountdown > 0) reuseCountdown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Config.Tag.Player) && reuseCountdown <= 0)
        {
            if (action != null) StopCoroutine(action);
            reuseCountdown = 2;
            action = burst();
            StartCoroutine(action);
        }
        //Debug.Log(other.name);
    }

    private IEnumerator burst()
    {
        var controller = player.GetComponent<FirstPersonController>();
        controller.MaxSpeed += speedDelta;
        controller.ForwardSpeed = controller.ForwardSpeed + speedDelta + burstSpeed;
        controller.CrossSpeed = controller.CrossSpeed + speedDelta;

        for (float t = 0; t < burstDuration; t += Time.deltaTime) yield return null;
        controller.ForwardSpeed = controller.MaxSpeed;
        action = null;
    }
}
