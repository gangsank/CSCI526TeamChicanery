using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed, jump;
    private float move;
    private Rigidbody2D rb;
    public float amplitude = 0.5f;
    public float frequency = 1f;
    Vector2 posOffset = new Vector2();
    Vector2 tempPos = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        posOffset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(move * speed, rb.velocity.y);

        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(new Vector2(rb.velocity.x, jump));
        }

        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.UpArrow))
        {
            tempPos = posOffset;
            tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
            transform.position = tempPos;
        }
    }
}
