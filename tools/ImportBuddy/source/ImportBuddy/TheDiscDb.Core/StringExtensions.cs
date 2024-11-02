namespace TheDiscDb
{
    using System.Text;

    public static class StringExtensions
    {
        const char Dash = '-';

        public static string Slugify(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder s = new();

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == Dash)
                {
                    s.Append(char.ToLower(c));
                }
                else if (char.IsWhiteSpace(c))
                {
                    s.Append(Dash);
                }
                else if (c == '&')
                {
                    s.Append("and");
                }
            }

            return s.ToString();
        }
    }
}
