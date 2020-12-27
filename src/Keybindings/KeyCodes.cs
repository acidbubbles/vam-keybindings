using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class KeyCodes
{
    private static readonly Dictionary<KeyCode, string> _keyCodeToCharMap = new Dictionary<KeyCode, string>()
    {
        {KeyCode.None, ""},
        {KeyCode.A, "a"},
        {KeyCode.B, "b"},
        {KeyCode.C, "c"},
        {KeyCode.D, "d"},
        {KeyCode.E, "e"},
        {KeyCode.F, "f"},
        {KeyCode.G, "g"},
        {KeyCode.H, "h"},
        {KeyCode.I, "i"},
        {KeyCode.J, "j"},
        {KeyCode.K, "k"},
        {KeyCode.L, "l"},
        {KeyCode.M, "m"},
        {KeyCode.N, "n"},
        {KeyCode.O, "o"},
        {KeyCode.P, "p"},
        {KeyCode.Q, "q"},
        {KeyCode.R, "r"},
        {KeyCode.S, "s"},
        {KeyCode.T, "t"},
        {KeyCode.U, "u"},
        {KeyCode.V, "v"},
        {KeyCode.W, "w"},
        {KeyCode.X, "x"},
        {KeyCode.Y, "y"},
        {KeyCode.Z, "z"},
        {KeyCode.Alpha0, "0"},
        {KeyCode.Alpha1, "1"},
        {KeyCode.Alpha2, "2"},
        {KeyCode.Alpha3, "3"},
        {KeyCode.Alpha4, "4"},
        {KeyCode.Alpha5, "5"},
        {KeyCode.Alpha6, "6"},
        {KeyCode.Alpha7, "7"},
        {KeyCode.Alpha8, "8"},
        {KeyCode.Alpha9, "9"},
        {KeyCode.Keypad0, "0"},
        {KeyCode.Keypad1, "1"},
        {KeyCode.Keypad2, "2"},
        {KeyCode.Keypad3, "3"},
        {KeyCode.Keypad4, "4"},
        {KeyCode.Keypad5, "5"},
        {KeyCode.Keypad6, "6"},
        {KeyCode.Keypad7, "7"},
        {KeyCode.Keypad8, "8"},
        {KeyCode.Keypad9, "9"},
        {KeyCode.Exclaim, "!"},
        {KeyCode.DoubleQuote, "\""},
        {KeyCode.Hash, "#"},
        {KeyCode.Dollar, "$"},
        {KeyCode.Ampersand, "&"},
        {KeyCode.Quote, "'"},
        {KeyCode.LeftParen, "("},
        {KeyCode.RightParen, ")"},
        {KeyCode.Asterisk, "*"},
        {KeyCode.Plus, "+"},
        {KeyCode.Comma, ","},
        {KeyCode.Minus, "-"},
        {KeyCode.Period, "."},
        {KeyCode.Slash, "/"},
        {KeyCode.Colon, ":"},
        {KeyCode.Semicolon, ";"},
        {KeyCode.Less, "<"},
        {KeyCode.Equals, "="},
        {KeyCode.Greater, ">"},
        {KeyCode.Question, "?"},
        {KeyCode.At, "@"},
        {KeyCode.LeftBracket, "["},
        {KeyCode.Backslash, "\\"},
        {KeyCode.RightBracket, "]"},
        {KeyCode.Caret, "^"},
        {KeyCode.Underscore, "_"},
        {KeyCode.BackQuote, "`"},
    };

    private static readonly KeyCode[] _specialKeyCodes = new[]
    {
        KeyCode.None,
        KeyCode.LeftControl,
        KeyCode.RightControl,
        KeyCode.LeftShift,
        KeyCode.RightShift,
        KeyCode.LeftAlt,
        KeyCode.RightAlt,
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.Mouse2,
        KeyCode.Mouse3,
        KeyCode.Mouse4,
        KeyCode.Mouse5,
        KeyCode.Mouse6,
    };

    public static readonly KeyCode[] bindableKeyCodes = ((KeyCode[]) Enum.GetValues(typeof(KeyCode)))
        .Except(_specialKeyCodes)
        .ToArray();

    public static readonly KeyCode[] textKeyCodes = new KeyCode[0]
        .Concat(Enumerable.Range((int) KeyCode.A, KeyCode.Z - KeyCode.A).Select(i => (KeyCode) i))
        .Concat(Enumerable.Range((int) KeyCode.Alpha0, KeyCode.Alpha9 - KeyCode.Alpha0).Select(i => (KeyCode) i))
        .ToArray();

    public static KeyCode GetCurrent(this KeyCode[] keyCodes)
    {
        for (var i = 0; i < keyCodes.Length; i++)
        {
            var key = keyCodes[i];
            if (Input.GetKey(key)) return key;
        }

        return KeyCode.None;
    }

    public static KeyCode GetCurrentDown(this KeyCode[] keyCodes)
    {
        for (var i = 0; i < keyCodes.Length; i++)
        {
            var key = keyCodes[i];
            if (Input.GetKeyDown(key)) return key;
        }

        return KeyCode.None;
    }

    public static string ToPrettyString(this KeyCode keyCode)
    {
        string keyStr;
        return !_keyCodeToCharMap.TryGetValue(keyCode, out keyStr) ? keyCode.ToString() : keyStr;
    }
}
