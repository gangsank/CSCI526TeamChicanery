using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleClosed : ObstacleBase
{
    public GameObject gate;
    public Material material;
    public float gateHeight = 13f;
    public float openTime = 2f;

    private bool isOpen = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        gate.GetComponent<MeshRenderer>().material = material;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
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
