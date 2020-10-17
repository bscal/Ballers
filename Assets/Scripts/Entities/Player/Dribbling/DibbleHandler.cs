using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  Was an attempt at doing dribbling with mouse movement. 
 *  It works ok needs tweaking and little features.
 *  Going to change to wasd and arrow keys 
 */

public enum MouseDirection
{
    NONE,
    NORTH,
    SOUTH,
    EAST,
    NORTH_EAST,
    SOUTH_EAST,
    WEST,
    NORTH_WEST,
    SOUTH_WEST
}

public class DibbleHandler : MonoBehaviour
{

    private static readonly int WIDTH_MID = Screen.width / 2;
    private static readonly int HEIGHT_MID = Screen.height / 2;
    private static readonly Vector3 CENTER = new Vector3((float) WIDTH_MID, (float) HEIGHT_MID, 0f);

    private const float DEADZONE = 2.0f;
    private const float OFFSET = 45.0f;

    private Player m_player;

    private float[] m_quadrants = new float[] { 0f, 0f, 0f, 0f};
    int direction = 0;
    private float xAxis = 0f;
    private float yAxis = 0f;

    private void Awake()
    {
        m_player = GetComponent<Player>();
    }

    void Start()
    {
        StartCoroutine(MousePosition());
    }


    private IEnumerator MousePosition()
    {
        while (true)
        {
            if (enabled)
            {
                int current = 0;

                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                if (mouseX == 0 && mouseY == 0)
                {
                    direction = 0;
                    xAxis = 0f;
                    yAxis = 0f;
                }
                else
                {
                    xAxis += mouseX;
                    yAxis += mouseY;
                }

                Vector3 mouse = Input.mousePosition;

                if (mouse == null)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                ResetQuadrants(0.0f);

                float dist = Vector3.Distance(CENTER, mouse);

                if (mouse.x > WIDTH_MID)
                {
                    current += 1;
                }
                if (mouse.y < HEIGHT_MID)
                {
                    current += 2;
                }

                if (yAxis > DEADZONE)
                {
                    print("up");
                    direction += 1;
                }
                else if (yAxis < -DEADZONE)
                {
                    print("down");
                    direction += 2;
                }

                if (xAxis > DEADZONE)
                {
                    print("right");
                    direction += 3;
                }
                else if (xAxis < -DEADZONE)
                {
                    print("left");
                    direction += 6;
                }

                print(dist);
                print(current);
                print(direction);
                print(" ");
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ResetQuadrants(float val)
    {
        for (int i = 0; i < m_quadrants.Length; i++)
        {
            m_quadrants[i] = val;
        }
    }
}
