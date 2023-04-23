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
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;
    [SerializeField] private GameObject goal; // use for midtern
    [SerializeField] private GameObject gameoverMenu;


    private GameMenu menu;
    private PlayerSave saveData;
    private bool playerInvincible = true;
    private float curTime = 0;
    private int stopped = 0;
    private bool gameEnded = false;
    private int activate_shield = 10;
    private bool shieldOn = false;
    readonly private int MaxHP = 100;
    private float startSpeed;

    private ParticleSystem healingParticle;
    private IEnumerator healingAnimation;

    public AudioSource crashSFX;
    public AudioSource coinSFX;


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
        startSpeed = player.GetComponent<FirstPersonController>().ForwardSpeed;
        hp = MaxHP;
        healthBar.value = hp;
        healthBar.maxValue = hp;
        shieldBar.value = 0;
        shieldBar.maxValue = 10;

        var audios = player.GetComponentsInChildren<AudioSource>();
        foreach (var audio in audios)
        {
            if (audio.gameObject.name == "CrashAudio")
                crashSFX = audio;
            else if (audio.gameObject.name == "CoinAudio")
                coinSFX = audio;
        }

        healingParticle = player.GetComponentsInChildren<ParticleSystem>()[0];
        healingParticle.Stop();

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
            Vector3 spherePosition = player.transform.position + player.transform.up + 0.5f * Vector3.forward;
            if (Physics.CheckSphere(spherePosition, 0.1f, damageLayer, QueryTriggerInteraction.Ignore))
            {
                playerInvincible = true;
                StartCoroutine(DamagePlayer());
            }
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

    private void HandleCoinCollect(Collider other)
    {

        if (other.gameObject.CompareTag(Config.Tag.Item))
        {
            coinSFX.Play();
            Destroy(other.gameObject);
            Coin coin;
            if (other.TryGetComponent<Coin>(out coin))
            {
                if (player.GetComponent<Gravity>().direction == -1)
                    numCeilingCoins += 1;
                if (hp < MaxHP)
                {
                    Heal();
                }
                else if (!shieldOn)
                {
                    numCoins += coin.value;
                    shieldBar.value = numCoins;
                }

                if (hp >= MaxHP && numCoins == activate_shield && !shieldOn)
                {
                    shieldOn = true;
                }
            }
        }
    }

    private IEnumerator DamagePlayer()
    {

        LoadSaveData();
        crashSFX.Play();
        healingParticle.Stop();
        healingAnimation = null;

        if (shieldOn)
        {
            shieldOn = false;
        }
        else
        {
            hp -= 25;
        }

        numCoins = 0;
        shieldBar.value = numCoins;

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
            (player.transform.localRotation.z == 0 || player.transform.localRotation.z == 1)
        )
        {
            RaycastHit hit;
            if (Physics.Raycast(player.transform.position + player.transform.up, -player.transform.up, out hit, 1.1f, wc.platform) && Vector3.Cross(hit.transform.up, transform.up).magnitude < 1E-6)
            {
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
        //Debug.Log($"Load: {saveData.playerPos}/{saveData.playerRotation.eulerAngles}/{saveData.gravityDirection}/{saveData.worldRotation.eulerAngles}/");

        RaycastHit forward;
        Physics.Raycast(saveData.playerPos, player.transform.forward, out forward, saveData.playerSpeed.y, damageLayer);

        if (
            !Physics.Raycast(saveData.playerPos, player.transform.forward + player.transform.up, out forward, saveData.playerSpeed.y, damageLayer) ||
            forward.distance >= 0.8 * saveData.playerSpeed.y
        )
        {
            player.transform.position = saveData.playerPos;
        }
        else
        {
            float complementDistance = 0.8f * saveData.playerSpeed.y - forward.distance;
            RaycastHit backward;
            Debug.Log("Too close to front");
            if (
                !Physics.Raycast(saveData.playerPos, -player.transform.forward, out backward, saveData.playerSpeed.y, damageLayer) ||
                (backward.distance - 10) >= complementDistance
            )
            {
                player.transform.position = saveData.playerPos + Vector3.back * complementDistance;
            }
            else
            {
                Debug.Log("Too close to back");

                float usableDistance = Mathf.Max(backward.distance - 10, 0);
                player.transform.position = saveData.playerPos + Vector3.back * usableDistance;
            }
        }



        player.transform.rotation = saveData.playerRotation;
        player.GetComponent<CharacterController>().enabled = true;

        player.GetComponent<Gravity>().direction = saveData.gravityDirection;
        player.GetComponent<Gravity>().velocity = player.GetComponent<Gravity>().force * -saveData.gravityDirection;
        player.GetComponent<WorldController>().SetRotation(saveData.worldRotation);
        follow.ShoulderOffset.y = saveData.camerePosY;

        var pc = player.GetComponent<FirstPersonController>();
        pc.MaxSpeed = saveData.playerSpeed.x;
        pc.ForwardSpeed = Mathf.Max(startSpeed, saveData.playerSpeed.y * 0.9f);
        pc.CrossSpeed = Mathf.Max(startSpeed, saveData.playerSpeed.z * 0.9f);
        pc.CinemachineCameraTarget.transform.eulerAngles = Vector3.zero;
        saveData.time = curTime;
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        gameEnded = true;

        if (menu != null)
            Pause("Game Over");
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

    private void ShowShield()
    {
        GameObject shield = player.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;
        if (shieldOn)
        {
            shield.SetActive(true);
        }
        else
        {
            shield.SetActive(false);
        }

    }

    private void Heal()
    {
        hp += 2;
        healthBar.value = hp;

        if (healingAnimation == null)
        {
            healingParticle.Play();
            healingAnimation = HealAction();
            StartCoroutine(healingAnimation);
        }
    }

    private IEnumerator HealAction()
    {
        yield return new WaitForSeconds(2);
        healingParticle.Stop();
        healingAnimation = null;
    }
}
