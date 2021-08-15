public static partial class SuperControllerExtensions
{
    public static void OpenTab(this SuperController sc, Atom selectedAtom, string tabName)
    {
        if (tabName == null) return;

        sc.SelectController(selectedAtom.mainController);
        sc.activeUI = SuperController.ActiveUI.SelectedOptions;

        sc.ShowMainHUDAuto();

        var selector = selectedAtom.gameObject.GetComponentInChildren<UITabSelector>(true);
        if (selector == null) return;

        selector.SetActiveTab(tabName);
    }
}
