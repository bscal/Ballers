using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ballers
{
    public class PlayerSettings : MonoBehaviour
    {

        public static PlayerSettings Singleton;

        private const int FOCUS_CAM = 0;
        private const int SIDE_CAM = 1;
        private const int FOLLOW_CAM = 2;
        private const int FREE_CAM = 3;

        public Camera follow;
        public Camera side;
        public Camera focus;
        public Camera free;

        public Camera Current { get; private set; }

        private Dropdown m_dropdown;

        void Awake()
        {
            Singleton = this;
            m_dropdown = GameObject.Find("Camera Dropdown").GetComponent<Dropdown>();
            m_dropdown.onValueChanged.AddListener(delegate { OnCameraChanged(m_dropdown.value); });

        }

        private void Start()
        {
            follow.enabled = false;
            side.enabled = false;
            free.enabled = false;
            OnCameraChanged(FOCUS_CAM);
        }

        public void OnCameraChanged(int id)
        {
            if (Current != null) Current.enabled = false;
            Current = IntToCamera(id);
            Current.enabled = true;
        }

        private Camera IntToCamera(int id)
        {
            switch (id)
            {
                case FOCUS_CAM:
                    return focus;
                case SIDE_CAM:
                    return side;
                case FOLLOW_CAM:
                    return follow;
                case FREE_CAM:
                    return free;
                default:
                    return focus;
            }
        }
    }
}