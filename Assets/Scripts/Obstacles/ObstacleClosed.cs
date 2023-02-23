using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleClosed : MonoBehaviour
{
    public GameObject gate;
    public Material material;
    public float gateHeight = 13f;
    public float openTime = 2f;

    private GameObject player;
    private bool isOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        gate.GetComponent<MeshRenderer>().material = material;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOpen && Vector3.Distance(transform.position, player.transform.position) < 70)
        {
            StartCoroutine(OpenGate());
        }
    }

    private IEnumerator OpenGate()
    {
        isOpen = true;
        for (float i = 0; i < openTime; i += Time.deltaTime)
        {
            gate.transform.Translate(Vector3.up * (gateHeight * Time.deltaTime / openTime));
            yield return null;
        }
    }
}
