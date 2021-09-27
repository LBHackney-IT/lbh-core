namespace Hackney.Shared.Sns
{
    public static class EventTypes
    {
        public const string PersonCreatedEvent = "PersonCreatedEvent";
        public const string PersonUpdatedEvent = "PersonUpdatedEvent";

        public const string PersonAddedToTenureEvent = "PersonAddedToTenureEvent";
        public const string PersonRemovedFromTenureEvent = "PersonRemovedFromTenureEvent";

        public const string ContactDetailAddedEvent = "ContactDetailAddedEvent";
        public const string ContactDetailDeletedEvent = "ContactDetailDeletedEvent";

        public const string TenureCreatedEvent = "TenureCreatedEvent";
        public const string TenureUpdatedEvent = "TenureUpdatedEvent";
    }
}
