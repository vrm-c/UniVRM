using UnityEngine;
using UnityEngine.UI;

namespace VRM10.Samples.MToon10Showcase
{
    public sealed class CameraScroller : MonoBehaviour
    {
        [SerializeField] private GraphicScrollHandler _scrollHandler;
        [SerializeField] private Scrollbar _scrollbar;
        private bool _initialized;
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _scrollWeightDelta;
        private float _currentWeight;
        private Transform _cameraTransform;

        public void Initialize(Vector3 startPosition, Vector3 endPosition, float scrollDistance)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _scrollWeightDelta = scrollDistance / Vector3.Distance(startPosition, endPosition);
            _currentWeight = 0.0f;
            _cameraTransform = Camera.main.transform;
            _cameraTransform.position = _startPosition;
            _scrollbar.enabled = true;
            _scrollbar.onValueChanged.AddListener(Scroll);
            _scrollHandler.OnScrollAction = (eventData) =>
            {
                var sign = Mathf.Sign(eventData.scrollDelta.y);
                Scroll(_currentWeight - _scrollWeightDelta * sign);
            };
        }

        private void Scroll(float weight)
        {
            _currentWeight = Mathf.Clamp01(weight);
            _scrollbar.value = _currentWeight;
            _cameraTransform.position = Vector3.Lerp(_startPosition, _endPosition, _currentWeight);
        }
    }
}