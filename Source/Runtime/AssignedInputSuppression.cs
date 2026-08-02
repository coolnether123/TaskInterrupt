namespace TaskBreak.Runtime
{
    internal static class AssignedInputSuppression
    {
        internal static bool ShouldSuppress(
            bool searchWidgetFocused,
            bool windowAbsorbsAllInput,
            bool nonImmediateDialogOpen)
        {
            return searchWidgetFocused ||
                windowAbsorbsAllInput ||
                nonImmediateDialogOpen;
        }
    }
}
