using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours.UI
{
    public class ProjectWindowBehaviour : EditorBaseBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private ProjectWindowItemBehaviour _prefabsWindowItemPrefab;
        [SerializeField] private ProjectItemsSearchFilterBehaviour _projectItemsSearchFilterBehaviour;
        [SerializeField] private Button _closeBtn;

        private readonly List<ProjectWindowItemBehaviour> _items = new List<ProjectWindowItemBehaviour>();
        private readonly Dictionary<GameObject, Texture2D> _screenshotsCache = new Dictionary<GameObject, Texture2D>();
        private readonly Queue<ProjectWindowItemBehaviour> _screenshotsQueue = new Queue<ProjectWindowItemBehaviour>();
        
        private void Start()
        {
            var data = new List<ProjectItemMetaData>();

            foreach (var it in GetComponents<ProjectWindowBaseDataProviderBehaviour>())
                if (it.enabled)
                    data.AddRange(it.GetData());
            
            _projectItemsSearchFilterBehaviour.SetData(data, Redraw);

            _closeBtn.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                DispatchUiEvent(new EditorUiBehaviour.HideWindowEvent{Window = GetType()});
            });

            StartCoroutine(UpdateThumbnails());
        }

        private void Redraw(List<ProjectItemMetaData> data)
        {
            foreach (var it in _items)
                Destroy(it.gameObject);
            
            _items.Clear();
            _screenshotsQueue.Clear();
            
            foreach (var it in data)
            {
                var item = Instantiate(_prefabsWindowItemPrefab, _content);
                item.SetData(it);
                _items.Add(item);
                _screenshotsQueue.Enqueue(item);
            }
        }

        private IEnumerator UpdateThumbnails()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                while (_screenshotsQueue.Count > 0)
                {
                    var item = _screenshotsQueue.Dequeue();

                    var go = item.ProjectItemMetaData.GameObject;
                    if (go != null)
                    {
                        if (_screenshotsCache.TryGetValue(go, out var screenshot))
                        {
                            item.UpdateScreenshot(screenshot);
                        }
                        else
                        {
                            var instance = Instantiate(go, null);
                            screenshot = RuntimeEditor.PrefabScreenshoterBehaviour.Screenshot(instance);
                            Destroy(instance);
                            _screenshotsCache[go] = screenshot;
                            item.UpdateScreenshot(screenshot);
                            break;
                        }
                    }
                }
            }
        }
    }
}
