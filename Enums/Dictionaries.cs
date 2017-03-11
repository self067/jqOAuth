using jqOAuth.Enums;
using System.Collections.Generic;

namespace jqOAuth.Enums
{
    /// <summary>
    /// Linked with enums Dictionaries
    /// </summary>
    public class Dictionaries
    {
        public static readonly Dictionary<OAuthParameterEnum, string> OAuthParametersDict = new Dictionary<OAuthParameterEnum, string>()
        {
            { OAuthParameterEnum.OAUTH_TOKEN, "oauth_token" },
            { OAuthParameterEnum.OAUTH_TOKEN_SECRET, "oauth_token_secret" },
            { OAuthParameterEnum.OAUTH_CALLBACK_CONFIRMED, "oauth_callback_confirmed" },
            { OAuthParameterEnum.OAUTH_VERIFIER, "oauth_verifier" },
            { OAuthParameterEnum.OAUTH_ACCESS_TOKEN, "access_token"}
        };
    }
}
