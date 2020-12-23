using System;
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

        for (var i = 0; i < _valuesReference.Count; i++)
        {
            var value = _valuesReference[i];
            if (value.Length < query.Length) continue;
            int score;
            if (!DoFuzzyMatch(value, query, out score))
                continue;
            _matches.Add(new FuzzyMatch {score = score, value = value});
        }

        _matches.Sort((x, y) => y.score.CompareTo(x.score));

        return _matches.Count > 0;
    }

    // https://gist.github.com/CDillinger/2aa02128f840bdca90340ce08ee71bc2
    private static bool DoFuzzyMatch(string stringToSearch, string pattern, out int outScore)
	{
		const int adjacencyBonus = 10;               // bonus for adjacent matches
		const int separatorBonus = 20;              // bonus if match occurs after a separator
		const int camelBonus = 10;                  // bonus if match is uppercase and prev is lower

        const int lengthPenalty = 1;                // penalty applied for every letter after the first separator
		const int leadingLetterPenalty = 1;         // penalty applied for every letter in stringToSearch before the first match
		const int maxLeadingLetterPenalty = 10;      // maximum penalty for leading letters
		const int unmatchedLetterPenalty = -1;      // penalty for every letter that doesn't matter

		// Loop variables
		var score = 0;
		var patternIdx = 0;
		var patternLength = pattern.Length;
		var strIdx = 0;
		var strLength = stringToSearch.Length;
        var firstSeparatorIdx = 0;
		var prevMatched = false;
		var prevLower = false;
		var prevSeparator = true; // true if first letter match gets separator bonus

		// Use "best" matched letter if multiple string letters match the pattern
		var bestLetter = (char)0;
		var bestLower = (char)0;
		var bestLetterScore = 0;

		// Loop over strings
		while (strIdx != strLength)
		{
			var patternChar = patternIdx != patternLength ? pattern[patternIdx] as char? : (char) 0;
			var strChar = stringToSearch[strIdx];

			var patternLower = patternChar != 0 ? char.ToLower((char)patternChar) : (char) 0;
			var strLower = char.ToLower(strChar);
			var strUpper = char.ToUpper(strChar);

			var nextMatch = patternChar != 0 && patternLower == strLower;
			var rematch = bestLetter != 0 && bestLower == strLower;

			var advanced = nextMatch && bestLetter != 0;
			var patternRepeat = bestLetter != 0 && patternChar != 0 && bestLower == patternLower;
			if (advanced || patternRepeat)
			{
				score += bestLetterScore;
                bestLetter = (char) 0;
                bestLower = (char) 0;
				bestLetterScore = 0;
			}

            if (firstSeparatorIdx == 0) firstSeparatorIdx = strIdx;

			if (nextMatch || rematch)
			{
				var newScore = 0;

				// Apply penalty for each letter before the first pattern match
				// Note: Math.Max because penalties are negative values. So max is smallest penalty.
				if (patternIdx == 0 && firstSeparatorIdx > 0)
				{
					var penalty = Math.Max(strIdx * leadingLetterPenalty, maxLeadingLetterPenalty);
					score += penalty;
				}

				// Apply bonus for consecutive bonuses
				if (prevMatched)
					newScore += adjacencyBonus;

				// Apply bonus for matches after a separator
				if (prevSeparator)
					newScore += separatorBonus;

				// Apply bonus across camel case boundaries. Includes "clever" isLetter check.
				if ((prevLower || prevSeparator) && strChar == strUpper && strLower != strUpper)
					newScore += camelBonus;

				// Update pattern index IF the next pattern letter was matched
				if (nextMatch)
					++patternIdx;

				// Update best letter in stringToSearch which may be for a "next" letter or a "rematch"
				if (newScore >= bestLetterScore)
				{
					// Apply penalty for now skipped letter
					if (bestLetter != 0)
						score += unmatchedLetterPenalty;

					bestLetter = strChar;
					bestLower = char.ToLower(bestLetter);
					bestLetterScore = newScore;
				}

				prevMatched = true;
			}
			else
			{
				score += unmatchedLetterPenalty;
				prevMatched = false;
			}

			// Includes "clever" isLetter check.
			prevLower = strChar == strLower && strLower != strUpper;
			prevSeparator = strChar == '_' || strChar == '.';

			++strIdx;
		}

		// Apply score for last match
		if (bestLetter != 0)
		{
			score += bestLetterScore;
		}

		outScore = score - (strLength - firstSeparatorIdx) * lengthPenalty;
		return patternIdx == patternLength;
	}

    // TODO: Almost the same implementation as the method before
    public static bool Match(string value, string query)
    {
        if (query.Length == 0) return true;
        if (value.Length < query.Length) return false;
        var queryIndex = 0;
        for (var valueIndex = 0; valueIndex < value.Length; valueIndex++)
        {
            var queryChar = query[queryIndex];
            var valueChar = value[valueIndex];
            var isMatch = char.IsLower(queryChar) ? queryChar == char.ToLowerInvariant(valueChar) : queryChar == valueChar;
            if (!isMatch)
                continue;

            queryIndex++;
            if (queryIndex <= query.Length - 1) continue;
            return true;
        }
        return false;
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
