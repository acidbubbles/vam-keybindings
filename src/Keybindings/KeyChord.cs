using System;
using System.Text;
using SimpleJSON;
using UnityEngine;

public struct KeyChord
{
    public static readonly KeyChord empty = new KeyChord(KeyCode.None, false, false, false);

    // This is not thread safe
    private static readonly StringBuilder _sb = new StringBuilder();

    public readonly bool ctrl;
    public readonly bool shift;
    public readonly bool alt;
    public readonly KeyCode key;

    public KeyChord(KeyCode key, bool ctrl, bool alt, bool shift)
    {
        this.key = key;
        this.ctrl = ctrl;
        this.alt = alt;
        this.shift = shift;
    }

    public bool IsDown()
    {
        if (key != KeyCode.None && !Input.GetKeyDown(key)) return false;

        if (ctrl != (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) return false;
        if (alt != (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) return false;
        if (shift != (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) return false;

        return true;
    }

    public bool IsActive()
    {
        if (key != KeyCode.None && !Input.GetKey(key)) return false;

        if (ctrl != (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) return false;
        if (alt != (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) return false;
        if (shift != (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) return false;

        return true;
    }

    public override string ToString()
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        if (ctrl) _sb.Append("Ctrl+");
        if (alt) _sb.Append("Alt+");
        if (shift) _sb.Append("Shift+");
        _sb.Append(key.ToPrettyString());
        var result = _sb.ToString();
        _sb.Length = 0;
        return result;
    }

    public JSONNode GetJSON()
    {
        var jc = new JSONClass
        {
            {"key", key.ToString()}
        };

        if (ctrl) jc["ctrl"].AsBool = true;
        if (alt) jc["alt"].AsBool = true;
        if (shift) jc["shift"].AsBool = true;

        return jc;
    }

    public static KeyChord FromJSON(JSONNode jsonNode)
    {
        return new KeyChord(
            (KeyCode) Enum.Parse(typeof(KeyCode), jsonNode["key"].Value),
            jsonNode["ctrl"].AsBool,
            jsonNode["alt"].AsBool,
            jsonNode["shift"].AsBool
        );
    }

    public bool Equals(KeyChord other)
    {
        return ctrl == other.ctrl && shift == other.shift && alt == other.alt && key == other.key;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is KeyChord && Equals((KeyChord) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ctrl.GetHashCode();
            hashCode = (hashCode * 397) ^ shift.GetHashCode();
            hashCode = (hashCode * 397) ^ alt.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) key;
            return hashCode;
        }
    }
}
