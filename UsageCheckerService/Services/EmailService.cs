using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using UsageCheckerService.Models;

namespace UsageCheckerService.Services;

public class EmailService(IOptions<EmailSettingsOptions> options)
{
    private const string BaseUri = "https://api.eu.mailgun.net/v3";
    
    public bool IsEmailEnabled => options.Value.NotificationEnabled;

    public RestResponse SendEmail(string body, FileModel[] files = null)
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
        
        if (files != null)
        {
            foreach (var file in files)
            {
                request.AddFile("attachment", file.Bytes, file.Name);
            }
        }
        
        return client.Execute(request);
    }

}