using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int numCoins;

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        player.GetComponent<FirstPersonController>().triggerEnter += HandleCoinCollect;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleCoinCollect(Collider other) {
        if (other.gameObject.CompareTag(Config.Tag.Item))
        {
            Destroy(other.gameObject);
            numCoins += 1;
        }
    }
}
