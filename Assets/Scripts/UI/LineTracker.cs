using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LineTracker : MonoBehaviour
{

    [SerializeField]
    private SpriteRenderer m_spriteRender;
    [SerializeField]
    private float thickness = 1f;

    void Update()
    {
        if (GameManager.Singleton != null && GameManager.Singleton.HasStarted)
        {
            Player p = GameManager.GetPlayer();
            if (p == null || !p.IsOwner)
                return;
            Player assignment = p.Assignment;
            if (assignment == null)
                return;

            Vector3 eulers = Quaternion.LookRotation(p.transform.forward, assignment.transform.forward).eulerAngles;
            eulers.x = 90;
            transform.rotation = Quaternion.Euler(eulers);

            float length = Vector3.Distance(assignment.transform.position, p.transform.position);
            m_spriteRender.size = new Vector2(length, thickness);
            Vector3 vec = p.transform.position + p.transform.forward * (length / 2);
            vec.y = .3f;
            transform.position = vec;
        }
    }
}
