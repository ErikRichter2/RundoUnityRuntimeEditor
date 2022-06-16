using UnityEngine;

namespace Rundo.RuntimeEditor.Tools
{
    /// <summary>
    /// Creates a Texture2D snapshots of a prefab, to be used as a image element in the project window for example.
    /// </summary>
    public class PrefabScreenshoterBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private Camera _camera;

        private Vector3 _originalPosition;

        private void Start()
        {
            _originalPosition = _camera.transform.position;
            _camera.enabled = false;
        }

        public Texture2D Screenshot(GameObject prefab)
        {
            gameObject.SetActive(true);
            prefab.gameObject.SetActive(true);
            prefab.transform.SetParent(_content, false);
            prefab.transform.localPosition = Vector3.zero;

            var center = Vector3.zero;
            var meshes = prefab.GetComponentsInChildren<MeshFilter>();
            foreach (var mesh in meshes)
                center += mesh.GetComponent<Renderer>().bounds.center;

            center /= meshes.Length;
            
            _camera.transform.position = _originalPosition;
            _camera.transform.LookAt(center);
            
            prefab.gameObject.SetActive(true);

            var rt = new RenderTexture(256, 256, 32);
            _camera.targetTexture = rt;

            _camera.enabled = true;
            _camera.Render();
            RenderTexture.active = rt;
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            texture.Apply();

            RenderTexture.active = null;

            _camera.enabled = false;
            _camera.targetTexture = null;
            prefab.gameObject.SetActive(false);
            gameObject.SetActive(false);
            
            return texture;
        }
    }
}

