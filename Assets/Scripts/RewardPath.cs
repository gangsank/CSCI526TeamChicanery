using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPath : MonoBehaviour
{
    public GameObject rewardPrefab;
    public float interval = 2f;

    private float sizeFactor = 10;
    private float depth;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;

        depth = transform.localScale.z * sizeFactor;
        Instantiate(rewardPrefab, transform.position + transform.up, transform.rotation, transform.parent);
        for (float i = interval; i <= Mathf.Floor(depth / 2); i += interval)
        {
            
            Instantiate(rewardPrefab, transform.position + transform.up + transform.forward * i, transform.rotation, transform.parent);
            Instantiate(rewardPrefab, transform.position + transform.up - transform.forward * i, transform.rotation, transform.parent);
        }

        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
