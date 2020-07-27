﻿using System;
using System.Runtime.CompilerServices;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Parser.Lexemes
{
    public class LexemeProvider :ILexemeProvider
    {
        readonly Memory<char> _data;
        readonly int _maxPosition;

        int _index;


        public LexemeProvider(in string input)
        {
            if(!string.IsNullOrEmpty(input))
            {
                _data = new Memory<char>(input.ToCharArray());
                _maxPosition = _data.Length-1;
                Reset();
            }
            else
                throw new NullReferenceException("Input string is empty. Lexeme provider must have non empty string");
        }

        public Option<Lexeme> LookAhead => ReadAtPosition(_index + 1);
        public Lexeme Current
        {
            get
            {
                var t0 = ReadAtPosition(_index);
                return t0.Match(x => x, () => throw new InvalidOperationException());
            }
        }
        public bool IsSafeToRead => _index <= _maxPosition;

        public bool Next()
        {
            var ret = false;

            switch(_index)
            {
                case int x when x < _maxPosition:
                    _index += 1;
                    ret = true;
                    break;

                case int x when x == _maxPosition:
                    _index += 1;
                    ret = false;
                    break;

                case int x when x > _maxPosition:
                    ret = false;
                    break;
            }
 
            return ret;
        }
        public void Reset() => _index = 0;
        public bool Back() {
            var ret = false;

            switch(_index)
            {
                case 0:
                    ret = false;
                    break;

                case int x when x < 0:
                    _index = 0;
                    ret = false;
                    break;

                case int x when x>0:
                    _index -= 1;
                        ret = true;
                    break;
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsValidCharacter(char ch) => !(char.IsControl(ch) || char.IsSurrogate(ch));
        Option<Lexeme> ReadAtPosition(int position)
        {
            if(position <= _maxPosition)
            {
                var ch = _data.Span[position];
                var t0 = IsValidCharacter(ch) ? Some(new Lexeme(ch)) : None;
                return t0;
            }
            else
                return Option<Lexeme>.None;

        }
    }
}