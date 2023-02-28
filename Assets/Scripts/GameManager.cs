using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Proyecto26;
using UnityEngine.InputSystem.XR;
using Cinemachine;

struct PlayerSave
{
    public Vector3 playerPos;
    public Quaternion playerRotation;
    public float gravityDirection;
    public Quaternion worldRotation;
    public float camerePosY;
    public float time;

    public PlayerSave(Vector3 _pos, Quaternion _rot, float gDir, Quaternion _wRot, float _cY, float t)
    {
        playerPos = _pos;
        playerRotation = _rot;
        gravityDirection = gDir;
        worldRotation = _wRot;
        camerePosY = _cY;
        time = t;
    }
}

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

    private PlayerSave saveData;
    private bool playerInvincible = true;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        player = GameObject.FindWithTag(Config.Tag.Player);
        player.GetComponent<FirstPersonController>().triggerEnter += HandleCoinCollect;
        healthBar.value = hp;
        healthBar.maxValue = hp;
        

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
        if ((player.transform.position.y <= -30 || player.transform.position.y >= 30 || controller.velocity.z <= 0.1) && !playerInvincible)
        {
            playerInvincible = true;
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
        
        LoadSaveData();
        

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
            player.GetComponent<WorldController>().enabled = true;
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
        CharacterController cc = player.GetComponent<CharacterController>();
        WorldController wc = player.GetComponent<WorldController>();

        RaycastHit hit;
        if (
            cc.velocity.z > 0 &&
            Time.realtimeSinceStartup - saveData.time >= 3 &&
            player.GetComponent<Gravity>().Grounded &&
            (player.transform.localRotation.z == 0 || player.transform.localRotation.z == 1) 
        )
        {
            if (Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 1.1f, wc.platform) && Vector3.Cross(hit.transform.up, transform.up).magnitude < 1E-6) {
                var follow = player.GetComponent<FirstPersonController>().vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                saveData = new PlayerSave(
                    player.transform.position,
                    player.GetComponent<Gravity>().direction > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, -180),
                    player.GetComponent<Gravity>().direction,
                    player.GetComponent<WorldController>().GetRotation(),
                    follow.ShoulderOffset.y,
                    Time.realtimeSinceStartup
                );
                Debug.Log($"Save: {saveData.playerPos}/{saveData.playerRotation.eulerAngles}/{saveData.gravityDirection}/{saveData.worldRotation.eulerAngles}/");
            }
        }

    }

    private void LoadSaveData()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<FirstPersonController>().enabled = false;
        player.GetComponent<TrailRenderer>().Clear();

        var follow = player.GetComponent<FirstPersonController>().vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        Debug.Log($"Load: {saveData.playerPos}/{saveData.playerRotation.eulerAngles}/{saveData.gravityDirection}/{saveData.worldRotation.eulerAngles}/");

        player.transform.position = saveData.playerPos;
        player.transform.rotation = saveData.playerRotation;
        player.GetComponent<Gravity>().direction = saveData.gravityDirection;
        player.GetComponent<Gravity>().velocity = player.GetComponent<Gravity>().force * -saveData.gravityDirection;
        player.GetComponent<WorldController>().SetRotation(saveData.worldRotation);
        follow.ShoulderOffset.y = saveData.camerePosY;

        player.GetComponent<FirstPersonController>().CinemachineCameraTarget.transform.eulerAngles = Vector3.zero;
        saveData.time = Time.realtimeSinceStartup;
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
