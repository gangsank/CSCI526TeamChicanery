using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Proyecto26;
using System;

public class Datacollector : MonoBehaviour
{
    public static int playerId = 0;

    private string collisionName;
    private string prevCollisionName;
    private string collisionPoint;

    private System.Random random = new System.Random();

    private void Awake()
    {
        if (Datacollector.playerId == 0)
            Datacollector.playerId = random.Next(1, 1001);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (
            hit.gameObject.layer != Config.Layer.Ground &&
            hit.gameObject.layer != Config.Layer.Boundary
        ) {
            collisionName = getFullName(hit.gameObject);
            collisionPoint = hit.point.ToString();
            if (collisionName != prevCollisionName)
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
            userId = playerId,
            scene = SceneManager.GetActiveScene().name
        });
    }

    private string getFullName(GameObject obj)
    {
        List<string> path = new List<string>();
        while (obj != null && obj.name != "World" && obj.name != "Pivot") {
            path.Add(obj.name);
            obj = obj.transform.parent.gameObject;
        }

        path.Reverse();
        return string.Join(" - ", path);
    }
}

[Serializable]
public class User
{
    public string userCollision;
    public string userCollisionPoint;
    public int userId;
    public int numCoins;
    public int numCeilCoins;
    public int endHp;
    public int numOfRotate;
    public string scene;
    //public User()
    //{
    //    userCollision = Datacollector.collisionName;
    //    userCollisionPoint = Datacollector.collisionPoint;
    //    userId = Datacollector.playerId;

    //}
    
}
