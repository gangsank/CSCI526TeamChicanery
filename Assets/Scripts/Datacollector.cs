using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;

public class Datacollector : MonoBehaviour
{
    

    public static string collisionName;
    private string prevCollisionName;
    public static string collisionPoint;
    public static int playerId;
    private System.Random random = new System.Random();
    // Start is called before the first frame update

    void Start()
    {
        playerId = random.Next(0, 1001);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.collider.tag != "Floor&Wall")
        {
            Debug.Log(hit.collider.name);
            collisionName = hit.collider.name;
            collisionPoint = hit.point.ToString();
            Debug.Log(hit.point);
            if(collisionName != prevCollisionName)
            {
                PostToDatabase();
            }
            prevCollisionName = collisionName;
            
        }

        
    }

    

    private void PostToDatabase()
    {
        User user = new User();
        RestClient.Post("https://rotatetest-d8bfc-default-rtdb.firebaseio.com/.json", user);

    }

    
}
