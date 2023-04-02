using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Proyecto26;
using Cinemachine;

struct PlayerSave
{
    public Vector3 playerPos;
    public Quaternion playerRotation;
    public Vector3 playerSpeed; // Max, Forward, Cross
    public float gravityDirection;
    public Quaternion worldRotation;
    public float camerePosY;
    public float time;

    public PlayerSave(Vector3 _pos, Quaternion _rot, Vector3 _speed, float gDir, Quaternion _wRot, float _cY, float t)
    {
        playerPos = _pos;
        playerRotation = _rot;
        playerSpeed = _speed;
        gravityDirection = gDir;
        worldRotation = _wRot;
        camerePosY = _cY;
        time = t;
    }
}

public class GameManager : MonoBehaviour
{
    public int numCoins = 0;
    public int numCeilingCoins = 0;
    public int hp = 100;
    public int initialPlayerSpeed = 5;

    [SerializeField] private GameObject player;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject goal; // use for midtern
    [SerializeField] private GameObject gameoverMenu;

    private GameMenu menu;
    private PlayerSave saveData;
    private bool playerInvincible = true;
    private float curTime = 0;
    private int stopped = 0;
    private bool gameEnded = false;
    private int activate_shield = 25;
    private bool shieldOn = false;
    readonly private int MaxHP = 100;

    // Start is called before the first frame update
    void Start()
    {
        menu = GameObject.FindWithTag(Config.Tag.GameMenu)?.GetComponent<GameMenu>();
        if (menu != null)
        {
            menu.back.onClick.RemoveAllListeners();
            menu.back.onClick.AddListener(Resume);
        }
        
        player = GameObject.FindWithTag(Config.Tag.Player);
        player.GetComponent<FirstPersonController>().triggerEnter += HandleCoinCollect;
        hp = MaxHP;
        healthBar.value = hp;
        healthBar.maxValue = hp;

        if (goal == null) goal = GameObject.FindWithTag(Config.Tag.Goal);
        if (gameoverMenu != null) gameoverMenu?.SetActive(false);
        if (goal != null) goal.GetComponent<End>().triggerEnter += GameClear;

        SaveData();
        Invoke("DisableInvincible", 1);
        Resume();

    }


    // Update is called once per frame
    void Update()
    {
        Save();
        HandleMenu();
        HandleFall();
        HandleHitObstacle();
        ShowShield();

        // Prevent controller.velocity.z is too low when time starts to go
        if (stopped > 0 && Time.timeScale != 0)
        {
            stopped--;
        }
    }

    void HandleMenu()
    {
        var input = player.GetComponent<StarterAssetsInputs>();
        if (input.menu)
        {
            input.menu = false;
            if (!gameEnded)
            {
                if (Time.timeScale == 0)
                    Resume();
                else
                    Pause();
            }
        }
    }

    void HandleFall()
    {
        if (stopped == 0 && (player.transform.position.y <= -50 || player.transform.position.y >= 50))
        {
            GameOver();
        }
    }

