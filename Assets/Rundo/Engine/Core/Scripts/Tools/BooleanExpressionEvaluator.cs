using System;
using System.Collections.Generic;
using System.Linq;

namespace Rundo.Tools
{
    #region Example
    // Example A:
    //    new BooleanExpressionEvaluator("!((true|false)&!false")).Evaluate();
    //
    // Evaluated example A: returns false
    //
    //
    // Example B:
    //    new BooleanExpressionEvaluator("(A|B)&!C").Evaluate(literal => {
    //        if (literal == A) return true;
    //        if (literal == B || literal == C) return false;});
    //
    // Evaluated example B: (true|false)&!false => returns true
    #endregion
    
    /**
     * Expression is parsed to a node-tree where node is either a boolean operator or a literal value. Each literal
     * value is converted to a boolean value using internal implicit conversion, or using explicit conversion function.
     */
    public class BooleanExpressionEvaluator
    {
        private readonly Node _root;
        private readonly bool _toLowerCaseLiterals;
        private readonly Dictionary<string, bool> _booleanValues = new Dictionary<string, bool>();
        
        private bool IsExpressionValid => _booleanValues.Count > 0;

        public BooleanExpressionEvaluator(string expr, bool toLowerCaseLiterals = false)
        {
            _toLowerCaseLiterals = toLowerCaseLiterals;
            
            if (string.IsNullOrEmpty(expr))
                return;
            
            var tokens = Tokenizer.Tokenize(expr);
            var polishNotation = TransformToPolishNotation(tokens);
            var enumerator = polishNotation.GetEnumerator();
            enumerator.MoveNext();
            _root = Make(ref enumerator);
        }

        /**
         * Implicit string -> bool conversion. Literals are expected to be either "true" or "false" values.
         */
        public bool Evaluate()
        {
            return Evaluate(literal => bool.TryParse(literal, out var parsedValue) && parsedValue);
        }

        /**
         * Explicit string -> bool conversion provided by the "literalToBoolConverter".
         */
        public bool Evaluate(Func<string, bool> literalToBoolConverter)
        {
            if (!IsExpressionValid)
                return false;

            foreach (var key in _booleanValues.Keys.ToArray())
                _booleanValues[key] = literalToBoolConverter.Invoke(key);

            return EvaluateNode(_root);
        }

