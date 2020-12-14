using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ISelectionHistoryManager
{
    IList<Atom> history { get; }
    Atom GetLastSelectedAtomOfType(string type);
}

public class SelectionHistoryManager : MonoBehaviour, ISelectionHistoryManager
{
    private int _lastValidCheck = Time.frameCount;
    private readonly List<Atom> _history = new List<Atom>();
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
}
