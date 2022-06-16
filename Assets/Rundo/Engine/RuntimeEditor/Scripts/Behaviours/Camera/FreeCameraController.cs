using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Unity-editor camera style.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FreeCameraController : EditorBaseBehaviour
    {
        public float LookSpeed = 0.5f;
        public float MoveSpeed = 10.0f;
        public float ZoomSpeed = 25.0f;
        public float DragSpeed = 10.0f;
        public float MoveTurboMultiplier = 3.0f;

        private Vector3 _prevMousePosition;

        private void Start()
        {
            _prevMousePosition = Input.mousePosition;
        }

        private void Update()
        {
            if (RuntimeEditorBehaviour.IsInputOverWorld == false)
                return;
            
            float inputVertical = 0f;
            float inputForward = 0f;
            float inputLeft = 0f;
            var delta = transform.position.magnitude / 100f;
            var speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? MoveTurboMultiplier : 1.0f;
            var mousePosition = Input.mousePosition;
            var mouseDist = mousePosition - _prevMousePosition;
            
            if (_prevMousePosition != mousePosition)
            {
                if (Input.GetMouseButton(1))
                {
                    float rotationX = transform.localEulerAngles.x;
                    float newRotationY = transform.localEulerAngles.y + mouseDist.x * LookSpeed;

                    // Euler angle mapping fix
                    float newRotationX = (rotationX - mouseDist.y * LookSpeed);
                    if (rotationX <= 90.0f && newRotationX >= 0.0f)
                        newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
                    if (rotationX >= 270.0f)
                        newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

                    transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);
                }
                else if (Input.GetMouseButton(2))
                {
                    float moveSpeed = Time.deltaTime * MoveSpeed * speedMultiplier;
                    transform.position += transform.right * moveSpeed * -mouseDist.x * DragSpeed
                                          + transform.up * moveSpeed * -mouseDist.y * DragSpeed;
                }
            }
            
            if (Input.mouseScrollDelta.y != 0)
            {
                transform.position += transform.forward * (ZoomSpeed * speedMultiplier * delta * ((Input.mouseScrollDelta.y < 0)?-1:1));
            }

            if (Input.GetMouseButton(1))
            {    
                if (Input.GetKey(KeyCode.Q))
                    inputVertical += -1;
                if (Input.GetKey(KeyCode.E))
                    inputVertical += 1;

                if (Input.GetKey(KeyCode.W))
                    inputForward += 1;
                if (Input.GetKey(KeyCode.S))
                    inputForward -= 1;

                if (Input.GetKey(KeyCode.A))
                    inputLeft -= 1;
                if (Input.GetKey(KeyCode.D))
                    inputLeft += 1;
            }
                
            if (inputVertical != 0.0f || inputForward != 0.0f || inputLeft != 0f)
            {
                var moveSpeed = Time.deltaTime * MoveSpeed;
                
                if (Input.GetMouseButton(2))
                    moveSpeed *= Input.GetKey(KeyCode.LeftShift) ? MoveTurboMultiplier : 1.0f;
                
                transform.position += 
                    transform.up * moveSpeed * inputVertical + 
                    transform.forward * moveSpeed * inputForward +
                    transform.right * moveSpeed * inputLeft;
            }

            _prevMousePosition = mousePosition;
        }
    }
}