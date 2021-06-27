using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Treevel.Common.Components.UIs
{
    public class TapObjectGenerator : MonoBehaviour
    {
        public GameObject tapObjectController;
        private Vector3 _mousePosition;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mousePosition = Input.mousePosition;
                Instantiate(tapObjectController, Camera.main.ScreenToWorldPoint(_mousePosition),Quaternion.identity);
            }
        }
    }
}
