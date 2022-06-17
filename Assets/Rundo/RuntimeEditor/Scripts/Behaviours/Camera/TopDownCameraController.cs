using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    /// <summary>
    /// Top-down camera with semi-fixed Y-axis for better level building
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TopDownCameraController : EditorBaseBehaviour
    {
        public float MoveSpeed = 10.0f;
        public float ZoomSpeed = 25.0f;
        public float DragSpeed = 10.0f;
        public float RotateSpeed = 1.0f;
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

            float inputVertical = 0;
            float inputHorizontal = 0;
            float inputForward = 0;

            float speedMultiplier = (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) ? MoveTurboMultiplier : 1.0f;

            var positionDiff = Input.mousePosition - _prevMousePosition;
            _prevMousePosition = Input.mousePosition;
            
            //Drag camera by right click
            if (Input.GetMouseButton(1) && positionDiff.magnitude != 0f)
            {
                inputVertical = -positionDiff.y * DragSpeed * speedMultiplier;
                inputHorizontal = -positionDiff.x * DragSpeed * speedMultiplier;
            }
            
            //Rotate camera by middle click
            if (Input.GetMouseButton(2) && positionDiff.magnitude != 0f)
            {
                float rotationAngleHorizontal = positionDiff.x * RotateSpeed * speedMultiplier;
                float offset = 20f; 
                transform.RotateAround(transform.position + transform.forward * offset, Vector3.up, rotationAngleHorizontal);
            }
            
            //Zooming
            if (Input.mouseScrollDelta.y != 0)
            {
                inputForward = ZoomSpeed * speedMultiplier * ((Input.mouseScrollDelta.y < 0)?-1:1);
            }

            float moveSpeed = Time.deltaTime * MoveSpeed;

            transform.position += transform.forward * moveSpeed * inputForward
                                  + transform.right * moveSpeed * inputHorizontal
                                  + transform.up * moveSpeed * inputVertical;
            
            float inputWS = 0f;
            float inputAD = 0f;

            if (Input.GetMouseButton(1))
            {
                if (Input.GetKey(KeyCode.W)) inputWS = 1;
                if (Input.GetKey(KeyCode.S)) inputWS = -1;
                if (Input.GetKey(KeyCode.A)) inputAD = -1;
                if (Input.GetKey(KeyCode.D)) inputAD = 1;
            }

            moveSpeed = Time.deltaTime * MoveSpeed * (Input.GetKey(KeyCode.LeftShift) ? MoveTurboMultiplier : 1.0f);

            transform.position += new Vector3(transform.forward.x, 0, transform.forward.z) * moveSpeed * inputWS +
                                  new Vector3(transform.right.x, 0, transform.right.z) * moveSpeed * inputAD;
        }
    }
}