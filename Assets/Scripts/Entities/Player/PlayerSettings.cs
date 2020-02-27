using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ballers
{
    public class PlayerSettings : MonoBehaviour
    {

        private const int FOCUS_CAM = 0;
        private const int SIDE_CAM = 1;
        private const int FOLLOW_CAM = 2;
        private const int FREE_CAM = 3;

        public Camera follow;
        public Camera side;
        public Camera focus;
        public Camera free;

        private Dropdown m_dropdown;

        void Start()
        {
            m_dropdown = GameObject.Find("Camera Dropdown").GetComponent<Dropdown>();
            m_dropdown.onValueChanged.AddListener(delegate { OnCameraChanged(m_dropdown.value); });

            // Sets default camera
            OnCameraChanged(FOCUS_CAM);

            free.enabled = false;
        }

        void Update() { }

        public void OnCameraChanged(int id)
        {
            focus.enabled = false;
            follow.enabled = false;
            side.enabled = false;
            free.enabled = false;

            switch (id)
            {
                case FOCUS_CAM:
                    focus.enabled = true;
                    break;
                case SIDE_CAM:
                    side.enabled = true;
                    break;
                case FOLLOW_CAM:
                    follow.enabled = true;
                    break;
                case FREE_CAM:
                    free.enabled = true;
                    break;
                default:
                    focus.enabled = true;
                    break;
            }
        }
    }
}