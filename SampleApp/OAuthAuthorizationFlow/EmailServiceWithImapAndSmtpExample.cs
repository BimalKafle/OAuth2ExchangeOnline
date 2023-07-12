using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using SampleApp.Models;

namespace SampleApp.OAuthAuthorizationFlow
{
    public class EmailServiceWithImapAndSmtpExample
    {
        public async Task<MailViewModel?> GetMessageDetail(int id)
        {
            //check the authentication controller
            var mail = new MailViewModel();
            using (var client = new ImapClient())
            {
                // For demo purposes, accept all SSL certificates
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(EmailConfiguration.ImapServer, EmailConfiguration.ImapPort, MailKit.Security.SecureSocketOptions.SslOnConnect);

                // Use the access token to authenticate with the server
                //var oauth2 = new SaslMechanismOAuth2(accessToken.UserName, accessToken.AccessToken);
                //await client.AuthenticateAsync(oauth2);

                // Open the INBOX folder
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                // Get the UID of the email you want to mark as read or unread
                var messageUid = id; // Replace with the actual UID of the email

                // Retrieve the message based on its UID
                var message = inbox.GetMessage(messageUid);

                mail.To = message.To.ToString(); mail.Subject = message.Subject;
                mail.From = message.From.ToString();
                mail.SentDate = message.Date;
                mail.Body = message.TextBody;





                // Disconnect from the IMAP server
                client.Disconnect(true);
            }
            return mail;
        }

        public async Task MarkAsRead(int id)
        {
            using (var client = new ImapClient())
            {
                // For demo purposes, accept all SSL certificates
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(EmailConfiguration.ImapServer, EmailConfiguration.ImapPort, SecureSocketOptions.SslOnConnect);

                // Use the access token to authenticate with the server
                //var oauth2 = new SaslMechanismOAuth2(accessToken.UserName, accessToken.AccessToken);
                //await client.AuthenticateAsync(oauth2);

                // Open the INBOX folder
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                // Get the UID of the email you want to mark as read or unread
                var messageUid = id; // Replace with the actual UID of the email

                // Retrieve the message based on its UID
                var message = inbox.GetMessage(messageUid);


                inbox.AddFlags(messageUid, MessageFlags.Seen, true);




                // Disconnect from the IMAP server
                client.Disconnect(true);
            }
        }

        public async Task MarkAsUnread(int id)
        {

            using (var client = new ImapClient())
            {
                // For demo purposes, accept all SSL certificates
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(EmailConfiguration.ImapServer, EmailConfiguration.ImapPort, SecureSocketOptions.SslOnConnect);

                //// Use the access token to authenticate with the server
                //var oauth2 = new SaslMechanismOAuth2(accessToken.UserName, accessToken.AccessToken);
                //await client.AuthenticateAsync(oauth2);

                // Open the INBOX folder
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                // Get the UID of the email you want to mark as read or unread
                var messageUid = id; // Replace with the actual UID of the email

                // Retrieve the message based on its UID
                var message = inbox.GetMessage(messageUid);





                inbox.RemoveFlags(messageUid, MessageFlags.Seen, true);

                // Disconnect from the IMAP server
                client.Disconnect(true);
            }
        }

        public async Task<List<Mail>> ReceiveInboxMail()
        {
            var InboxMessgeList = new List<Mail>();
            using (var client = new ImapClient())
            {
                // For demo purposes, accept all SSL certificates
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(EmailConfiguration.ImapServer, EmailConfiguration.ImapPort, SecureSocketOptions.SslOnConnect);

                // Use the access token to authenticate with the server
                //var oauth2 = new SaslMechanismOAuth2(accessToken.UserName, accessToken.AccessToken);
                //await client.AuthenticateAsync(oauth2);

                // Get the INBOX mailbox
                var inbox = client.Inbox;
                await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

                var messageSummaries = await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure);

                foreach (var summary in messageSummaries)
                {
                    var mail = new Mail();
                    mail.Id = summary.Index;
                    mail.MessageReadStatus = (summary.Flags & MessageFlags.Seen) != 0;
                    mail.Email = summary.Envelope.From.ToString();
                    mail.Subject = summary.Envelope.Subject.ToString();
                    InboxMessgeList.Add(mail);
                }
                // Fetch sent messages



                await client.DisconnectAsync(true);
            }
            return InboxMessgeList;
        }

        public async Task<List<Mail>> ReceiveSentMail()
        {

            var SentMessgeList = new List<Mail>();
            using (var client = new ImapClient())
            {
                // For demo purposes, accept all SSL certificates
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(EmailConfiguration.ImapServer, EmailConfiguration.ImapPort, SecureSocketOptions.SslOnConnect);

                // Use the access token to authenticate with the server
                //var oauth2 = new SaslMechanismOAuth2(accessToken.UserName, accessToken.AccessToken);
                //await client.AuthenticateAsync(oauth2);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                // Search for sent messages
                var sentMessageIds = await client.Inbox.SearchAsync(SearchQuery.SentBefore(DateTime.Now));

                // Fetch the sent messages
                var sentMessages = await client.Inbox.FetchAsync(sentMessageIds, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope);





                foreach (var summary in sentMessages)
                {
                    var mail = new Mail();
                    Console.WriteLine("Sent Message:");
                    mail.Email = summary.Envelope.To.ToString();
                    mail.Subject = summary.Envelope.Subject.ToString();

                    SentMessgeList.Add(mail);
                }


                await client.DisconnectAsync(true);
            }
            return SentMessgeList;
        }

        public async Task SendMail(MailModel mail)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sender Name", userName));
            message.To.Add(new MailboxAddress("Recipient Name", mail.To));
            message.Subject = mail.Subject;
            message.Body = new TextPart("plain")
            {
                Text = mail.Body
            };
            if (mail.MailCc != null)
            {
                message.Cc.Add(new MailboxAddress("CC Recipient Name", mail.MailCc));
            }
            if (mail.MailBcc != null)
            {
                message.Bcc.Add(new MailboxAddress("BCC Recipient Name", mail.MailBcc));
            }



            // Send the email using MailKit with the access token as a bearer token
            using (var client = new SmtpClient())
            {
                client.Connect(EmailConfiguration.SmtpServer, EmailConfiguration.SmtpPort, SecureSocketOptions.StartTls);
                //client.Authenticate(new SaslMechanismOAuth2(userName, accessToken.AccessToken));
                client.Send(message);
                client.Disconnect(true);
            }



        }
    }
}
