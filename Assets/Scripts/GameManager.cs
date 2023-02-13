using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int numCoins = 0;
    public int hp = 4;

    [SerializeField] private GameObject player;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject end;
    [SerializeField] private GameObject gameoverMenu;

    private bool playerInvincible = true;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        player = GameObject.FindWithTag(Config.Tag.Player);
        player.GetComponent<FirstPersonController>().triggerEnter += HandleCoinCollect;
        healthBar.value = hp;
        healthBar.maxValue = hp;
        gameoverMenu?.SetActive(false);

        end.GetComponent<End>().triggerEnter += GameOver;
        Invoke("DisableInvincible", 1);
    }


    // Update is called once per frame
    void Update()
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller.velocity.z <= 3 && !playerInvincible)
        {
            StartCoroutine(DamagePlayer());
        }
    }


    private void HandleCoinCollect(Collider other) {
        if (other.gameObject.CompareTag(Config.Tag.Item))
        {
            Destroy(other.gameObject);
            numCoins += 1;
            coinsText.text = $"{numCoins}";
        }
    }

    private IEnumerator DamagePlayer()
    {
        playerInvincible = true;
        player.GetComponent<CharacterController>().Move(-20 * player.transform.forward);
        player.GetComponent<FirstPersonController>().ForwardSpeed = 5;
        hp -= 1;
        healthBar.value = hp;
        if (hp <= 0)
        {
            GameOver();
        }
        else
        {
            yield return new WaitForSeconds(2);
        }
        playerInvincible = false;

    }

    private void GameOver()
    {
        Time.timeScale = 0;
        gameoverMenu?.SetActive(true);
    }


    private void DisableInvincible()
    {
        playerInvincible = false;
    }
}
