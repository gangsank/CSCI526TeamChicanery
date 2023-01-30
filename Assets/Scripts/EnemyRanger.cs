using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyRanger : MonoBehaviour
{
    public GameObject bullet;
    public int nBullets = 1;
    public float CoolDown = 2f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Shoot", CoolDown, CoolDown);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Shoot()
    {
        GameObject inst = Instantiate<GameObject>(bullet, transform.transform.position, transform.rotation, transform.parent);
        inst.transform.Translate(0, 0.4f, 0);
        inst.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 100f);
    }
}
