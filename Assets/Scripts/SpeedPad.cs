using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPad : MonoBehaviour
{
    static SpeedPad last;
    public float speedDelta = 6;
    public float burstSpeed = 4;
    public float burstDuration = 3;

    private IEnumerator action;
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
        if (other.CompareTag(Config.Tag.Player) && reuseCountdown <= 0)
        {
            if (last) last.Stop();
            last = this;
            
            reuseCountdown = 2;
            action = burst();
            StartCoroutine(action);
        }
    }

    private IEnumerator burst()
    {
        var controller = player.GetComponent<FirstPersonController>();
        var wc = player.GetComponent<WorldController>();
        controller.MaxSpeed += speedDelta;
        controller.ForwardSpeed = controller.MaxSpeed + burstSpeed;
        controller.CrossSpeed = controller.MaxSpeed;

        for (float t = 0; t < burstDuration; t += wc.isRotating ? 0 : Time.deltaTime)
        {
            yield return null;
        }
        controller.ForwardSpeed = controller.MaxSpeed;
        action = null;
    }
}
