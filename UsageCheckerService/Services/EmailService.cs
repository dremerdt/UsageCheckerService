using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace UsageCheckerService.Services;

public class EmailService(IOptions<EmailSettingsOptions> options)
{
    private const string BaseUri = "https://api.eu.mailgun.net/v3";
    
    public bool IsEmailEnabled => options.Value.NotificationEnabled;

    public RestResponse SendEmail(string body)
    {
        var settings = options.Value;
        if (!IsEmailEnabled || settings.EmailsParsed.Length == 0)
        {
            return null;
        }
        var client = new RestClient(new RestClientOptions
        {
            BaseUrl = new Uri(BaseUri),
            Authenticator = new HttpBasicAuthenticator("api", settings.MainGunApiKey)
        });

        var request = new RestRequest();
        request.AddParameter("domain", settings.Domain, ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter("from", $"{settings.SenderDisplayName} <{settings.SenderAddress}>");

        foreach (var toEmail in settings.EmailsParsed)
        {
            request.AddParameter("to", toEmail);
        }

        request.AddParameter("subject", settings.Subject);
        request.AddParameter("html", body);
        request.Method = Method.Post;
        return client.Execute(request);
    }

}