        private bool EvaluateNode(Node node)
        {
            switch (node.NodeType)
            {
                case Node.NodeTypeEnum.LEAF:
                    if (_booleanValues.ContainsKey(node.Lit) == false)
                        return false;
                    return _booleanValues[node.Lit];
                case Node.NodeTypeEnum.AND:
                    if (node.Left == null || node.Right == null)
                        return false;
                    return EvaluateNode(node.Left) && EvaluateNode(node.Right);
                case Node.NodeTypeEnum.OR:
                    if (node.Left == null || node.Right == null)
                        return true;
                    return EvaluateNode(node.Left) || EvaluateNode(node.Right);
                case Node.NodeTypeEnum.NOT:
                    if (node.Left == null)
                        return false;
                    return !EvaluateNode(node.Left);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Node Make(ref List<Token>.Enumerator polishNotationTokensEnumerator)
        {
            if (polishNotationTokensEnumerator.Current == null)
                return null;
            
            if (polishNotationTokensEnumerator.Current.TokenType == Token.TokenTypeEnum.LITERAL)
            {
                var value = polishNotationTokensEnumerator.Current.TokenLiteral;
                polishNotationTokensEnumerator.MoveNext();

                value = value.Trim();
                if (_toLowerCaseLiterals)
                    value = value.ToLower();

                if (_booleanValues.ContainsKey(value) == false)
                    _booleanValues.Add(value, false);

                return new Node{Lit = value};
            }
            else
            {
                Node.NodeTypeEnum nodeType = Node.NodeTypeEnum.OR;
                Node left = null;
                Node right = null;
                
                switch (polishNotationTokensEnumerator.Current.TokenValue)
                {
                    case Token.TokenValueEnum.AND:
                        polishNotationTokensEnumerator.MoveNext();
                        nodeType = Node.NodeTypeEnum.AND;
                        left = Make(ref polishNotationTokensEnumerator);
                        right = Make(ref polishNotationTokensEnumerator);
                        break;
                    case Token.TokenValueEnum.OR:
                        polishNotationTokensEnumerator.MoveNext();
                        nodeType = Node.NodeTypeEnum.OR;
                        left = Make(ref polishNotationTokensEnumerator);
                        right = Make(ref polishNotationTokensEnumerator);
                        break;
                    case Token.TokenValueEnum.NOT:
                        polishNotationTokensEnumerator.MoveNext();
                        nodeType = Node.NodeTypeEnum.NOT;
                        left = Make(ref polishNotationTokensEnumerator);
                        break;
                    default:
                        return null;
                }
                
                return new Node
                {
                    NodeType = nodeType,
                    Left = left,
                    Right = right,
                };
            }
        }

        private static List<Token> TransformToPolishNotation(IReadOnlyList<Token> infixTokenList)
        {
            Queue<Token> outputQueue = new Queue<Token>();
            Stack<Token> stack = new Stack<Token>();

            int index = 0;
            while (infixTokenList.Count > index)
            {
                Token t = infixTokenList[index];

                switch (t.TokenType)
                {
                    case Token.TokenTypeEnum.LITERAL:
                        outputQueue.Enqueue(t);
                        break;
                    case Token.TokenTypeEnum.BINARY_OP:
                    case Token.TokenTypeEnum.UNARY_OP:
                    case Token.TokenTypeEnum.OPEN_PAREN:
                        stack.Push(t);
                        break;
                    case Token.TokenTypeEnum.CLOSE_PAREN:
                        while (stack.Peek().TokenType != Token.TokenTypeEnum.OPEN_PAREN)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        stack.Pop();
                        if (stack.Count > 0 && stack.Peek().TokenType == Token.TokenTypeEnum.UNARY_OP)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        break;
                    default:
                        break;
                }

                ++index;
            }
            while (stack.Count > 0)
            {
                outputQueue.Enqueue(stack.Pop());
            }

            return outputQueue.Reverse().ToList();
        }
    }

    internal class Node
    {
        public enum NodeTypeEnum
        {
            LEAF,
            AND,
            OR,
            NOT
        };

        public NodeTypeEnum NodeType;
        public Node Left;
        public Node Right;
        public string Lit;
    }
    
    internal class Token
    {
        private static readonly Dictionary<string, KeyValuePair<TokenTypeEnum, TokenValueEnum>> Tokens = new Dictionary<string, KeyValuePair<TokenTypeEnum, TokenValueEnum>>()
        {
            {
                "(", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.OPEN_PAREN, TokenValueEnum.OPEN_PAREN)
            },
            {
                ")", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.CLOSE_PAREN, TokenValueEnum.CLOSE_PAREN)
            },
            {
                "!", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.UNARY_OP, TokenValueEnum.NOT)
            },
            {
                "-", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.UNARY_OP, TokenValueEnum.NOT)
            },
            {
                "NOT", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.UNARY_OP, TokenValueEnum.NOT)
            },
            {
                "+", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.BINARY_OP, TokenValueEnum.AND)
            },
            {
                "&", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.BINARY_OP, TokenValueEnum.AND)
            },
            {
                "AND", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.BINARY_OP, TokenValueEnum.AND)
            },
            {
                "/", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.BINARY_OP, TokenValueEnum.OR)
            },
            {
                "OR", new KeyValuePair<TokenTypeEnum, TokenValueEnum>(TokenTypeEnum.BINARY_OP, TokenValueEnum.OR)
            },
        };

        public enum TokenValueEnum
        {
            OPEN_PAREN,
            CLOSE_PAREN,
            AND,
            OR,
            NOT,
        }

        public enum TokenTypeEnum
        {
            OPEN_PAREN,
            CLOSE_PAREN,
            UNARY_OP,
            BINARY_OP,
            LITERAL,
            EXPR_END
        }

        public readonly TokenTypeEnum TokenType;
        public readonly TokenValueEnum TokenValue;
        public readonly string TokenLiteral;

        public Token(string expression, ref int refIndex)
        {
            // skip initial whitespaces
            for (; refIndex < expression.Length; ++refIndex)
                if (char.IsWhiteSpace(expression[refIndex]) == false)
                    break;
            
            int startIndex = refIndex;

            // is end of expression
            if (startIndex >= expression.Length)
            {
                TokenType = TokenTypeEnum.EXPR_END;
                return;
            }

            // find index of the next token
            (int index, string token) FindNextToken()
            {
                var tokens = Tokens.Keys.ToArray();

                for (var i = startIndex; i < expression.Length; ++i)
                {
                    var tokenTemp = "";
                    for (var j = i; j < expression.Length; ++j)
                    {
                        tokenTemp += expression[j];
                        var possibleTokenExists = false;
                        foreach (var token in tokens)
                        {
                            if (possibleTokenExists == false && token.StartsWith(tokenTemp))
                                possibleTokenExists = true;
                            
                            if (token == tokenTemp)
                                return (i, tokenTemp);
                        }
                        
                        if (possibleTokenExists == false)
                            break;
                    }
                }

                return (expression.Length, null);
            }

            var nextToken = FindNextToken();

            // is token
            if (string.IsNullOrEmpty(nextToken.token) == false && nextToken.index == startIndex)
            {
                var op = nextToken.token;
                refIndex += op.Length;
                TokenType = Tokens[op].Key;
                TokenValue = Tokens[op].Value;
                return;
            }

            refIndex = nextToken.index;

            var word = expression.Substring(startIndex, nextToken.index - startIndex).Trim();
            TokenType = TokenTypeEnum.LITERAL;
            TokenLiteral = word;
        }
    }

    internal static class Tokenizer
    {
        public static List<Token> Tokenize(string expression)
        {
            expression = expression.Trim();
            
            var prevCharWasSpace = false;
            var expressionWithSingleSpaces = "";

            // remove multiple spaces
            for (int i = 0; i < expression.Length; ++i)
            {
                if (char.IsWhiteSpace(expression[i]))
                {
                    if (prevCharWasSpace)
                        continue;
                    prevCharWasSpace = true;
                }
                else
                {
                    prevCharWasSpace = false;
                }
                expressionWithSingleSpaces += expression[i];
            }
            
            var tokens = new List<Token>();
            var index = 0;
            Token t = null;
            do
            {
                t = new Token(expressionWithSingleSpaces, ref index);
                tokens.Add(t);
            } while (t.TokenType != Token.TokenTypeEnum.EXPR_END);

            return tokens;
        }
    }
}

