using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fast.Services;

public class FastmailService
{
    private readonly string _token = "#APIKEY";
    private readonly string _sessionUrl = "jmap/session";

    public FastmailService()
    {
    }

    public async Task<string> SetMaskedEmail(string description)
    {
        try
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.fastmail.com/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            var result = await client.GetAsync(_sessionUrl);
            result.EnsureSuccessStatusCode();
            var response = await result.Content.ReadAsStringAsync();
            
            var json = JsonNode.Parse(response);
            var accountId = json?["primaryAccounts"]?["https://www.fastmail.com/dev/maskedemail"]?.ToString();
            var apiUrl = json?["apiUrl"]?.ToString();
            
            //Console.WriteLine($"Account Id: {accountId}, API Url: {apiUrl}");
            
            var requestJObj = new JObject
            {
                new JProperty("using",
                    new JArray { "https://www.fastmail.com/dev/maskedemail", "urn:ietf:params:jmap:core" }),
                new JProperty("methodCalls",
                    new JArray
                    {
                        new JArray
                        {
                            "MaskedEmail/set",
                            new JObject
                            {
                                ["accountId"] = accountId,
                                ["create"] = new JObject
                                {
                                    ["new-masked-email"] = new JObject
                                    {
                                        ["state"] = "enabled",
                                        ["description"] = description,
                                        ["url"] = "",
                                        ["emailPrefix"] = ""
                                    }
                                }
                            },
                            "0"
                        }
                    })
            };
            
            var content = new StringContent(requestJObj.ToString(), Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiUrl ?? string.Empty),
                Content = content
            };
            
            var maskedEmailMessage = await client.SendAsync(request);
            maskedEmailMessage.EnsureSuccessStatusCode();
            var maskedEmailResponse = await maskedEmailMessage.Content.ReadAsStringAsync();
            
            dynamic maskedEmailJson = JsonConvert.DeserializeObject(maskedEmailResponse) ?? throw new InvalidOperationException();
            var maskedEmail = maskedEmailJson.methodResponses[0][1].created["new-masked-email"].email;
            
            return maskedEmail;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

}