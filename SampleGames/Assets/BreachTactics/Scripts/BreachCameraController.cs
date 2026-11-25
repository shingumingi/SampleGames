using UnityEngine;
using UnityEngine.InputSystem;

namespace BreachTactics
{
    /// <summary>
    /// 회전된 카메라에서도 화면의 위/아래/좌/우를 기준으로 이동하는 전술 카메라입니다.
    /// </summary>
    public sealed class BreachCameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float rotateSpeed = 0.18f;
        [SerializeField] private float keyRotateSpeed = 75f;
        [SerializeField] private float zoomSpeed = 1.2f;
        [SerializeField] private float minDistance = 7f;
        [SerializeField] private float maxDistance = 22f;

        private Vector3 pivot;
        private float yaw;
        private float pitch;
        private float distance;

        private void Awake()
        {
            pivot = Vector3.zero;
            Vector3 offset = transform.position - pivot;
            distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
            yaw = Mathf.Atan2(offset.x, -offset.z) * Mathf.Rad2Deg;
            pitch = Mathf.Asin(offset.y / Mathf.Max(0.01f, distance)) * Mathf.Rad2Deg;
            ApplyTransform();
        }

        private void Update()
        {
            HandleMove();
            HandleRotate();
            HandleZoom();
            ApplyTransform();
        }

        private void HandleMove()
        {
            if (Keyboard.current == null) return;

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontal += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                vertical -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                vertical += 1f;

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 direction = Vector3.ClampMagnitude(right * horizontal + forward * vertical, 1f);
            pivot += direction * moveSpeed * Time.unscaledDeltaTime;

            if (Keyboard.current.qKey.isPressed) yaw -= keyRotateSpeed * Time.unscaledDeltaTime;
            if (Keyboard.current.eKey.isPressed) yaw += keyRotateSpeed * Time.unscaledDeltaTime;
        }

        private void HandleRotate()
        {
            if (Mouse.current == null || !Mouse.current.rightButton.isPressed) return;

            Vector2 delta = Mouse.current.delta.ReadValue();
            yaw += delta.x * rotateSpeed;
            pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed, 28f, 78f);
        }

        private void HandleZoom()
        {
            if (Mouse.current == null) return;
            float scroll = Mouse.current.scroll.ReadValue().y / 120f;
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        private void ApplyTransform()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = pivot + rotation * new Vector3(0f, 0f, -distance);
            transform.rotation = rotation;
        }
    }
}
