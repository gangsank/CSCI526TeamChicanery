using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WorldController : MonoBehaviour
{
    public bool ColorMatch = true;
    public GameObject environment;
    public LayerMask platform;
    public bool isRotating = false;
    public float rotationDuration = 0.5f;
    private GameObject currentGround;

    private GameObject player;
    private bool shouldReset = false;
    private GameObject bottomWall;
    private GameObject leftWall;
    private GameObject topWall;
    private GameObject rightWall;
    private GameObject preBottomWall;
    private GameObject preLeftWall;
    private GameObject preTopWall;
    private GameObject preRightWall;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (!environment)
        {
            environment = GameObject.FindWithTag("World");
        }

        bottomWall = GameObject.Find("0");
        leftWall = GameObject.Find("1");
        topWall = GameObject.Find("2");
        rightWall = GameObject.Find("3");
        preBottomWall = GameObject.Find("Pre0");
        preLeftWall = GameObject.Find("Pre1");
        preTopWall = GameObject.Find("Pre2");
        preRightWall = GameObject.Find("Pre3");
    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.transform.up, out hit, 40, platform))
        {
            player.transform.GetChild(2).GetComponent<MeshRenderer>().material = hit.transform.gameObject.GetComponent<MeshRenderer>().material;
        }

        if (!isRotating && Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 5, platform))
        {
            currentGround = hit.transform.gameObject;
        }

        // generating walls endlessly
        if (player.transform.position.z > bottomWall.transform.position.z)
        {
            //bottom
            var tempBottomWall = preBottomWall;
            preBottomWall = bottomWall;
            tempBottomWall.transform.position += new Vector3(0, 0, 2000);
            bottomWall = tempBottomWall;

            //left
            var tempLeftWall = preLeftWall;
            preLeftWall = leftWall;
            tempLeftWall.transform.position += new Vector3(0, 0, 2000);
            leftWall = tempLeftWall;

            //top
            var tempTopWall = preTopWall;
            preTopWall = topWall;
            tempTopWall.transform.position += new Vector3(0, 0, 2000);
            topWall = tempTopWall;

            //right
            var tempRightWall = preRightWall;
            preRightWall = rightWall;
            tempRightWall.transform.position += new Vector3(0, 0, 2000);
            rightWall = tempRightWall;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var values = AllowRotate(hit);

        if (values != null)
        {
            StartCoroutine(RotateWorld(values.Value.axis, values.Value.angle, hit.gameObject, hit.gameObject.transform.InverseTransformPoint(hit.point)));
        }
    }

    private (Vector3 axis, float angle)? AllowRotate(ControllerColliderHit hit)
    {
        int layer = 1 << hit.gameObject.layer;
        bool hitUnderGround = hit.gameObject.transform.up.y >= 0.95;

        bool hitGround = (layer & platform) > 0;
        bool colorMatched = ColorMatch ? currentGround.GetComponent<MeshRenderer>().material.color == hit.gameObject.GetComponent<MeshRenderer>().material.color : true;

        if (!shouldReset && colorMatched && hitGround && !hitUnderGround && !isRotating)
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
        isRotating = true;
        player.GetComponent<FirstPersonController>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<TrailRenderer>().emitting = false;

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

        player.GetComponent<FirstPersonController>().enabled = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<TrailRenderer>().emitting = true;

        yield return new WaitForSeconds(0.3f);
        isRotating = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Config.Tag.Reset))
        {
            shouldReset = true;
            RaycastHit hit;
            if (
                environment.transform.eulerAngles.z != 0 &&
                Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 5, platform))
            {
                Vector3 axis = Vector3.Cross(hit.transform.up, Vector3.up).normalized;
                float angle = environment.transform.eulerAngles.z;
                player.GetComponent<FirstPersonController>().gravityDirection = 1;
                StartCoroutine(RotateWorld(axis, angle, hit.transform.gameObject, hit.transform.InverseTransformPoint(hit.point)));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Config.Tag.Reset))
        {
            shouldReset = false;
        }
    }
}
