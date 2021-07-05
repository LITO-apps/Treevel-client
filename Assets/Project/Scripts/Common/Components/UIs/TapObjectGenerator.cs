using UnityEngine;

namespace Treevel.Common.Components.UIs
{
    [RequireComponent(typeof(Canvas))]
    public class TapObjectGenerator : MonoBehaviour
    {
        public GameObject tapObjectController;
        private Vector3 _mousePosition;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mousePosition = Input.mousePosition;
                var tapObject = Instantiate(tapObjectController, transform, false);
                tapObject.transform.position = _mousePosition;
            }
        }
    }
}
