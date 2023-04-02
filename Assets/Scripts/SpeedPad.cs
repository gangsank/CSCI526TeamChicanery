using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPad : MonoBehaviour
{
    public static SpeedPad last;
    public float speedDelta = 6;
    public float burstSpeed = 4;
    public float burstDuration = 3;

    private float MinSpeed = 5;
    private IEnumerator action;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        var playerController = player.GetComponent<FirstPersonController>();
    }

    public void Stop()
    {
        if (action != null)
        {
            StopCoroutine(action);
            action = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (
            other.CompareTag(Config.Tag.Player) &&
            (last == null || last.transform.position.z != transform.position.z)
        )
        {
            if (last) last.Stop();
            last = this;
            
            action = burst();
            StartCoroutine(action);
        }
    }

    private IEnumerator burst()
    {
        var controller = player.GetComponent<FirstPersonController>();
        var wc = player.GetComponent<WorldController>();
        controller.MaxSpeed = Mathf.Max(controller.MaxSpeed + speedDelta, MinSpeed);
        controller.ForwardSpeed = Mathf.Max(controller.MaxSpeed + burstSpeed, MinSpeed);
        controller.CrossSpeed = controller.MaxSpeed;

        for (float t = 0; t < burstDuration; t += wc.isRotating ? 0 : Time.deltaTime)
        {
            yield return null;
        }
        controller.ForwardSpeed = controller.MaxSpeed;
        action = null;
    }
}
