using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class User
{
    public string userCollision;
    public string userCollisionPoint;
    public int userId;

    public User()
    {
        userCollision = Datacollector.collisionName;
        userCollisionPoint = Datacollector.collisionPoint;
        userId = Datacollector.playerId;

    }
    
}
