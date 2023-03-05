using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGate : ObstacleBase
{
    public GameObject left;
    public GameObject right;

    public float OpenSpeed = 5f;
    public float CloseSpeed = 5f;
    public float MaxGap = 20;
    public float MinGap = 10;

    private float leftWidth;
    private float rightWidth;
    private bool opening;

    private IEnumerator action;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        leftWidth = left.transform.localScale.x;
        rightWidth = right.transform.localScale.x;
        float gap = Vector3.Distance(left.transform.position, right.transform.position) - leftWidth / 2 - rightWidth / 2;
        opening = gap < MaxGap;
        StartCoroutine(StartMove());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (action == null && Vector3.Distance(transform.position, player.transform.position) < 100)
        {
            action = StartMove();
            StartCoroutine(action);
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
            Vector3 dist = (opening ? MaxGap : MinGap) * transform.right;
            float distDelta = (opening ? OpenSpeed : CloseSpeed);

            Vector3 leftPos = left.transform.position;
            left.transform.Translate(distDelta * (opening ? Vector3.left : Vector3.right) * Time.deltaTime);
            right.transform.Translate(distDelta * (opening ? Vector3.right : Vector3.left) * Time.deltaTime);

            float gap = Vector3.Distance(left.transform.position, right.transform.position) - leftWidth / 2 - rightWidth / 2;

            if (gap >= MaxGap) opening = false;
            else if (gap <= MinGap) opening = true;

            yield return null;
        }
    }
}
