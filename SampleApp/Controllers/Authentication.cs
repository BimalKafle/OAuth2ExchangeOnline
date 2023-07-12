using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using SampleApp.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SampleApp.Controllers
{
    public class Authentication : Controller
    {
        private string codeVerifier;
        private string codeChallenge;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static string codeVerifierFilePath;
        private static string codeChallengeFilePath;
    
        public Authentication(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            codeVerifierFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "CodeVerifier.txt");
        
        }

        public async Task Index()
        {
            var url = CreateAuthorizationCodeRequestUrl();
            Response.Redirect(url);
        }


        private bool ValidateToken(DateTime expiresOn)
        {
            if (DateTimeOffset.UtcNow >= expiresOn)
            {
                return true;
            }

            return false;
        }
        public async Task<IActionResult> ExchangeCodeForAccessToken(string code)
        {
            try
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
                  .Create("clientId")
                  .WithClientSecret("clientSecret")
                  .WithRedirectUri("RedirectUrl")
                  .WithAuthority("Authority")
                  .Build();
                string codeVerifier = GetCodeVerifier();

                AuthenticationResult result = await app.AcquireTokenByAuthorizationCode(/*scope*/, code).WithPkceCodeVerifier(codeVerifier).ExecuteAsync();
                AccessTokenModel model = new();
                model.AccessToken = result.AccessToken;
                model.UserName = result.Account.Username;
                model.Token_type = result.TokenType;
                model.ExpiresOn = result.ExpiresOn.DateTime;
                //save the token

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {

                var msg = e.InnerException;
                var asd = e.Message;
                var source = e.Source;

                return View();
            }

        }

        public string GetCodeVerifier()
        {

            if (System.IO.File.Exists(codeVerifierFilePath))
            {
                return System.IO.File.ReadAllText(codeVerifierFilePath);
            }

            return null;
        }

        public string GetCodeChallenge()
        {
            string wwwrootPath = _webHostEnvironment.WebRootPath;
            string filePath = Path.Combine(wwwrootPath, "codeVerifier.txt");
            if (System.IO.File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(codeChallengeFilePath);
            }

            return null;
        }

        public void SaveCodeVerifierAndChallenge(string codeVerifier, string codePath)
        {
            System.IO.File.WriteAllText(codePath, codeVerifier);

        }
        public string CreateAuthorizationCodeRequestUrl()
        {
            codeVerifier = GenerateCodeVerifier();
            codeChallenge = GenerateCodeChallenge(codeVerifier);

            var builder = new UriBuilder("authority" + "/oauth2/v2.0/authorize");

            var query = HttpUtility.ParseQueryString(builder.Query);
            query["client_id"] = "clientId";
            query["redirect_uri"] = "RedirectUrl";
            query["response_type"] = "code";
            query["scope"] = string.Join(" ",//scope);
            query["code_challenge"] = codeChallenge;
            query["code_challenge_method"] = "S256";

            builder.Query = query.ToString();

            string authorizationRequestUrl = builder.ToString();
            SaveCodeVerifierAndChallenge(codeVerifier, codeVerifierFilePath);
            return authorizationRequestUrl;
        }

        private string GenerateCodeVerifier()
        {
            const int codeVerifierLength = 64;
            byte[] randomBytes = new byte[codeVerifierLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            codeVerifier = Base64UrlEncode(randomBytes);
            return codeVerifier;
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            byte[] codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            byte[] codeChallengeBytes;
            using (var sha256 = SHA256.Create())
            {
                codeChallengeBytes = sha256.ComputeHash(codeVerifierBytes);
            }
            codeChallenge = Base64UrlEncode(codeChallengeBytes);
            return codeChallenge;
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            string base64 = Convert.ToBase64String(bytes);
            string base64Url = base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
            return base64Url;
        }
    }
}
