public interface IMap
{
    string commandName { get; set; }
    int slot { get; }
    string GetPrettyString();
}
