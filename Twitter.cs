using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using jqOAuth.Enums;
using jqOAuth.DataObjects;

namespace UI.Logic.OAuth
{
    public class Twitter : OAuth
    {
        public override OAuthProviderEnum OAuthProvider { get { return OAuthProviderEnum.TWITTER; } }

        string _consumerSecret;

        string _accessTokenSecret;

        public Twitter(string clientId, string clientPrivate, string consumerSecret, string accessTokenSecret)
            : base(clientId, clientPrivate)
        {
            this._consumerSecret = consumerSecret;

            this._accessTokenSecret = accessTokenSecret;
        }

        public string GetRequestToken()
        {
            const string url = "https://api.twitter.com/oauth/request_token";

            const string method = "POST";

            var headerParams = new Dictionary<string, string>() {

                { "oauth_callback", $"http://heap.tech/oauth/callback/{OAuthProvider}/" }

            };

            var oauthHeader = this.generateOAuthRequestHeader(url, method, headerParams);

            var response = base.HttpPostRequest(
                url: url,
                paramsToBeReturned: new OAuthParameterEnum[] { OAuthParameterEnum.OAUTH_TOKEN },
                headerParams: new Dictionary<string, string>() { { "Authorization", oauthHeader } },
                bodyParams: null);

            return response
                .Where(_ => _.Key == OAuthParameterEnum.OAUTH_TOKEN)
                .Select(_ => _.Value)
                .SingleOrDefault();
        }

        public override OAuthUser GetUserCredentials(Dictionary<string, string> headerParameters, Dictionary<string, string> queryParameters)
        {
            const string url = "https://api.twitter.com/1.1/account/verify_credentials.json";

            const string method = "GET";

            var oauthHeader = generateOAuthRequestHeader(url, method);

            return this.HttpGetRequestJson<OAuthUser>(
                url: url,
                headerParameters: new Dictionary<string, string> { { "Authorization", oauthHeader } },
                queryParameters: null,
                jsonSerializerMappings: null);
        }

        public override IEnumerable<KeyValuePair<OAuthParameterEnum, string>> GetAccessToken(Dictionary<string, string> headerParams, Dictionary<string, string> bodyParams)
        {
            const string url = "https://api.twitter.com/oauth/access_token";

            const string method = "POST";

            var oauthHeader = generateOAuthRequestHeader(url, method, headerParams);

            return this.HttpPostRequest(
                url: url,
                paramsToBeReturned: new OAuthParameterEnum[] { OAuthParameterEnum.OAUTH_TOKEN_SECRET },
                headerParams: new Dictionary<string, string> { { "Authorization", oauthHeader } },
                bodyParams: bodyParams);
        }

        #region Twitter private methods
        string generateOAuthRequestSignature(string url, string httpMethod, Dictionary<string, string> oauthRequestParameters)
        {
            var sigString = string.Join("&",
                oauthRequestParameters
                    .Union(oauthRequestParameters)
                    .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            var fullSigData = string.Format(
                "{0}&{1}&{2}",
                httpMethod,
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(sigString.ToString())
            );

            var signatureSHA1Hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", _consumerSecret, _accessTokenSecret)));

            var signatureSHA1Hash = signatureSHA1Hasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData.ToString()));

            return Convert.ToBase64String(signatureSHA1Hash);
        }

        string generateOAuthRequestHeader(string url, string method = "POST", Dictionary<string, string> advancedHeaderParams = null)
        {
            var timeStamp = ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

            var header = new Dictionary<string, string>()
            {
                { "oauth_consumer_key", base._clientId },
                { "oauth_nonce", base.generateNonce() },
                { "oauth_signature_method","HMAC-SHA1"},
                { "oauth_timestamp",  timeStamp},
                { "oauth_token", base._clientSecret},
                { "oauth_version","1.0"}
            };

            if (advancedHeaderParams != null &&
                advancedHeaderParams.Count > 0)
            {
                string _tryGetValue;

                foreach (var _ in advancedHeaderParams)
                {
                    if (!header.TryGetValue(_.Key, out _tryGetValue))
                    {
                        header.Add(_.Key, _.Value);
                    }
                    else
                    {
                        header[_.Key] = _.Value;
                    }
                }
            }

            header.Add("oauth_signature", generateOAuthRequestSignature(url, method, header));

            var stringifiedHeader = from _ in header
                                    orderby _.Key
                                    select string.Format("{0}=\"{1}\"", Uri.EscapeDataString(_.Key), Uri.EscapeDataString(_.Value));

            return string.Format("OAuth {0}", string.Join(", ", stringifiedHeader));
        }
        #endregion
    }
}