using jqOAuth.DataObjects;
using jqOAuth.Enums;
using System.Collections.Generic;

namespace UI.Logic.OAuth
{
    public class Github : OAuth
    {
        public override OAuthProviderEnum OAuthProvider { get { return OAuthProviderEnum.GITHUB; } }

        public Github(string clientId, string clientPrivate)
            : base(clientId, clientPrivate)
        { }

        public override OAuthUser GetUserCredentials(Dictionary<string, string> headerParameters, Dictionary<string, string> queryParameters)
        {
            const string url = "https://api.github.com/user";

            return this.HttpGetRequestJson<OAuthUser>(
                url: url,
                headerParameters: headerParameters,
                queryParameters: queryParameters,
                jsonSerializerMappings: new Dictionary<string, string>() { { "screen_name", "login" }, { "profile_image_url", "avatar_url" } });
        }

        public override IEnumerable<KeyValuePair<OAuthParameterEnum, string>> GetAccessToken(Dictionary<string, string> headerParams, Dictionary<string, string> bodyParams)
        {
            const string url = "https://github.com/login/oauth/access_token";

            return this.HttpPostRequest(
                url: url,
                paramsToBeReturned: new OAuthParameterEnum[] { OAuthParameterEnum.OAUTH_ACCESS_TOKEN },
                headerParams: null,
                bodyParams: bodyParams);
        }
    }

}