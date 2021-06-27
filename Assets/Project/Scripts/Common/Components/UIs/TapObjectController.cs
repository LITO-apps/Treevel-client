using UnityEngine;

namespace Treevel.Common.Components.UIs
{
    public class TapObjectController : MonoBehaviour
    {
        private const float _TIME_LIMIT = 0.5f;
        private float _time = 0f;

        // Update is called once per frame
        void Update()
        {
            _time += Time.deltaTime;
            if (_time >= _TIME_LIMIT) Destroy(gameObject);
        }
    }
}
