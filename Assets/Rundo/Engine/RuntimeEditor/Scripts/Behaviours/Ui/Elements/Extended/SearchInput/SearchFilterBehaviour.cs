using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rundo.RuntimeEditor.Behaviours
{
    [RequireComponent(typeof(ExpressionEvaluatorBehaviour))]
    public abstract class SearchFilterBehaviour<T> : EditorBaseBehaviour
    {
        private readonly List<T> _data = new List<T>();
        private readonly List<T> _searchResult = new List<T>();
        private ExpressionEvaluatorBehaviour _expressionEvaluatorBehaviour;
        private bool _invalidateSearch;
        private Action<List<T>> _searchResultCallback;
        
        public bool IsExpression => string.IsNullOrEmpty(_expressionEvaluatorBehaviour.Expression) == false;
        
        private void Start()
        {
            _expressionEvaluatorBehaviour = GetComponent<ExpressionEvaluatorBehaviour>();
            _expressionEvaluatorBehaviour.OnExpressionChange(() =>
            {
                _invalidateSearch = true;
            });
        }

        private void Update()
        {
            if (_invalidateSearch)
            {
                _invalidateSearch = false;
                Search();
            }
        }

        private void Search()
        {
            _searchResult.Clear();

            foreach (var it in _data)
                if (IsExpression == false ||
                    _expressionEvaluatorBehaviour.Evaluate(literal => FilterFunction(literal, it)))
                    _searchResult.Add(it);
            
            _searchResultCallback.Invoke(_searchResult);
        }

        public void SetData(
            IEnumerable<T> data,
            Action<List<T>> searchResult)
        {
            _data.Clear();
            _data.AddRange(data);
            _searchResultCallback = searchResult;
            _invalidateSearch = true;
        }

        protected abstract bool FilterFunction(string literal, T data);
    }
}
