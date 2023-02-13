using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Weapon : MonoBehaviour
{
    public int damage = 1;
    public int respawn; 
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      
      // collision handle here
    }
    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player")){
            PlayerStart.currentHealth -= damage;
             if(PlayerStart.currentHealth == 0){
                PlayerStart.currentHealth = 4;
                SceneManager.LoadScene(respawn);
             }
        }
       
    }

    // void TakeDamage(int damage) {
    //     currentHealth -= damage;
    // }


}
