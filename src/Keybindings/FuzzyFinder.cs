using System.Collections.Generic;
using System.Text;

public class FuzzyFinder
{
    private struct FuzzyMatch
    {
        public string value;
        public int score;
    }

    public string current => _matches.Count > 0 ? _matches[tabIndex].value : null;

    private List<string> _valuesReference;
    private readonly StringBuilder _colorizedStringBuilder = new StringBuilder();
    private readonly List<FuzzyMatch> _matches = new List<FuzzyMatch>();
    private string _lastQuery;
    public int tabIndex { get; set; }
    public int matches => _matches.Count;

    public void Init(List<string> values)
    {
        _valuesReference = values;
    }

    public bool FuzzyFind(string query)
    {
        if (string.IsNullOrEmpty(query))
            return false;

        if (query == _lastQuery) return _matches.Count > 0;
        _lastQuery = query;

        tabIndex = 0;
        _matches.Clear();

        // TODO: Optimize
        // TODO: Keep track of the results subset so we can accelerate fuzzy finding
        for (var i = 0; i < _valuesReference.Count; i++)
        {
            var value = _valuesReference[i];
            if (value.Length < query.Length) continue;
            var score = -value.Length * 2; // The shorter the better
            var queryIndex = 0;
            for (var valueIndex = 0; valueIndex < value.Length; valueIndex++)
            {
                var queryChar = query[queryIndex];
                var valueChar = value[valueIndex];
                var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(valueChar) : queryChar == valueChar;
                if (!isMatch)
                {
                    score--; // Clumped matches are better
                    continue;
                }

                queryIndex++;
                if (queryIndex <= query.Length - 1) continue;
                _matches.Add(new FuzzyMatch{ value = value, score = score});
                break;
            }
        }

        _matches.Sort((x, y) => y.score.CompareTo(x.score));

        return _matches.Count > 0;
    }

    public string ColorizeMatch(string commandName, string query)
    {
        var lengthWithColors = commandName.Length + query.Length * 20;
        if (_colorizedStringBuilder.Capacity < lengthWithColors)
            _colorizedStringBuilder.Capacity = lengthWithColors;

        var queryIndex = 0;
        for (var actionIndex = 0; actionIndex < commandName.Length; actionIndex++)
        {
            if (queryIndex >= query.Length)
            {
                _colorizedStringBuilder.Append(commandName.Substring(actionIndex));
                break;
            }

            // TODO: Same code in RemoteActionsManager, to extract and reuse
            var queryChar = query[queryIndex];
            var actionChar = commandName[actionIndex];
            var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(actionChar) : queryChar == actionChar;

            if (isMatch)
            {
                queryIndex++;
                _colorizedStringBuilder.Append("<color=cyan>");
                _colorizedStringBuilder.Append(commandName[actionIndex]);
                _colorizedStringBuilder.Append("</color>");
                continue;
            }

            _colorizedStringBuilder.Append(commandName[actionIndex]);
        }

        var result = _colorizedStringBuilder.ToString();
        _colorizedStringBuilder.Length = 0;
        return result;
    }

    public void Clear()
    {
        _lastQuery = null;
        tabIndex = 0;
        _matches.Clear();
        _valuesReference = null;
    }
}
