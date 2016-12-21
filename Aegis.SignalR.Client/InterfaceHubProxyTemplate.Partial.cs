using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aegis.SignalR.Client
{
    public partial class InterfaceHubProxyTemplate
    {
        public Type Interface { get; set; }
        public Type ProxyInterface { get; set; }

        public static string GetTypeString(Type type)
        {
            if (type.IsGenericType)
            {
                var gt = type.GetGenericTypeDefinition();
                if (gt == typeof (Task<>))
                {
                    return GetTypeString(type.GetGenericArguments()[0]);
                }

                var strBuilder = new StringBuilder();
                strBuilder.Append(Regex.Replace(type.GetGenericTypeDefinition().Name, @"`\d", "<"));

                var ts = type.GenericTypeArguments;

                strBuilder.Append(string.Join(",", ts.Select(GetTypeString)));

                strBuilder.Append(">");

                return strBuilder.ToString();
            }
            return type.FullName;
        }
    }
}