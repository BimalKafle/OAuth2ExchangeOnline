using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using SampleApp.Models;

namespace SampleApp.GraphAPi
{
    public class GraphAPiExample
    {
        public static readonly string clientId = "yourClientId";
        public static readonly string tenantId = "yourTenatId";

        public static readonly string clientSecret = "clientSecret";
        // Microsoft Graph API scopes for accessing Exchange Online
        string[] scopes = { "https://graph.microsoft.com/.default" };


        public async Task<List<MailModel>> Index()
        {
            var mailList = new List<MailModel>();
            try
            {
                var options = new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                };

                // https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
                var clientSecretCredential = new ClientSecretCredential(
                    tenantId, clientId, clientSecret, options);

                var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

                var mailboxes = await graphClient.Users.GetAsync();

                // Retrieve inbox for each mailbox
                foreach (var mailbox in mailboxes.Value)
                {
                    if (mailbox.Mail != "Email")
                    {
                        var userEmailAddress = mailbox.Mail;
                        var inboxMessages = await graphClient.Users[userEmailAddress].Messages
                            .GetAsync();

                        foreach (var message in inboxMessages.Value)
                        {
                            var emailData = new MailModel();
                            emailData.From = message.Sender.EmailAddress.Address;
                            emailData.Subject = message.Subject;
                            emailData.To = userEmailAddress;
                            emailData.Body = message.Body.Content;
                            mailList.Add(emailData);
                        }
                    }

                }

                // Get user's messages
                //var messages = await graphClient.Users["email"].Messages.GetAsync(x =>
                //{
                //    x.QueryParameters.Top = 100;
                //});

                //foreach (var message in messages.Value)
                //{
                //    var emailData = new MailModel();
                //    emailData.From = message.Sender.EmailAddress.Address;
                //    emailData.Subject = message.Subject;

                //    emailData.Body = message.Body.Content;
                //    mailList.Add(emailData);
                //}
                return mailList;
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve emails: {ex.Message}");
            }
            return mailList;
        }
    }
}
