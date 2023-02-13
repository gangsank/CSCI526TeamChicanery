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

    private GameObject player;
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
        // slope slide won't trigger OnControllerColliderHit

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
        int layer = 1 << hit.gameObject.layer;
        bool hitUnderGround = hit.gameObject.transform.up.y >= 0.95;

        if ((layer & platform) > 0 && !hitUnderGround && !isRotating)
        {
            Vector3 axis = Vector3.Cross(hit.gameObject.transform.up, Vector3.up).normalized;
            float angle = Mathf.Round(Vector3.Angle(Vector3.up, hit.gameObject.transform.up));
            if (axis != Vector3.zero)
            {
                isRotating = true;
                player.GetComponent<FirstPersonController>().enabled = false;
                player.GetComponent<CharacterController>().enabled = false;
                StartCoroutine(RotateWorld(axis, angle, hit.gameObject, hit.gameObject.transform.InverseTransformPoint(hit.point)));
            }
        }
        
    }

    private IEnumerator RotateWorld(Vector3 axis, float angle, GameObject wall, Vector3 local)
    {
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
    }

}
