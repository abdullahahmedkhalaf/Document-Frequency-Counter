namespace Document_Frequency_Counter.src
{
    public static class StopWords
    {
        //Stop words...Duh😂
        private static readonly HashSet<string> _stopWords =
        [
            "a", "an", "and", "of", "to", "is", "are", "was", "were", "be",
            "been", "being", "have", "has", "had", "having", "for", "with", "on", "at",
            "by", "in", "out", "up", "down", "from", "or", "but", "so", "if",
            "then", "than", "that", "this","these", "those", "it", "they", "we", "you",
            "he", "she", "it", "i", "as", "will", "would", "its", "can", "could",
            "can't", "couldn't", "may", "might", "when", "what", "who", "how", "where", "while",
            "whom", "why", "which"
        ];

        //A helper method that returns true if the word is a stop word
        public static bool IsStopWords(string word) => _stopWords.Contains(word);

        //We remove any stop words from the tokens arrays each one on its own
        public static async Task<string[]> RemoveStopWordsAsync(string[] tokens)
        {
            //If the array is empty a.k.a (the file itself is empty) then return an empty array
            if (tokens == null || tokens.Length == 0) return [];

            //We filter the array from any stop words at the end
            return await Task.Run(() => tokens.Where(token => !IsStopWords(token)).ToArray());
        }
    }
}