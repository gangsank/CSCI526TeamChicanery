using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBackground : MonoBehaviour
{
    Camera cam;
    private GameObject player;

    private float currentH = 0f;
    public float baseColorSpeed = 60f;
    public float expectedPlayerSpeed = 20f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        cam = GetComponent<Camera>();
        currentH = Random.Range(0, 360);
    }

    // Update is called once per frame
    void Update()
    {
        float factor = player.GetComponent<FirstPersonController>().ForwardSpeed / expectedPlayerSpeed;
        float speed = baseColorSpeed * factor;

        currentH += Time.deltaTime * speed;
        currentH %= 360f;
        cam.backgroundColor = Color.HSVToRGB(currentH / 360f, 0.2f, 1f);
    }
}