    void HandleHitObstacle()
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (stopped == 0 && (controller.velocity.z <= 0.1) && !playerInvincible)
        {
            playerInvincible = true;
            StartCoroutine(DamagePlayer());
        }
    }

    void Pause(string message = "Paused")
    {
        Time.timeScale = 0;
        stopped = 5;
        if (menu != null)
        {
            menu.Show(message);
        }
    }

    void Resume()
    {
        Time.timeScale = 1;
        if (menu != null)
        {
            menu.Hide();
        }
    }

    private void HandleCoinCollect(Collider other) {
        
        if (other.gameObject.CompareTag(Config.Tag.Item))
        {
            Destroy(other.gameObject);
            Coin coin;
            if (other.TryGetComponent<Coin>(out coin)) {
                if(player.GetComponent<Gravity>().direction == -1)
                numCeilingCoins += 1;
                if (hp < MaxHP)
                {
                    hp += 1;
                    healthBar.value = hp;
                }
                else
                {
                    numCoins += coin.value;
                }

                if (hp >= MaxHP && numCoins >= activate_shield && !shieldOn)
                {
                    numCoins -= activate_shield;
                    shieldOn = true;
                }
                coinsText.text = $"{numCoins}";
            }
        }
    }

    private IEnumerator DamagePlayer()
    {
        
        LoadSaveData();
        
        if (shieldOn){
            shieldOn = false;
        }
        else{
            hp -= 25;
            numCoins = 0;
            coinsText.text = $"{numCoins}";
        }

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
            SpeedPad.last = null;
            //player.GetComponent<FirstPersonController>().ForwardSpeed = initialPlayerSpeed;
            //player.GetComponent<FirstPersonController>().CrossSpeed = initialPlayerSpeed;
            //player.GetComponent<FirstPersonController>().SpeedUp();
            player.GetComponent<FirstPersonController>().CancelJump();
        }
        yield return new WaitForSeconds(2);
        playerInvincible = false;

    }

    private void Save()
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        WorldController wc = player.GetComponent<WorldController>();
        FirstPersonController pc = player.GetComponent<FirstPersonController>();

        curTime += Time.deltaTime;

        if (
            cc.velocity.z > initialPlayerSpeed &&
            curTime - saveData.time >= 3 &&
            player.GetComponent<Gravity>().Grounded &&
            !Physics.Raycast(player.transform.position + player.transform.up, player.transform.forward, 0.8f * pc.ForwardSpeed) &&
            (player.transform.localRotation.z == 0 || player.transform.localRotation.z == 1)
        )
        {
            RaycastHit hit;
            if (Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 1.1f, wc.platform) && Vector3.Cross(hit.transform.up, transform.up).magnitude < 1E-6) {
                SaveData();
            }
        }

    }

    private void SaveData()
    {
        var pc = player.GetComponent<FirstPersonController>();
        var follow = pc.vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        saveData = new PlayerSave(
            player.transform.position,
            player.GetComponent<Gravity>().direction > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, -180),
            new Vector3(pc.MaxSpeed, pc.ForwardSpeed, pc.CrossSpeed),
            player.GetComponent<Gravity>().direction,
            player.GetComponent<WorldController>().GetRotation(),
            follow.ShoulderOffset.y,
            curTime
        );
        Debug.Log($"Save: {saveData.playerPos}/{saveData.playerRotation.eulerAngles}/{saveData.gravityDirection}/{saveData.worldRotation.eulerAngles}/");
    }

    private void LoadSaveData()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<FirstPersonController>().enabled = false;
        player.GetComponent<TrailRenderer>().Clear();
        player.GetComponent<Gravity>().Stop();

        var follow = player.GetComponent<FirstPersonController>().vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        Debug.Log($"Load: {saveData.playerPos}/{saveData.playerRotation.eulerAngles}/{saveData.gravityDirection}/{saveData.worldRotation.eulerAngles}/");

        player.transform.position = saveData.playerPos;
        player.transform.rotation = saveData.playerRotation;
        player.GetComponent<CharacterController>().enabled = true;

        player.GetComponent<Gravity>().direction = saveData.gravityDirection;
        player.GetComponent<Gravity>().velocity = player.GetComponent<Gravity>().force * -saveData.gravityDirection;
        player.GetComponent<WorldController>().SetRotation(saveData.worldRotation);
        follow.ShoulderOffset.y = saveData.camerePosY;

        var pc = player.GetComponent<FirstPersonController>();
        pc.MaxSpeed = saveData.playerSpeed.x;
        pc.ForwardSpeed = saveData.playerSpeed.y;
        pc.CrossSpeed = saveData.playerSpeed.z;
        pc.CinemachineCameraTarget.transform.eulerAngles = Vector3.zero;
        saveData.time = curTime;
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        gameEnded = true;
        
        if (menu != null)
            Pause("YOU ARE DEAD");
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GameClear()
    {
        Time.timeScale = 0;
        gameEnded = true;
        SendData();
        if (menu != null)
        {
            Pause($"{SceneManager.GetActiveScene().name.Replace("Course", "Stage ")} Cleared");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void SendData()
    {
        RestClient.Post<User>("https://rotatetest-d8bfc-default-rtdb.firebaseio.com/.json", new User
        {

            userId = Datacollector.playerId,
            numCoins = this.numCoins,
            numCeilCoins = this.numCeilingCoins,
            numOfRotate = player.GetComponent<WorldController>().numRotate,
            endHp = this.hp,
            scene = SceneManager.GetActiveScene().name
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

    private void ShowShield(){
         GameObject shield= player.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;
        if (shieldOn){
            //show shield
            //Debug.Log(numCoins);
            shield.transform.localScale = new Vector3(4, 1.2f, 3);
        }
        else{
            //Debug.Log("less than 5");
            // player.GetComponentInChildren<Material>().color = Color.red;
            // Debug.Log(player.GetComponentInChildren<Material>().color);
           
            shield.transform.localScale = new Vector3(0,0,0);
            //shield.GetComponent<Transform>();
           
        }
       
    }
}
