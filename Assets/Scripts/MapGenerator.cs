using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private GameObject player;
    private GameObject bottomWall;
    private GameObject leftWall;
    private GameObject topWall;
    private GameObject rightWall;
    private GameObject preBottomWall;
    private GameObject preLeftWall;
    private GameObject preTopWall;
    private GameObject preRightWall;
    public GameObject wall1;
    public GameObject wall2;
    public GameObject wall3;
    public GameObject wall4;
    public GameObject jumpWall1;
    public GameObject jumpWall2;
    public GameObject blockWall1;
    public GameObject blockWall2;
    public GameObject blockWall3;
    
    private GameObject obstacle1,
        obstacle2,
        obstacle3,
        obstacle4,
        obstacle5,
        obstacle6,
        obstacle7,
        obstacle8,
        obstacle9;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag(Config.Tag.Player);
        bottomWall = GameObject.Find("0");
        leftWall = GameObject.Find("1");
        topWall = GameObject.Find("2");
        rightWall = GameObject.Find("3");
        preBottomWall = GameObject.Find("Pre0");
        preLeftWall = GameObject.Find("Pre1");
        preTopWall = GameObject.Find("Pre2");
        preRightWall = GameObject.Find("Pre3");

        // obstacle1 = GenerateObstacle(wall1, -460.0f);
        // obstacle2 = GenerateObstacle(wall2, -440.0f);
        // obstacle3 = GenerateObstacle(wall3, -420.0f);
        // obstacle4 = GenerateObstacle(wall4, -400.0f);
        // obstacle5 = GenerateObstacle(jumpWall1, -380.0f);
        // obstacle6 = GenerateObstacle(jumpWall2, -360.0f);
        // obstacle7 = GenerateObstacle(blockWall1, -340.0f);
        // obstacle8 = GenerateObstacle(blockWall2, -320.0f);
        // obstacle9 = GenerateObstacle(blockWall3, -300.0f);
    }
    
    GameObject GenerateObstacle(GameObject obstaclePrefab, float referenceZ)
    {
        GameObject obstacle = GameObject.Instantiate(obstaclePrefab);
        obstacle.transform.position =
            new Vector3(obstaclePrefab.transform.position.x, obstaclePrefab.transform.position.y, 
                referenceZ + 180.0f);  
    
        return obstacle;
    }
    
    void Update()
    {
        // generating walls endlessly
        if (player.transform.position.z > bottomWall.transform.position.z)
        {
            //bottom
            var tempBottomWall = preBottomWall;
            preBottomWall = bottomWall;
            tempBottomWall.transform.position += new Vector3(0, 0, 2000);
            bottomWall = tempBottomWall;

            //left
            var tempLeftWall = preLeftWall;
            preLeftWall = leftWall;
            tempLeftWall.transform.position += new Vector3(0, 0, 2000);
            leftWall = tempLeftWall;

            //top
            var tempTopWall = preTopWall;
            preTopWall = topWall;
            tempTopWall.transform.position += new Vector3(0, 0, 2000);
            topWall = tempTopWall;

            //right
            var tempRightWall = preRightWall;
            preRightWall = rightWall;
            tempRightWall.transform.position += new Vector3(0, 0, 2000);
            rightWall = tempRightWall;
        }
        
        // generating obstacles endlessly
        //generating obstacles endlessly
        // if (player.transform.position.z > obstacle1.transform.position.z)
        // {
        //     obstacle1.transform.position = new Vector3(obstacle1.transform.position.x, obstacle1.transform.position.y,
        //         obstacle1.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle2.transform.position.z)
        // {
        //     obstacle2.transform.position = new Vector3(obstacle2.transform.position.x, obstacle2.transform.position.y,
        //         obstacle2.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle3.transform.position.z)
        // {
        //     obstacle3.transform.position = new Vector3(obstacle3.transform.position.x, obstacle3.transform.position.y,
        //         obstacle3.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle4.transform.position.z)
        // {
        //     obstacle4.transform.position = new Vector3(obstacle4.transform.position.x , obstacle4.transform.position.y,
        //             obstacle4.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle5.transform.position.z)
        // {
        //     obstacle5.transform.position = new Vector3(obstacle5.transform.position.x, obstacle5.transform.position.y,
        //         obstacle5.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle6.transform.position.z)
        // {
        //     obstacle6.transform.position = new Vector3(obstacle6.transform.position.x, obstacle6.transform.position.y,
        //         obstacle6.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle7.transform.position.z)
        // {
        //     obstacle7.transform.position = new Vector3(obstacle7.transform.position.x, obstacle7.transform.position.y,
        //         obstacle7.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle8.transform.position.z)
        // {
        //     obstacle8.transform.position = new Vector3(obstacle8.transform.position.x, obstacle8.transform.position.y,
        //         obstacle8.transform.position.z + 180.0f);
        // }
        // else if (player.transform.position.z > obstacle9.transform.position.z)
        // {
        //     obstacle9.transform.position = new Vector3(obstacle9.transform.position.x, obstacle9.transform.position.y,
        //         obstacle9.transform.position.z + 180.0f);
        // }
    }
}
