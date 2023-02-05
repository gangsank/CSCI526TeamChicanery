using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerAttackManager : MonoBehaviour
{
    private GameObject player;
	private StarterAssetsInputs _input;
    private GameObject weapon;

    public bool attacking = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
		_input = player.GetComponent<StarterAssetsInputs>();
        weapon = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Attack();
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains("Bullet")) return;
        if (attacking)
        {
            Destroy(other.gameObject);
            player.GetComponent<FirstPersonController>().DoubleJump();
        }
    }

    private void Attack()
    {
        if (!attacking && _input.attack)
        {
            _input.attack = false;
            attacking = true;
            StartCoroutine(AttackGenerator());
        }
    }

    private IEnumerator AttackGenerator()
    {
        float curTime = 0;
        float accAngle = 0;
        while (curTime < 0.3)
        {
            curTime += Time.deltaTime;
            float curAngle = Mathf.Min(180 * curTime / 0.3f, 180f);
            weapon.transform.Rotate(Vector3.right, curAngle - accAngle);
            accAngle = curAngle;
            yield return null;
        }

        while (curTime < 0.2)
        {
            curTime += Time.deltaTime;
            var rotation = Vector3.Lerp(Vector3.zero, new Vector3(180, 0, 0), curTime / 0.2f);
            weapon.transform.localRotation = Quaternion.Euler(rotation);
            yield return null;
        }
        attacking = false;
    }
}
