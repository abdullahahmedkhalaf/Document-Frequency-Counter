namespace Document_Frequency_Counter.src
{
    public static class Handler
    {
        //We process all files at the same time (Parallel)
        public static async Task<(List<KeyValuePair<string, int>> SortedMap, int TotalCount)> GetFrequenciesAsync(CancellationToken token, params string[]?[] tokensArrays)
        {
            //Remove top words
            //Remove the stop words from all the files at the same time
            var filterStopWordsTasks = tokensArrays.Select(StopWords.RemoveStopWordsAsync!).ToList();
            //Get the arrays without the stop words
            token.ThrowIfCancellationRequested();
            var frequencyDicTasks = await Task.WhenAll(filterStopWordsTasks);
            //Count the frequencies
            //Count the frequency of the word in each file on its own at the same time
            var frequencyDic = frequencyDicTasks.Select(GetFrequencyAsync).ToList();
            //Get the Dictionaries of all the files
            token.ThrowIfCancellationRequested();
            var allDic = await Task.WhenAll(frequencyDic);

            //Merge all the dictionaries a.k.a "All files" together sequentially so no one override the other
            //We create a map that will work as the base for all of them and store all the values
            return await Task.Run(() =>
            {
                var map = new Dictionary<string, int>();
                int totalCount = 0;

                foreach (var dic in allDic)
                {
                    //if a dictionary is empty then don't bother and go to the next dictionary
                    if (dic == null) continue;

                    //we get the key and the value of a.k.a (the word and it frequency)
                    foreach (var (key, value) in dic)
                    {
                        //We get the same word from the main dictionary so that we can modify it and if it wasn't there we add it
                        //So that we use TryGetValue(T key, out T value)
                        //if the key in the dictionary then get me its value if not then handel it and make the value = 0
                        map.TryGetValue(key, out int current);
                        //We add the original value with the new one like that we merged two dictionaries
                        map[key] = current + value;
                        totalCount += value;
                    }
                }

                var sortedMap = map.OrderByDescending(x => x.Value).ToList();

                //Sort and return
                return (sortedMap, totalCount);
            });
        }

        //We get the Frequency of each file on its own
        private static async Task<Dictionary<string, int>> GetFrequencyAsync(string[]? tokens)
        {
            //if the file was empty then return an empty array
            if (tokens?.Length == 0 || tokens == null) return [];

            return await Task.Run(() =>
            {
                //We make a dictionary to hold the word and there frequencies
                var localMap = new Dictionary<string, int>();
                //We iterate through all tokens
                foreach (var token in tokens)
                {
                    //We add a safe measurement if there was an empty tokens or and underscore or apostrophe or dash (this has to do with the cleaner) then skip those
                    if (IsNullOrEmptyOrUnderscoreOrHyphenOrDash(token)) continue;
                    //We check if the word is in the dictionary already or no if it is in when increment its value by one if not then add it and make its default vale 1
                    if (!localMap.TryAdd(token, 1)) localMap[token] += 1;
                }

                return localMap;
            });
        }

        // We process all files at the same time (Parallel)
        public static async Task<(List<KeyValuePair<string, int>>, int)> GetNGramsAsync(CancellationToken token, params string[]?[] tokensArrays)
        {
            //Remove top words
            //Remove the stop words from all the files at the same time
            var filterStopWordsTasks = tokensArrays.Select(StopWords.RemoveStopWordsAsync!).ToList();
            //Get the arrays without the stop words
            token.ThrowIfCancellationRequested();
            var tasks = await Task.WhenAll(filterStopWordsTasks);
            //Get the NGrams for the files and the number of them in each file
            var nGramsTasks = tasks.Select(GetNGramAsync).ToList();
            //Get the dictionary fo all the files
            token.ThrowIfCancellationRequested();
            var nGrams = await Task.WhenAll(nGramsTasks);

            //Merge all the dictionaries a.k.a "All files" together sequentially so no one override the other
            //We create a map that will work as the base for all of them and store all the values
            return await Task.Run(() =>
            {
                var map = new Dictionary<string, int>();
                int totalCount = 0;

                foreach (var nGram in nGrams)
                {
                    //if a dictionary is empty then don't bother and go to the next dictionary
                    if (nGram == null) continue;

                    //we get the key and the value of a.k.a (the word and it frequency)
                    foreach (var (key, value) in nGram)
                    {
                        //We get the same word from the main dictionary so that we can modify it and if it wasn't there we add it
                        //So that we use TryGetValue(T key, out T value)
                        //if the key in the dictionary then get me its value if not then handel it and make the value = 0
                        map.TryGetValue(key, out int current);
                        //We add the original value with the new one like that we merged two dictionaries
                        map[key] = current + value;
                        totalCount += value;
                    }
                }

                var sortedMap = map.OrderByDescending(x => x.Value).ToList();

                //Sort and return
                return (sortedMap, totalCount);
            });
        }

        //We get the NGrams for each file on its own
        private static async Task<Dictionary<string, int>> GetNGramAsync(string?[] tokens)
        {
            //if the file was empty then return an empty array
            if (tokens?.Length == 0 || tokens == null) return [];

            return await Task.Run(() =>
            {

                //We make a dictionary to hold the word and there frequencies
                var localMap = new Dictionary<string, int>();

                //We iterate through all tokens
                for (int i = 0; i < tokens.Length; i++)
                {
                    //if the next index is the same size as the array then we reached the last element so we fo out of the loop
                    if (i + 1 == tokens.Length) break;

                    //We add a safe measurement if there was an empty tokens or and underscore or apostrophe or dash (this has to do with the cleaner) then skip those
                    if (IsNullOrEmptyOrUnderscoreOrHyphenOrDash(tokens[i])) continue;

                    //We create the nGram from the current token and the token that after it
                    var nGram = $"{tokens[i]} {tokens[i + 1]}";
                    //We check if the nGram is in the dictionary already or no if it is in when increment its value by one if not then add it and make its default vale 1
                    if (!localMap.TryAdd(nGram, 1)) localMap[nGram] += 1;
                }

                return localMap;
            });
        }

        private static bool IsNullOrEmptyOrUnderscoreOrHyphenOrDash(string? token) => string.IsNullOrEmpty(token) || token.Equals("-") || token.Equals("_") || token.Equals("'");
    }
}