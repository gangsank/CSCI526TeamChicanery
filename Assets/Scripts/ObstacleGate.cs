using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGate : MonoBehaviour
{
    public GameObject left;
    public GameObject right;

    public float OpenSpeed = 5f;
    public float CloseSpeed = 5f;
    public float MaxGap = 20;
    public float MinGap = 10;

    private GameObject player;
    private float leftWidth;
    private float rightWidth;
    private bool opening;

    private IEnumerator action;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        leftWidth = left.transform.localScale.x;
        rightWidth = right.transform.localScale.x;
        float gap = (right.transform.position.x - rightWidth / 2) - (left.transform.position.x + leftWidth / 2);
        opening = gap < MaxGap;
    }

    // Update is called once per frame
    void Update()
    {
        if (action == null && Vector3.Distance(transform.position, player.transform.position) < 100)
        {
            action = StartMove();
            StartCoroutine(StartMove());
        }
        else if (action != null && Vector3.Distance(transform.position, player.transform.position) > 150)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator StartMove()
    {
        while (MaxGap != MinGap)
        {
            Vector3 dist = new Vector3((opening ? MaxGap : MinGap) / 2, 0, 0);
            float distDelta = Time.deltaTime * (opening ? OpenSpeed : CloseSpeed);

            Vector3 leftPos = left.transform.position;
            left.transform.position = Vector3.MoveTowards(leftPos, new Vector3(transform.position.x - leftWidth / 2, leftPos.y, leftPos.z) - dist, distDelta);
            Vector3 rightPos = right.transform.position;
            right.transform.position = Vector3.MoveTowards(rightPos, new Vector3(transform.position.x + rightWidth / 2, rightPos.y, rightPos.z) + dist, distDelta);

            float gap = (right.transform.position.x - rightWidth / 2) - (left.transform.position.x + leftWidth / 2);
            if (gap >= MaxGap) opening = false;
            else if (gap <= MinGap) opening = true;

            yield return null;
        }
    }
}
