namespace mkgig
{
    public static class Repository
    {
        private const string ApiUrl = "https://www.toptal.com/developers/gitignore/api/";

        public static async Task<IEnumerable<string>> GetTemplateNames()
        {
            var templateNames = await InvokeApi("list");
            return templateNames.Split('\n').SelectMany(line => line.Split(',')).ToList();
        }

        public static async Task<string> GetTemplate(string[] names)
        {
            return await InvokeApi(string.Join(',', names));
        }

        private static async Task<string> InvokeApi(string method)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync($"{ApiUrl}{method}").ConfigureAwait(false);
        }
    }
}
