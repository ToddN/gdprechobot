using System;
using System.Linq;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.GDPREchoBot
{
    public class BotAuthenticationMicrosoftInternal : BotAuthentication
    {
        static BotAuthenticationMicrosoftInternal()
        {
            switch ("localhost")
            {
                case "localhost":
                case "localhost-scratch":
                case "scratch":
                    MicrosoftAppCredentials.TrustServiceUrl("https://intercom-api-scratch.azurewebsites.net", DateTime.MaxValue);
                    break;

                case "localhost-ppe":
                case "ppe":
                    MicrosoftAppCredentials.TrustServiceUrl("https://intercom-api-ppe.azurewebsites.net", DateTime.MaxValue);
                    break;

                case "localhost-bcdr":
                case "bcdr":
                    MicrosoftAppCredentials.TrustServiceUrl("https://api.bcdr.botframework.com", DateTime.MaxValue);
                    break;

                case "prod":
                default:
                    // no need to override default
                    break;
            }
        }

        public override string OpenIdConfigurationUrl
        {
            get
            {
                switch ("localhost")
                {
                    case "localhost":
                    case "localhost-scratch":
                    case "scratch":
                    case "localhost-ppe":
                    case "ppe":
                    case "bcdr":
                        return "https://intercom-api-ppe.azurewebsites.net/v1/.well-known/openidconfiguration";

                    case "prod":
                    default:
                        return "https://login.botframework.com/v1/.well-known/openidconfiguration";
                }
            }
        }
    }
}