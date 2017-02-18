using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Tweetinvi;
using Tweetinvi.Models;

namespace ReactiveTwitter.Model
{
    public class TwitterAuthenticator
    {
        private static IAuthenticationContext _authenticationContext;
        const string CreadentialsFileName = "ReactiveTwitter.xml";

        public static bool AuthenticateUser()
        {
            var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
            var userCredentials = GetCredentials();
            if (userCredentials != null)
            {
                Auth.SetUserCredentials(consumerKey, consumerSecret,
                    userCredentials.AccessToken, userCredentials.AccessTokenSecret);
                return true;
            }
            var appCredentials = new TwitterCredentials(consumerKey, consumerSecret);

            _authenticationContext = AuthFlow.InitAuthentication(appCredentials);

            Process.Start(_authenticationContext.AuthorizationURL);
            return false;
        }

        public static void CreateAndSetCredentials(string pinCode)
        {
            var userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pinCode, _authenticationContext);

            Auth.SetCredentials(userCredentials);
            SaveCredentials(userCredentials);
        }

        private static ITwitterCredentials GetCredentials()
        {
            string settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);
            string credentialsFile = Path.Combine(settingsDirectory, CreadentialsFileName);
            if (!File.Exists(credentialsFile))
                return null;
            var credentialsDoc = XDocument.Load(credentialsFile);
            if (credentialsDoc.Root == null)
                return null;
            return new TwitterCredentials("", "", credentialsDoc.Root.Element("AccessToken")?.Value, 
                credentialsDoc.Root.Element("AccessSecret")?.Value);
        }

        public static void SaveCredentials(ITwitterCredentials credentials)
        {
            string settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);
            string credentialsFile = Path.Combine(settingsDirectory, CreadentialsFileName);
            XDocument credentialsDoc = new XDocument(
                new XElement("Credentials",
                    new XElement("AccessToken", credentials.AccessToken),
                    new XElement("AccessSecret", credentials.AccessTokenSecret)));
            credentialsDoc.Save(credentialsFile);
        }
    }
}