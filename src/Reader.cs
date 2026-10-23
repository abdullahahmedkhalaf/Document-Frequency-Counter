namespace Document_Frequency_Counter.src
{
    public static class Reader
    {
        //Read multiple files at the same time (Saves a lot of time)
        //Here we read the files using the method ReadFileAsync(string file) that gives each file the ability to be read on its own thread
        //Waiting until all of them are finished from processing and return an array with all the words
        public static async Task<string?[]> ReadFilesAsync(CancellationToken token, params string[] files) =>
                        await Task.WhenAll(files.Select(f => ReadFileAsync(f, token)).ToList());

        //We read every file on its own
        private static async Task<string?> ReadFileAsync(string file, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                //We use StreamReader to read the hole file
                using (StreamReader stream = new(file))
                {
                    //We read the file a hole text
                    string lines = stream.ReadToEnd();
                    return lines;
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine(ioEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}