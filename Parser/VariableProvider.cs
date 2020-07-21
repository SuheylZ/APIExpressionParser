﻿using LanguageExt;

using System;
using System.Linq;
using System.Text;

using static LanguageExt.Prelude;


namespace Parser
{
    #region  Dead Code
    //public enum Operator:byte
    //{
    //    Equals,
    //    NotEquals,
    //    GreaterThan,
    //    GreaterThanOrEqual,
    //    LessThan,
    //    LessThanOrEqual,
    //    Like
    //}
    //public enum Joiner: byte
    //{
    //    AND,
    //    OR
    //}
    //public enum ValueOrVariable:byte
    //{
    //    Value,
    //    Variable
    //}


    //public readonly struct Operand
    //{
    //    public readonly ValueOrVariable type;
    //    public readonly string value;
    //}


    //public readonly struct Expression1
    //{
    //    public readonly Operand leftOperand;
    //    public readonly Operator @operator;
    //    public readonly Operand rightOperand;
    //}

    //public readonly struct Expression2
    //{
    //    public readonly Operand leftOperand;
    //}

    //public readonly struct Expression3
    //{
    //    public readonly Expression1 leftExpression;
    //    public readonly Joiner @operator;
    //    public readonly Expression1 rightOperand;
    //}




    //internal class DataProvider
    //{
    //    readonly Memory<char> _data;
    //    int _nextIndex;

    //    public DataProvider(string data)
    //    {
    //        _data = data.ToCharArray();
    //        _nextIndex = 0;
    //    }

    //    public IEnumerable<Span<char>> GetToken()
    //    {
    //        var operators = new[]{'=', '>', '<', '(', ')' };
    //        var span = _data.Span;
    //        var curr = _nextIndex;
    //        var endPosition = _nextIndex;

    //        var nextToken = "";

    //        while(curr< _data.Length)
    //        {
    //            if(span[curr] == '@' || char.IsLetter(span[curr]))
    //            {
    //                nextToken = "variable";
    //                var (v0, idx)= ExtractVariable(_data.Slice(curr));
    //                curr += idx;
    //                yield return v0;
    //            }
    //            else if(char.IsDigit(span[curr]))
    //            {
    //                nextToken = "number";
    //                var (v0, idx) = ExtractNumber(_data.Slice(curr));
    //                curr += idx;
    //                yield return v0;
    //            }
    //            else if(operators.Contains(span[curr]))
    //            {
    //                nextToken = "operator";
    //                var (v0, idx) = ExtractOperator(_data.Slice(curr));
    //                curr += idx;
    //                yield return v0;
    //            }
    //            else if(char.IsWhiteSpace(span[curr]))
    //            {
    //                curr++;
    //            }
    //        }


    //        while(_nextIndex < _data.Length)
    //        {
    //            if(span[curr]=='@' || char.IsLetterOrDigit(span[curr])

    //        }




    //    }


    //}

    //internal class Tokenizer
    //{

    //} 
    #endregion

    public class VariableProvider
    {
        ILexemeProvider _provider;

        public VariableProvider(ILexemeProvider provider) => _provider = provider ?? throw new ArgumentNullException(nameof(provider));


        enum States { Started, LetterFound, LetterOrDigitFound, DotFound, Finished, Error };
        enum LexemeType { Unknown, Alpha, Digit, Dot, Symbol };


        /// <summary>
        /// THis function is a state machine. it should be called only when the lexeme is a character that is valid for this machine 
        /// it advances the provider only when the current charcter is a valid one. if it is not then it exits 
        ///  </summary>
        /// <param name="provider">Lexeme Provider</param>
        /// <returns> a valid character or nothing</returns>
        public Option<string> Get()
        {
            var token = Option<string>.None;
            Func<char, LexemeType> lexemeIs = (ch) =>
            {
                var type = LexemeType.Unknown;

                if(char.IsLetter(ch)) type = LexemeType.Alpha;
                else if(char.IsDigit(ch)) type = LexemeType.Digit;
                else if('.' == ch) type = LexemeType.Dot;
                else if(new[] { '=', '>', '<', '(', ')', '&', '|' }.Contains(ch)) type = LexemeType.Symbol;
                else type = LexemeType.Unknown;

                return type;
            };

            //// Verify that provider has valid character now
            //if(lexemeIs(_provider.Current) != LexemeType.Alpha)
            //    return token;



            Predicate<States> canContinue = (s0)=> s0 != States.Error && s0!= States.Finished;
            var (current, future) = (States.Started, States.Started);
            var sb = new StringBuilder();

            do
            {
                var ch = _provider.Current;
                switch(current)
                {

                    case States.Started: // Past: ? Current Letter, Next: Letter|Digit|.
                        switch(lexemeIs(ch))
                        {
                            case LexemeType.Alpha:
                                sb.Append(ch);
                                future = States.LetterFound;
                                break;

                            default:
                                future = States.Error;
                                break;
                        }
                        break;

                    case States.LetterFound: // Past: letter, Current: Letter|digit|. , Next: Letter|digit|.
                        switch(lexemeIs(ch))
                        {
                            case LexemeType.Alpha:
                            case LexemeType.Digit:
                                sb.Append(ch);
                                future = States.LetterOrDigitFound;
                                break;
                            case LexemeType.Dot:
                                sb.Append(ch);
                                future = States.DotFound;
                                break;
                            default:
                                future = States.Finished;
                                break;
                        }
                        break;

                    case States.LetterOrDigitFound:
                        switch(lexemeIs(ch))
                        {
                            case LexemeType.Alpha:
                            case LexemeType.Digit:
                                sb.Append(ch);
                                future = States.LetterOrDigitFound;
                                break;
                            case LexemeType.Dot:
                                sb.Append(ch);
                                future = States.DotFound;
                                break;
                            default:
                                future = States.Finished;
                                break;
                        }
                        break;


                    case States.DotFound:
                        switch(lexemeIs(ch))
                        {
                            case LexemeType.Alpha:
                                sb.Append(ch);
                                future = States.LetterFound;
                                break;
                            case LexemeType.Digit:
                            case LexemeType.Dot:
                                future = States.Error;
                                break;
                            default:
                                future = States.Finished;
                                break;
                        }
                        break;


                    case States.Finished:

                    case States.Error:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                current = future;
            }
            while(canContinue(current) && _provider.Next());

            if(future == States.Finished)
                token = Some(sb.ToString());




            return token;
        }
    }
}