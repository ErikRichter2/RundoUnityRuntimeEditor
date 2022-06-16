using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class SimpleScrollViewControllerBehaviour : MonoBehaviour
    {
        [SerializeField] private RectTransform _content;
        [SerializeField] private int _rowCount = 1;
        [SerializeField] private Vector2 _customCellSize;
        [SerializeField] private bool _expandWidth;

        private ScrollRect _scrollRect;
        private RectTransform _scrollRectTransform;
        private ISimpleScrollViewControllerData _dataProvider;
        private Vector2[] _positionsByIndex;
        private bool _invalidateRebuild;
        private bool _invalidateRedraw;
        private int _contentY;
        private bool _wasFirstRebuild;
        private int _invalidateScrollToIndex = -1;

        private Vector2 _cellSize = Vector2.zero;
        private Vector2 _spacing = Vector2.zero;

        private void Start()
        {
            _scrollRect = _content.GetComponentInParent<ScrollRect>();
            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();

            _cellSize = _customCellSize;
            
            if (_content.TryGetComponent<VerticalLayoutGroup>(out var verticalLayoutGroup))
            {
                _spacing = new Vector2(0, verticalLayoutGroup.spacing);
            }
            else if (_content.TryGetComponent<GridLayoutGroup>(out var gridLayoutGroup))
            {
                _cellSize = gridLayoutGroup.cellSize;
                _spacing = gridLayoutGroup.spacing;
                if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                    _rowCount = gridLayoutGroup.constraintCount;
            }

            if (_content.TryGetComponent<LayoutGroup>(out var layoutGroup))
                layoutGroup.enabled = false;
            if (_content.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter))
                contentSizeFitter.enabled = false;

            Rebuild();
        }
        
        public void SetDataProvider<T>(SimpleScrollViewControllerData<T> dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void Rebuild()
        {
            _invalidateRebuild = true;
        }

        public void DestroyChildren()
        {
            foreach (Transform child in _content)
                Destroy(child.gameObject);
        }

        public void Redraw()
        {
            if (_wasFirstRebuild == false)
                _invalidateRebuild = true;
            _invalidateRedraw = true;
        }

        private void RebuildInternal()
        {
            _wasFirstRebuild = true;
            _invalidateRedraw = true;
            for (int i = 0; i < _dataProvider.Count; ++i)
                _dataProvider.DestroyAtIndex(i);
            DestroyChildren();
            _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, 0);
        }

        private void RedrawInternal()
        {
            var scrollHeight = (int)_scrollRectTransform.rect.height;

            var rowCnt = 0;
            var totalHeight = 0f;
            var currentHeight = 0f;
            var totalWidth = 0f;
            if (_positionsByIndex == null || _positionsByIndex.Length != _dataProvider.Count)
                _positionsByIndex = new Vector2[_dataProvider.Count];
            for (int i = 0; i < _dataProvider.Count; ++i)
            {
                if (_dataProvider.IsInvisibleAtIndex(i))
                {
                    if (_dataProvider.IsInstantiatedAtIndex(i))
                        _dataProvider.GetRectTransformAtIndex(i).gameObject.SetActive(false);
                    continue;
                }

                var size = _dataProvider.IsSizeProvider ? _dataProvider.GetSizeAtIndex(i) : _cellSize;
                var pos = _positionsByIndex[i];
                
                pos.x = totalWidth;
                pos.y = -currentHeight;
                _positionsByIndex[i] = pos;
                
                totalWidth += size.x + _spacing.x;
                
                if (rowCnt == 0)
                {
                    totalHeight += size.y + _spacing.y;
                }

                if (++rowCnt == _rowCount)
                {
                    rowCnt = 0;
                    totalWidth = 0;
                    currentHeight += size.y + _spacing.y;
                }

                if ((pos.y - size.y) + _contentY <= 0 && pos.y + _contentY >= -scrollHeight)
                {
                    if (_dataProvider.IsInstantiatedAtIndex(i) == false)
                    {
                        var obj = _dataProvider.InstantiateAtIndex(i, _content);
                        var rectTransform = obj.GetComponent<RectTransform>();

                        if (_expandWidth)
                        {
                            rectTransform.anchoredPosition = new Vector2(pos.x, pos.y);
                            rectTransform.sizeDelta = new Vector2(size.x, size.y);
                            rectTransform.anchorMin = new Vector2(0, 1);
                            rectTransform.anchorMax = new Vector2(1, 1);
                            rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                            rectTransform.offsetMax = new Vector2(1, rectTransform.offsetMax.y);
                            //rectTransform.pivot = new Vector2(0.5f, 1);
                        }
                        else
                        {
                            rectTransform.anchorMin = new Vector2(0, 1);
                            rectTransform.anchorMax = new Vector2(1, 1);
                            rectTransform.pivot = new Vector2(0f, 1);
                            rectTransform.anchoredPosition = new Vector2(pos.x, pos.y);
                            rectTransform.sizeDelta = new Vector2(size.x, size.y);
                        }
                        
                        _dataProvider.ActivatedAtIndex(i);
                    }
                    else
                    {
                        var rectTransform = _dataProvider.GetRectTransformAtIndex(i);
                        rectTransform.gameObject.SetActive(true);
                        rectTransform.anchoredPosition = pos;
                        _dataProvider.ActivatedAtIndex(i);
                    }
                }
                else
                {
                    if (_dataProvider.IsInstantiatedAtIndex(i))
                    {
                        _dataProvider.GetRectTransformAtIndex(i).gameObject.SetActive(false);
                        _dataProvider.DeactivatedAtIndex(i);
                    }
                }
            }

            _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalHeight);
        }

        public void Update()
        {
            if (_dataProvider == null)
                return;

            if (_invalidateRebuild)
            {
                _invalidateRebuild = false;
                RebuildInternal();
            }

            var contentY = (int)_content.anchoredPosition.y;
            if (_invalidateRedraw || contentY != _contentY)
            {
                _invalidateRedraw = false;
                _contentY = contentY;
                RedrawInternal();
            }

            if (_invalidateScrollToIndex != -1)
            {
                _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, -_positionsByIndex[_invalidateScrollToIndex].y);
                _invalidateScrollToIndex = -1;
            }
        }

        public void ScrollToIndex(int index)
        {
            _invalidateScrollToIndex = index;
        }
    }
}

