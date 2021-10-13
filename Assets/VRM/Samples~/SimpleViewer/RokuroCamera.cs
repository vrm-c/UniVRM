using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRM.SimpleViewer
{
    public class RokuroCamera : MonoBehaviour
    {
        [Range(0.1f, 5.0f)]
        public float RotateSpeed = 0.7f;

        [Range(0.1f, 5.0f)]
        public float GrabSpeed = 0.7f;

        [Range(0.1f, 5.0f)]
        public float DollySpeed = 1.0f;

        struct PosRot
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        class _Rokuro
        {
            public float Yaw;
            public float Pitch;
            public float ShiftX;
            public float ShiftY;
            public float Distance = 2.0f;

            public void Rotate(float x, float y)
            {
                Yaw += x;
                Pitch -= y;
                Pitch = Mathf.Clamp(Pitch, -90, 90);
            }

            public void Grab(float x, float y)
            {
                ShiftX += x * Distance;
                ShiftY += y * Distance;
            }

            public void Dolly(float delta)
            {
                if (delta > 0)
                {
                    Distance *= 0.9f;
                }
                else if (delta < 0)
                {
                    Distance *= 1.1f;
                }
            }

            public PosRot Calc()
            {
                var r = Quaternion.Euler(Pitch, Yaw, 0);
                return new PosRot
                {
                    Position = r * new Vector3(-ShiftX, -ShiftY, -Distance),
                    Rotation = r,
                };
            }
        }
        private _Rokuro _currentCamera = new _Rokuro();

        private List<Coroutine> _activeCoroutines = new List<Coroutine>();
        private void OnEnable()
        {
            // left mouse drag
            _activeCoroutines.Add(StartCoroutine(MouseDragOperationCoroutine(0, diff =>
            {
                _currentCamera.Rotate(diff.x * RotateSpeed, diff.y * RotateSpeed);
            })));
            // right mouse drag
            _activeCoroutines.Add(StartCoroutine(MouseDragOperationCoroutine(1, diff =>
            {
                _currentCamera.Rotate(diff.x * RotateSpeed, diff.y * RotateSpeed);
            })));
            // middle mouse drag
            _activeCoroutines.Add(StartCoroutine(MouseDragOperationCoroutine(2, diff =>
            {
                _currentCamera.Grab(
                    diff.x * GrabSpeed / Screen.height,
                    diff.y * GrabSpeed / Screen.height
                    );
            })));
            // mouse wheel
            _activeCoroutines.Add(StartCoroutine(MouseScrollOperationCoroutine(diff =>
            {
                _currentCamera.Dolly(diff.y * DollySpeed);
            })));
        }
        private void OnDisable()
        {
            foreach (var coroutine in _activeCoroutines)
            {
                StopCoroutine(coroutine);
            }
            _activeCoroutines.Clear();
        }
        private void Update()
        {
            var posRot = _currentCamera.Calc();

            transform.localRotation = posRot.Rotation;
            transform.localPosition = posRot.Position;
        }
        private IEnumerator MouseDragOperationCoroutine(int buttonIndex, Action<Vector2> dragOperation)
        {
            while (true)
            {
                while (!Input.GetMouseButtonDown(buttonIndex))
                {
                    yield return null;
                }
                var prevPos = Input.mousePosition;
                while (Input.GetMouseButton(buttonIndex))
                {
                    var currPos = Input.mousePosition;
                    var diff = currPos - prevPos;
                    dragOperation(diff);

                    prevPos = currPos;
                    yield return null;
                }
            }
        }
        private IEnumerator MouseScrollOperationCoroutine(Action<Vector2> scrollOperation)
        {
            while (true)
            {
                scrollOperation(Input.mouseScrollDelta);
                yield return null;
            }
        }
    }
}
