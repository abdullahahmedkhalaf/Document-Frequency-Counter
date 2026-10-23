using System.Text.RegularExpressions;

namespace Document_Frequency_Counter.src
{
    public static class Cleaner
    {
        //We clean all the files at the same time (Saves a lot of time)
        public static async Task<string[]?[]> CleanTexts(CancellationToken token, params string?[] text) =>
            await Task.WhenAll(text.Select(t => Task.Run(() => CleanText(t, token))));

        //We clean every file on it own
        private static string[] CleanText(string? text, CancellationToken token)
        {
            //We check if the text is empty (means that the file was empty) then we skip it entirely
            if (string.IsNullOrEmpty(text)) return [];

            //We make all the text to lower case so that there will be any difference between something like (Hello, hello, HELLO) and we use ToLowerInvariant() so that there would't be any conflict with other languages like the Turkish issue (I => i but without the dot) and this is a huge problem that this method solve
            text = text.ToLowerInvariant();
            //We use the regular expression to filter anything that isn't (Alphabetic, numeric, _ in the word itself, ' in the word itself, and - in the word itself)
            token.ThrowIfCancellationRequested();
            var tokens = Regex.Split(text, @"[^\w'-]+");

            //We check at the end if the regular expression has passed any empty text if so then filter it
            return [.. tokens.Where(t => !string.IsNullOrEmpty(t))];
        }
    }
}