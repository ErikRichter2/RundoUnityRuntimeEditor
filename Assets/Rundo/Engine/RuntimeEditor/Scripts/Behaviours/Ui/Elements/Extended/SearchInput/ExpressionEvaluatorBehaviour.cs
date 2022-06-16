using System;
using Rundo.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rundo.RuntimeEditor.Behaviours
{
    public class ExpressionEvaluatorBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _closeBtn;

        private BooleanExpressionEvaluator _booleanExpressionEvaluator;
        private string _currentExpression;
        private Action _onExpressionChanged;
        
        public string Expression
        {
            get => _inputField.text;
            set => _inputField.text = value;
        }

        private void Start()
        {
            Expression = "";
            _closeBtn.gameObject.SetActive(false);
            _closeBtn.onClick.AddListener(() =>
            {
                Expression = "";
            });
            _inputField.onValueChanged.AddListener(e =>
            {
                _closeBtn.gameObject.SetActive(string.IsNullOrEmpty(Expression) == false);
                _onExpressionChanged?.Invoke();
            });
        }

        public bool Evaluate(Func<string, bool> evaluator)
        {
            if (string.IsNullOrEmpty(Expression))
                return true;
            
            if (_currentExpression != Expression)
            {
                _currentExpression = Expression;
                _booleanExpressionEvaluator = new BooleanExpressionEvaluator(_currentExpression, true);
            }
            
            return _booleanExpressionEvaluator.Evaluate(evaluator);
        }
        
        public void OnExpressionChange(Action onExpressionChanged)
        {
            _onExpressionChanged = onExpressionChanged;
        }
    }
}

