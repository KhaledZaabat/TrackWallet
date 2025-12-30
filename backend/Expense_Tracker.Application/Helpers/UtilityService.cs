namespace Expense_Tracker.Application.Helpers
{
    public static class UtilityService
    {
        public static string MaskEmail(string email)
        {
            int atIndex = email.IndexOf('@');
            if (atIndex <= 1)
            {
                return $"****{email.AsSpan(atIndex)}";
            }

            return email[0] + "****" + email[atIndex - 1] + email[atIndex..];
        }

        public static string MaskPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;


            phone = phone.Replace(" ", "").Replace("-", "");


            if (phone.Length <= 4)
                return "****";


            string last2 = phone[^2..];
            var masked = new string('*', phone.Length - 2);

            return masked + last2;
        }
    }
}
