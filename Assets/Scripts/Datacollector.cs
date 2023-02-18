using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;

public class Datacollector : MonoBehaviour
{
    

    public string collisionName;
    private string prevCollisionName;
    public string collisionPoint;
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
            collisionName = hit.collider.name;
            collisionPoint = hit.point.ToString();
            if(collisionName != prevCollisionName)
            {
                PostToDatabase();
            }
            prevCollisionName = collisionName;
            
        }

        
    }

    

    private void PostToDatabase()
    {
        //User user = new User();
        //RestClient.Post("https://rotatetest-d8bfc-default-rtdb.firebaseio.com/.json", user);
        RestClient.Post<User>("https://rotatetest-d8bfc-default-rtdb.firebaseio.com/.json", new User
        {
            userCollision = collisionName,
            userCollisionPoint = collisionPoint,
            userId = playerId
        });

    }

    
}
