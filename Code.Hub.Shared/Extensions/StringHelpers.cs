namespace Code.Hub.Shared.Extensions
{
    public static class StringHelpers
    {
        public static string GetTextSummary(string fullText, int breakPoint)
        {
            return (fullText.Length < breakPoint) ? fullText : fullText.Substring(0, breakPoint) + "...";
        }
    }
}
