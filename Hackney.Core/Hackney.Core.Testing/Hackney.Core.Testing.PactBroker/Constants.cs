namespace Hackney.Core.Testing.PactBroker
{
    public static class Constants
    {
        public static readonly string DEFAULT_SERVER_URI = "http://localhost:9222";
        public static readonly string PROVIDER_STATES_ROUTE = "/provider-states";

        public static readonly string ENV_VAR_PACT_BROKER_USER = "PactBrokerUser";
        public static readonly string ENV_VAR_PACT_BROKER_USER_PASSWORD = "PactBrokerUserPassword";
        public static readonly string ENV_VAR_PACT_BROKER_PROVIDER_NAME = "PactBrokerProviderName";
        public static readonly string ENV_VAR_PACT_BROKER_PATH = "PactBrokerPath";
    }
}
