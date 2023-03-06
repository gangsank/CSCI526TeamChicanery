using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gravity))]
public class ObstacleGravity : ObstacleBase
{
    private Gravity g;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        g = GetComponent<Gravity>();
        g.GroundedOffset = -g.direction * transform.localScale.y / 2;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        Reverse();
        ApplyGravity();
    }

    private void ApplyGravity()
    {

        if (!g.GroundedCheck())
        {
            transform.Translate(Vector3.up * Time.deltaTime * g.velocity);
        }
    }

    private void Reverse()
    {
        var input = player.GetComponent<StarterAssetsInputs>();
        if (input.primaryAction) {
            g.Reverse();
            g.GroundedOffset = -g.direction * transform.localScale.y / 2;
        }
    }
}
