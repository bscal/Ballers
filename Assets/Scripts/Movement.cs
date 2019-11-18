using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private float m_movementSpeed = 12.0f;
    private float m_turningSpeed = 200.0f;

    public float Horizontal { get; set; }
    public float Vertical { get; set; }

    // Update is called once per frame
    void Update()
    {
        Horizontal = Input.GetAxis("Horizontal") * m_turningSpeed * Time.deltaTime;
        transform.Rotate(0, Horizontal, 0);

        Vertical = Input.GetAxis("Vertical") * m_movementSpeed * Time.deltaTime;
        transform.Translate(0, 0, -Vertical);
    }

}
