using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Proyecto26;

public class GameManager : MonoBehaviour
{
    public int numCoins = 0;
    public int hp = 4;
    public int initialPlayerSpeed = 5;

    [SerializeField] private GameObject player;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject goal; // use for midtern
    [SerializeField] private GameObject gameoverMenu;

    private float lastSaveTime;
    private Vector3 lastSavepoint;
    private bool playerInvincible = true;


    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        player = GameObject.FindWithTag(Config.Tag.Player);
        player.GetComponent<FirstPersonController>().triggerEnter += HandleCoinCollect;
        healthBar.value = hp;
        healthBar.maxValue = hp;
        lastSavepoint = player.transform.position;

        if (goal == null) goal = GameObject.FindWithTag(Config.Tag.Goal);
        if (gameoverMenu != null) gameoverMenu?.SetActive(false);
        if (goal != null) goal.GetComponent<End>().triggerEnter += GameOver;
        Invoke("DisableInvincible", 1);
    }


    // Update is called once per frame
    void Update()
    {
        Save();
        CharacterController controller = player.GetComponent<CharacterController>();
        if ((player.transform.position.y <= -30 || player.transform.position.y >= 30 || controller.velocity.z <= 0.1 ) && !playerInvincible)
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
        player.transform.position = lastSavepoint;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<FirstPersonController>().enabled = false;
        
        hp -= 1;
        healthBar.value = hp;
        if (hp <= 0)
        {
            GameOver();
        }
        else
        {
            yield return new WaitForSeconds(2);
            player.GetComponent<CharacterController>().enabled = true;
            player.GetComponent<FirstPersonController>().enabled = true;
            player.GetComponent<FirstPersonController>().ForwardSpeed = initialPlayerSpeed;
            player.GetComponent<FirstPersonController>().CrossSpeed = initialPlayerSpeed;
            player.GetComponent<FirstPersonController>().SpeedUp();
            player.GetComponent<FirstPersonController>().CancelJump();
        }
        yield return new WaitForSeconds(2);
        playerInvincible = false;

    }

    private void Save()
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller.velocity.z > 3 && Time.realtimeSinceStartup - lastSaveTime >= 4 && player.GetComponent<Gravity>().Grounded)
        {
            lastSaveTime = Time.realtimeSinceStartup;
            lastSavepoint = player.transform.position;
        }

    }

    private void GameOver()
    {
        Time.timeScale = 0;
        if(hp > -1)
        {
            SendData();
        }
        if (gameoverMenu != null)
            gameoverMenu?.SetActive(true);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Cursor.lockState = CursorLockMode.None;
    }

    private void SendData()
    {
        RestClient.Post<User>("https://rotatetest-d8bfc-default-rtdb.firebaseio.com/.json", new User
        {

            userId = Datacollector.playerId,
            numCoins = this.numCoins,
            endHp = this.hp
        });
    }

    private void DisableInvincible()
    {
        playerInvincible = false;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
