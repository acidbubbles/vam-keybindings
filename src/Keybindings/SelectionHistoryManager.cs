using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ISelectionHistoryManager
{
    IList<Atom> history { get; }
    Atom GetLastSelectedAtomOfType(string type);
}

public class SelectionHistoryManager : ISelectionHistoryManager
{
    private int _lastValidCheck = Time.frameCount;
    private readonly List<Atom> _history = new List<Atom>();
    public readonly Dictionary<Atom, MVRScript> _latestScriptPerAtom = new Dictionary<Atom, MVRScript>();

    public IList<Atom> history
    {
        get
        {
            if (_lastValidCheck == Time.frameCount)
                return _history;

            _lastValidCheck = Time.frameCount;
            for (var i = 0; i < _history.Count; i++)
            {
                if (_history[i] != null) continue;
                _history.RemoveAt(i);
                i--;
            }

            return _history;
        }
    }

    public void Update()
    {
        var atom = SuperController.singleton.GetSelectedAtom();
        if (ReferenceEquals(atom, null)) return;
        if (_history.Count > 0)
        {
            var current = _history[_history.Count - 1];
            if (current == atom) return;
            _history.Remove(atom);
        }
        _history.Add(atom);
    }

    public Atom GetLastSelectedAtomOfType(string type)
    {
        for (var i = _history.Count - 1; i >= 0; i--)
        {
            var atom = _history[i];
            if (atom == null)
            {
                _history.RemoveAt(i);
                i--;
                continue;
            }
            if (type == null || atom.type == type)
                return atom;
        }

        if (type == null)
            return SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == "Person") ?? SuperController.singleton.GetAtoms().FirstOrDefault();

        return SuperController.singleton.GetAtoms().FirstOrDefault(a => a.type == type);
    }

    public void SetLatestScriptPerAtom(MVRScript script)
    {
        _latestScriptPerAtom[script.containingAtom] = script;
    }

    public MVRScript GetLatestScriptPerAtom(Atom atom)
    {
        MVRScript script;
        return _latestScriptPerAtom.TryGetValue(atom, out script) ? script : null;
    }

    public void Clear(JSONStorable storable)
    {
        var atom = _latestScriptPerAtom.FirstOrDefault(kvp => kvp.Value == storable).Key;
        if(!ReferenceEquals(atom, null))
            Clear(atom);
    }

    public void Clear(Atom atom)
    {
        _latestScriptPerAtom.Remove(atom);
    }
}
