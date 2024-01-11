using System.Collections.Generic;

namespace Hackney.Core.DynamoDb.EntityUpdater
{
    public class UpdateEntityResult<T> where T : class
    {
        /// <summary>
        /// The updated entity
        /// </summary>
        public T UpdatedEntity { get; set; }

        /// <summary>
        /// A simple dictionary listing the previous value(s) of any entity properties actually updated by the call.
        /// The size and keys of this dictionary will match that of the NewValues property.
        /// </summary>
        public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A simple dictionary listing the new value(s) of any entity properties actually updated by the call.
        /// The size and keys of this dictionary will match that of the OldValues property.
        /// </summary>
        public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A collection of properties that were in the orignal request json but that were ignored because either
        /// * They did not exist on the request object, or
        /// * They did not exist on the entity at all.
        /// </summary>
        public List<string> IgnoredProperties { get; set; } = new List<string>();
    }
}
