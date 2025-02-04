using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Wardler;

internal partial class Vehicles
{
    public static async Task<string> FetchGitHubData(string owner, string repoName, string path, string githubToken)
    {
        string apiUrl = "https://api.github.com/graphql";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:134.0) Gecko/20100101 Firefox/134.0"); // Important!

            string query = @"query RepoTreeQuery($owner: String!, $name: String!, $path: String!) {
  repository(owner: $owner, name: $name) {
    id
    defaultBranchRef {
      name
      id
      __typename
    }
    object(expression: $path) {
      id
      ...TreeFields
      __typename
    }
    __typename
  }
}

fragment TreeFields on Tree {
  id
  entries {
    ...TreeEntryFields
    __typename
  }
}

fragment TreeEntryFields on TreeEntry {
  name
  type
  path
  oid
}";

            string variables = $"{{\"owner\":\"{owner}\",\"name\":\"{repoName}\",\"path\":\"master:{path}\"}}";

            string postData = $"{{\"query\":\"{query.Replace("\n", "\\n").Replace("\"", "\\\"")}\",\"operationName\":\"RepoTreeQuery\",\"variables\":{variables}}}";


            var content = new StringContent(postData, Encoding.UTF8, "application/json");

            using (var response = await client.PostAsync(apiUrl, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(data);
                    return data;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.WriteLine(await response.Content.ReadAsStringAsync()); // Print error details
                    return null;
                }
            }
        }
    }

    public static async Task Main(string[] args)
    {
        string owner = "gszabi99";
        string repoName = "War-Thunder-Datamine";
        string path = "aces.vromfs.bin_u/gamedata/units/tankmodels";
        string githubToken = "YOUR_GITHUB_TOKEN"; // Replace with your actual token

        string jsonData = await FetchGitHubData(owner, repoName, path, githubToken);

        if (jsonData != null)
        {
            Console.WriteLine(jsonData);
        }

        Console.ReadKey();
    }
}