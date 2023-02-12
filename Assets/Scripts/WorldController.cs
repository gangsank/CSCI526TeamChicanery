using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WorldController : MonoBehaviour
{
    public GameObject environment;
    public LayerMask platform;
    public bool isRotating = false;
    public float rotationDuration = 1f;
    private GameObject currentGround;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (!environment)
        {
            environment = GameObject.FindWithTag("World");
        }

    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.transform.up, out hit, 20, platform))
        {
            player.transform.GetChild(2).GetComponent<MeshRenderer>().material = hit.transform.gameObject.GetComponent<MeshRenderer>().material;
        }

        if (!isRotating && Physics.Raycast(player.transform.position, -player.transform.up, out hit, 5, platform))
        {
            currentGround = hit.transform.gameObject;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var values = AllowRotate(hit);

        if (values != null)
        {
            isRotating = true;
            player.GetComponent<FirstPersonController>().enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<TrailRenderer>().emitting = false;

            StartCoroutine(RotateWorld(values.Value.axis, values.Value.angle, hit.gameObject, hit.gameObject.transform.InverseTransformPoint(hit.point)));
        }
    }

    private (Vector3 axis, float angle)? AllowRotate(ControllerColliderHit hit)
    {
        int layer = 1 << hit.gameObject.layer;
        bool hitUnderGround = hit.gameObject.transform.up.y >= 0.95;

        bool hitGround = (layer & platform) > 0;
        bool colorMatched = currentGround.GetComponent<MeshRenderer>().material.color == hit.gameObject.GetComponent<MeshRenderer>().material.color;

        if (colorMatched && hitGround && !hitUnderGround && !isRotating)
        {
            Vector3 axis = Vector3.Cross(hit.gameObject.transform.up, Vector3.up).normalized;
            float angle = Mathf.Round(Vector3.Angle(Vector3.up, hit.gameObject.transform.up)) * player.GetComponent<FirstPersonController>().gravityDirection;
            if (axis != Vector3.zero)
            {

                return (axis, angle < 0 ? -180 - angle : angle);
            }
        }
        return null;
    }

    private IEnumerator RotateWorld(Vector3 axis, float angle, GameObject wall, Vector3 local)
    {
        if (axis == Vector3.right)
        {
            Destroy(currentGround.transform.parent);
        }

        for (float t = 0; t < rotationDuration; t += Time.deltaTime)
        {
            environment.transform.Rotate(axis, angle * Time.deltaTime / rotationDuration, Space.World);
            player.transform.position = wall.transform.TransformPoint(local);
            yield return null;
        }

        environment.transform.eulerAngles = new Vector3(
            Mathf.Round(environment.transform.eulerAngles.x / angle) * angle,
            Mathf.Round(environment.transform.eulerAngles.y / angle) * angle,
            Mathf.Round(environment.transform.eulerAngles.z / angle) * angle
        );

        player.transform.position = wall.transform.TransformPoint(local);
        player.GetComponent<FirstPersonController>().CancelJump();

        isRotating = false;
        player.GetComponent<FirstPersonController>().enabled = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<TrailRenderer>().emitting = true;
    }

}
