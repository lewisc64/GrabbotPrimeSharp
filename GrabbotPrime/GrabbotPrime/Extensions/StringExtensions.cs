namespace GrabbotPrime.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceIfNullOrEmpty(this string s, string replacement)
        {
            if (string.IsNullOrEmpty(s))
            {
                return replacement;
            }
            return s;
        }
    }
}
