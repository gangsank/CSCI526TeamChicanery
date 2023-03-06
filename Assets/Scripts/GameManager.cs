using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Proyecto26;
using UnityEngine.InputSystem.XR;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;

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
    public int numCeilingCoins = 0;
    public int hp = 4;
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
    private int activate_shield = 10;

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
        healthBar.value = hp;
        healthBar.maxValue = hp;
        

        if (goal == null) goal = GameObject.FindWithTag(Config.Tag.Goal);
        if (gameoverMenu != null) gameoverMenu?.SetActive(false);
        if (goal != null) goal.GetComponent<End>().triggerEnter += GameOver;

        SaveData();
        Invoke("DisableInvincible", 1);
        Resume();

    }


    // Update is called once per frame
    void Update()
    {
        var input = player.GetComponent<StarterAssetsInputs>();
        Save();
        CharacterController controller = player.GetComponent<CharacterController>();

        if (input.menu)
        {
            input.menu = false;
            if (Time.timeScale == 0)
                Resume();
            else
                Pause();
        }


        if ( stopped == 0 && (player.transform.position.y <= -50 || player.transform.position.y >= 50) && !playerInvincible)
        {
            GameOver();
        }

        if (stopped == 0 && (controller.velocity.z <= 0.1) && !playerInvincible)
        {
            playerInvincible = true;
            StartCoroutine(DamagePlayer());
        }

        // Prevent controller.velocity.z is too low when time starts to go
        if (stopped > 0 && Time.timeScale != 0)
        {
            stopped--;
        }
        showshield();
    }

    void Pause()
    {
        Time.timeScale = 0;
        stopped = 5;
        if (menu != null)
        {
            menu.Show();
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
                numCoins += coin.value;
                coinsText.text = $"{numCoins}";
                if(player.GetComponent<Gravity>().direction == -1)
                numCeilingCoins += 1;
            }

            
        }
    }

    private IEnumerator DamagePlayer()
    {
        
        LoadSaveData();
        
        if (numCoins >= activate_shield ){
            numCoins -= activate_shield;
            coinsText.text = $"{numCoins}";
        }
        else{
            hp -= 1;
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

        curTime += Time.deltaTime;

        if (
            cc.velocity.z > initialPlayerSpeed &&
            curTime - saveData.time >= 3 &&
            player.GetComponent<Gravity>().Grounded &&
            !Physics.Raycast(player.transform.position + player.transform.up, player.transform.forward, 5) &&
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
        var follow = player.GetComponent<FirstPersonController>().vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        saveData = new PlayerSave(
            player.transform.position,
            player.GetComponent<Gravity>().direction > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, -180),
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

        player.GetComponent<FirstPersonController>().CinemachineCameraTarget.transform.eulerAngles = Vector3.zero;
        saveData.time = curTime;
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
        Debug.Log("SendData");
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

    private void showshield(){
         GameObject shield= player.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject;
        if (numCoins>=activate_shield){
            //show shield
            //Debug.Log(numCoins);
            shield.transform.localScale = new Vector3(4, 1.2f, 3);
        }
        else{
            Debug.Log("less than 5");
            // player.GetComponentInChildren<Material>().color = Color.red;
            // Debug.Log(player.GetComponentInChildren<Material>().color);
           
            shield.transform.localScale = new Vector3(0,0,0);
            //shield.GetComponent<Transform>();
           
        }
       
    }
}
