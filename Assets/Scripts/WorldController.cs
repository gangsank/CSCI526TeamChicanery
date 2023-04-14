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
    public float anglePerSecond = 150f;
    private GameObject currentGround;

    private GameObject player;
    private bool shouldReset = false;
    public int numRotate = 0;

    // Start is called before the first frame update

    private void Awake()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        environment = GameObject.FindWithTag(Config.Tag.World);
    }

    private void Start()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 5, platform))
        {
            currentGround = hit.transform.gameObject;
        }
    }

    private void Update()
    {
        if (!player.GetComponent<Gravity>().Grounded)
        {
            currentGround = null;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var values = AllowRotate(hit);
        //Debug.Log(values);
        if (values != null)
        {
            StartCoroutine(RotateWorld(values.Value.axis, values.Value.angle, hit.gameObject, hit.gameObject.transform.InverseTransformPoint(hit.point)));
        }
    }

    private (Vector3 axis, float angle)? AllowRotate(ControllerColliderHit hit)
    {
        if (player.GetComponent<CharacterController>().velocity.z == 0) return null;

        int layer = 1 << hit.gameObject.layer;
        bool hitUnderGround = Mathf.Abs(hit.gameObject.transform.up.y) >= 0.95;
        bool hitGround = (layer & platform) > 0;

        if (hitUnderGround)
            currentGround = hit.gameObject;
        if (hitUnderGround || !hitGround) return null; 
          
        bool colorMatched = true;
        if (ColorMatch && currentGround != null && currentGround.GetComponent<MeshRenderer>().material.color != hit.gameObject.GetComponent<MeshRenderer>().material.color)
        {
            colorMatched = false;
        }
        else
        {
            currentGround = hit.gameObject;
        }

        if (!shouldReset && colorMatched && !isRotating)
        {
            Vector3 axis = Vector3.Cross(hit.gameObject.transform.up, Vector3.up).normalized;
            float angle = Mathf.Round(Vector3.Angle(Vector3.up, hit.gameObject.transform.up)) * player.GetComponent<Gravity>().direction;
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
        yield return new WaitForSeconds(0.05f);
        if (player.GetComponent<CharacterController>().velocity.z == 0)
        {
            isRotating = false;
            yield break;
        }

        player.GetComponent<FirstPersonController>().enabled = false;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<TrailRenderer>().emitting = false;
        float finalEulerZ = environment.transform.eulerAngles.z + axis.z * angle;

        float duration = Mathf.Abs(angle) / anglePerSecond;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            environment.transform.Rotate(axis, angle * Time.deltaTime / duration, Space.World);
            player.transform.position = wall.transform.TransformPoint(local);
            yield return null;
        }

        environment.transform.eulerAngles = new Vector3(0, 0, finalEulerZ);

        player.transform.position = wall.transform.TransformPoint(local);
        player.GetComponent<FirstPersonController>().CancelJump();

        player.GetComponent<FirstPersonController>().enabled = true;
        player.GetComponent<CharacterController>().enabled = true;
        player.GetComponent<TrailRenderer>().emitting = true;

        numRotate += 1;

        yield return new WaitForSeconds(0.1f);
        isRotating = false;
    }

    public void SetRotation(Quaternion rotation)
    {
        environment.transform.rotation = rotation;
    }

    public Quaternion GetRotation()
    {
        return environment.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Config.Tag.Reset) && !shouldReset)
        {
            shouldReset = true;
            RaycastHit hit;
            if (
                environment.transform.eulerAngles.z != 0 &&
                Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 5, platform))
            {
                Vector3 axis = Vector3.Cross(hit.transform.up, Vector3.up).normalized;
                float angle = environment.transform.eulerAngles.z;
                //player.GetComponent<FirstPersonController>().gravityDirection = 1;
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
