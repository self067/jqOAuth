using jqOAuth.DataObjects;
using jqOAuth.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;


namespace UI.Logic.OAuth
{
    public abstract class OAuth
    {
        public abstract OAuthProviderEnum OAuthProvider { get; }

        protected string _clientId;

        protected string _clientSecret;

        public OAuth(string clientId, string clientSecret)
        {
            _clientId = clientId;

            _clientSecret = clientSecret;
        }

        public abstract OAuthUser GetUserCredentials(Dictionary<string, string> headerParameters, Dictionary<string, string> queryParameters);

        public abstract IEnumerable<KeyValuePair<OAuthParameterEnum, string>> GetAccessToken(Dictionary<string, string> headerParams, Dictionary<string, string> bodyParams);

        protected T HttpGetRequestJson<T>(string url, Dictionary<string, string> headerParameters, Dictionary<string, string> queryParameters, Dictionary<string, string> jsonSerializerMappings)
        {
            using (var http = new HttpClient())
            {
                if (headerParameters != null)
                {
                    foreach (var _ in headerParameters)
                        http.DefaultRequestHeaders.Add(_.Key, _.Value);
                }

                string queryString = null;

                if (queryParameters != null)
                {
                    queryString = string.Join("&", queryParameters.Select(_ => $"{Uri.EscapeDataString(_.Key)}={Uri.EscapeDataString(_.Value)}"));

                    url = string.Concat(url, "?", queryString);
                }

                try
                {
                    var httpResp = http.GetAsync(url).Result;

                    if (jsonSerializerMappings != null)
                    {
                        var jsonSerializerSettings = new JsonSerializerSettings();

                        jsonSerializerSettings.ContractResolver = new CustomContractResolver(jsonSerializerMappings);

                        return JsonConvert.DeserializeObject<T>(httpResp.Content.ReadAsStringAsync().Result, jsonSerializerSettings);
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<T>(httpResp.Content.ReadAsStringAsync().Result);
                    }

                }
                catch
                {
                    return default(T);
                }

            }
        }

        protected IEnumerable<KeyValuePair<OAuthParameterEnum, string>> HttpGetRequest(string url, OAuthParameterEnum[] paramsToBeReturned, Dictionary<string, string> queryParameters)
        {
            using (var http = new HttpClient())
            {
                string queryString = null;

                if (queryParameters != null)
                {
                    queryString = string.Join("&", queryParameters.Select(_ => $"{Uri.EscapeDataString(_.Key)}={Uri.EscapeDataString(_.Value)}"));

                    url = string.Concat(url, "?", queryString);
                }

                var httpResp = http.GetAsync(url).Result;

                return parseResponse(
                        response: httpResp.Content.ReadAsStringAsync().Result,
                        paramsToFind: paramsToBeReturned);
            }
        }

        protected IEnumerable<KeyValuePair<OAuthParameterEnum,string>> HttpPostRequest(string url, OAuthParameterEnum[] paramsToBeReturned, Dictionary<string,string> headerParams, Dictionary<string,string> bodyParams)
        {
            using (var http = new HttpClient())
            {
                FormUrlEncodedContent formData = null;

                if (headerParams != null)
                {
                    foreach (var _ in headerParams)
                        http.DefaultRequestHeaders.Add(_.Key, _.Value);
                }

                if (bodyParams != null)
                    formData = new FormUrlEncodedContent(bodyParams);

                var httpResp = http.PostAsync(url, formData).Result;

                return parseResponse(
                    response: httpResp.Content.ReadAsStringAsync().Result,
                    paramsToFind: paramsToBeReturned
                    );
            }
        }

        #region private
        protected IEnumerable<KeyValuePair<OAuthParameterEnum, string>> parseResponse(string response, OAuthParameterEnum[] paramsToFind)
        {
            var foundParameters = new List<KeyValuePair<OAuthParameterEnum, string>>();

            var parameters = from pd in Dictionaries.OAuthParametersDict
                             join pf in paramsToFind on pd.Key equals pf
                             select pd.Value;

            var regex = new Regex("(?<key>" + string.Join("|", parameters) + ")" + "=(?<value>[^&][A-z0-9\\-_]*)");

            Nullable<KeyValuePair<OAuthParameterEnum, string>> _matchedParameter;

            if (regex.IsMatch(response))
            {
                foreach (Match m in regex.Matches(response))
                {
                    _matchedParameter = Dictionaries.OAuthParametersDict.Where(_ => _.Value == m.Groups["key"]?.Value).Single();

                    if (_matchedParameter != null)
                        foundParameters.Add(new KeyValuePair<OAuthParameterEnum, string>(_matchedParameter.Value.Key, m.Groups["value"].Value));
                }
            }

            return foundParameters;
        }

        /// <summary>
        /// generate a random string containing only letters and digits 
        /// </summary>
        /// <returns></returns>
        protected string generateNonce(int length = 32)
        {
            var rnd = new Random();
            var regex = new Regex("[^A-z0-9]");
            var bytes = new byte[length];

            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(rnd.Next(0, 255));

            return regex.Replace(Convert.ToBase64String(bytes), "");
        }
        #endregion
    }
}