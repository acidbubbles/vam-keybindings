public static partial class SuperControllerExtensions
{
    public static void OpenTab(this SuperController sc, Atom selectedAtom, string tabName)
    {
        if (tabName == null) return;

        sc.SelectController(selectedAtom.mainController);
        sc.SetActiveUI("SelectedOptions");

        sc.ShowMainHUD();

        // TODO: Wait for the UI to be available

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>(true);
        if (selector == null) return;

        selector.SetActiveTab(tabName);
    }
}
