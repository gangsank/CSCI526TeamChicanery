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
    }

    // Update is called once per frame
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
    }
}
