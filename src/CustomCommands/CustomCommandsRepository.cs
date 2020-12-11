using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.Events;

namespace CustomActions
{
    public interface ICustomCommandsRepository : IEnumerable
    {
        UnityEvent onChange { get; }
        ICustomCommand AddDiscreteTrigger();
        void Remove(ICustomCommand command);
    }

    public class CustomCommandsRepository : ICustomCommandsRepository
    {
        public UnityEvent onChange { get; } = new UnityEvent();
        public int count => _commands.Count;

        private readonly Atom _containingAtom;
        private readonly IPrefabManager _prefabManager;
        private readonly List<ICustomCommand> _commands = new List<ICustomCommand>();

        public CustomCommandsRepository(Atom containingAtom, IPrefabManager prefabManager)
        {
            _containingAtom = containingAtom;
            _prefabManager = prefabManager;
        }

        public ICustomCommand AddDiscreteTrigger()
        {
            var command = new DiscreteTriggerCommand(_containingAtom, _prefabManager);
            _commands.Add(command);
            onChange.Invoke();
            return command;
        }

        public void Remove(ICustomCommand command)
        {
            _commands.Remove(command);
            onChange.Invoke();
        }

        public void Validate()
        {
            foreach (var command in _commands)
                command.Validate();
        }

        public void SyncAtomNames()
        {
            foreach (var command in _commands)
                command.SyncAtomNames();
        }

        public JSONNode GetJSON()
        {
            var commandsJSON = new JSONArray();
            foreach (var command in _commands)
            {
                var commandJSON = command.GetJSON();
                if (commandJSON == null) continue;
                commandJSON["__type"] = command.type;
                commandsJSON.Add(commandJSON);
            }

            return commandsJSON;
        }

        public void RestoreFromJSON(JSONNode commandsJSON)
        {
            if ((commandsJSON?.Count ?? 0) == 0) return;
            _commands.Clear();
            foreach (JSONClass commandJSON in commandsJSON.AsArray)
            {
                ICustomCommand action;
                var commandType = commandJSON["__type"];
                switch (commandType)
                {
                    case DebugCommand.Type:
                        action = new DebugCommand();
                        break;
                    case DiscreteTriggerCommand.Type:
                        action = new DiscreteTriggerCommand(_containingAtom, _prefabManager);
                        break;
                    default:
                        SuperController.LogError($"Unknown command type {commandType}");
                        continue;
                }

                action.RestoreFromJSON(commandJSON);
                _commands.Add(action);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _commands.GetEnumerator();
        }
    }
}
