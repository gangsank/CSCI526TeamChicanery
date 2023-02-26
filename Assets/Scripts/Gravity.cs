using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Gravity : MonoBehaviour
{
    [Tooltip("If the GameObject is grounded or not.")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;


    public float force = -9.8f;
    public float direction = 1;
    private IEnumerator reverseAction;
    public float velocity = 0;
    private float _terminalVelocity = 53.0f;

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();

        if (Grounded) // jump 
        {
            velocity = direction > 0 ? Mathf.Max(-3f, velocity) : Mathf.Min(3f, velocity);
        }

        ApplyGravity();
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    public void Reverse()
    {
        direction = -direction;

        if (reverseAction != null)
            StopCoroutine(reverseAction);
        reverseAction = ReverseCharacterAction(direction == 1 ? 0 : 180);
        StartCoroutine(reverseAction);
    }

    private IEnumerator ReverseCharacterAction(float angle)
    {
        yield return new WaitForSeconds(0.05f);

        while (transform.eulerAngles.z != angle)
        {
            transform.eulerAngles = Vector3.MoveTowards(
                transform.eulerAngles,
                new Vector3(transform.rotation.eulerAngles.x, transform.rotation.y, angle),
                Time.deltaTime * 480
            );
            yield return null;
        }
        reverseAction = null;
    }

    public void ApplyGravity()
    {
        if (velocity < _terminalVelocity)
        {
            velocity += force * Time.deltaTime * direction;
        }
    }

    public void AddForce(float velocity)
    {
        this.velocity += Time.deltaTime * force * direction * -0.5f;
    }

    public bool HasGroundUp() {
        return Physics.Raycast(transform.position, transform.up, 50, GroundLayers);
    }

}
