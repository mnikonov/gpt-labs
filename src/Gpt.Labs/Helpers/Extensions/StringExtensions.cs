namespace Gpt.Labs.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ConvertCrLfToLf(this string value)
        {
            return value.Replace("\r\n", "\n");
        }
    }
}
