namespace Expense_Tracker.Application.Constants;

public static class Topics
{
    public static string getFamilyTopic(Guid familyId)
    {

        return $"family-{familyId}";

    }
}
