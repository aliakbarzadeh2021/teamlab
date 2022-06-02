namespace System
{
    static class StringExp
    {
        public static string First(this string str, int length)
        {
            return str == null ? null : str.Length <= length ? str : str.Remove(length);
        }
    }
}