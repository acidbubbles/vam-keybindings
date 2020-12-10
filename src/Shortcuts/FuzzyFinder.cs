using System.Collections.Generic;
using System.Text;

public class FuzzyFinder
{
    private List<string> _values;
    private readonly StringBuilder _colorizedStringBuilder = new StringBuilder();

    public void Init(List<string> values)
    {
        _values = values;
    }

    public string FuzzyFind(string query)
    {
        if (string.IsNullOrEmpty(query))
            return null;

        // TODO: Optimize
        // TODO: Keep track of the results subset so we can accelerate fuzzy finding
        foreach (var value in _values)
        {
            if(value.Length < query.Length) continue;
            var queryIndex = 0;
            for(var valueIndex = 0; valueIndex < value.Length; valueIndex++)
            {
                var queryChar = query[queryIndex];
                var valueChar = value[valueIndex];
                var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(valueChar) : queryChar == valueChar;
                if (!isMatch) continue;

                queryIndex++;
                if (queryIndex > query.Length - 1)
                    return value;
            }
        }
        return null;
    }

    public string ColorizeMatch(string action, string query)
    {
        var lengthWithColors = action.Length + query.Length * 20;
        if (_colorizedStringBuilder.Capacity < lengthWithColors)
            _colorizedStringBuilder.Capacity = lengthWithColors;

        var queryIndex = 0;
        for (var actionIndex = 0; actionIndex < action.Length; actionIndex++)
        {
            if (queryIndex >= query.Length)
            {
                _colorizedStringBuilder.Append(action.Substring(actionIndex));
                break;
            }

            // TODO: Same code in RemoteActionsManager, to extract and reuse
            var queryChar = query[queryIndex];
            var actionChar = action[actionIndex];
            var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(actionChar) : queryChar == actionChar;

            if (isMatch)
            {
                queryIndex++;
                _colorizedStringBuilder.Append("<color=cyan>");
                _colorizedStringBuilder.Append(action[actionIndex]);
                _colorizedStringBuilder.Append("</color>");
                continue;
            }

            _colorizedStringBuilder.Append(action[actionIndex]);
        }

        var result = _colorizedStringBuilder.ToString();
        _colorizedStringBuilder.Length = 0;
        return result;
    }

    public void Clear()
    {
        _values = null;
    }
}
