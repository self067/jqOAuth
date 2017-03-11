using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace UI.Logic.OAuth
{
    public class CustomContractResolver : DefaultContractResolver
    {
        Dictionary<string, string> _propertyMappings;

        public CustomContractResolver(Dictionary<string, string> mappings)
        {
            this._propertyMappings = mappings;
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            string resolvedName;

            if (this._propertyMappings.TryGetValue(propertyName, out resolvedName))
                return resolvedName;

            else
                return base.ResolvePropertyName(propertyName);
        }
    }
}