namespace Hackney.Core.DynamoDb.EntityUpdater.Interfaces
{
    /// <summary>
    /// Interface describing generic methods for updating an instance of an entity from the suypplied request object and raw request json
    /// </summary>
    public interface IEntityUpdater
    {
        /// <summary>
        /// Updates the supplied entity with the updated property values described in the request object / json
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TUpdateObject">The type of the update request object</typeparam>
        /// <param name="entityToUpdate">The entity to update</param>
        /// <param name="updateJson">The raw update request json from which the request object was deserialized</param>
        /// <param name="updateObject">The update request object</param>
        /// <returns>A response object</returns>
        UpdateEntityResult<TEntity> UpdateEntity<TEntity, TUpdateObject>(
            TEntity entityToUpdate,
            string updateJson,
            TUpdateObject updateObject)
                where TEntity : class
                where TUpdateObject : class;

        /// <summary>
        /// Updates the supplied entity with the updated property values described in the request object / json.
        /// * This method expects both a request object and the raw request json so that the appropriate request object validation
        /// can be executed by the MVC pipeline.
        /// * The raw request json should contain ONLY the properties to be updated.
        /// * The property names in the json / request object MUST MATCH the corresponing properties on the entity type (assuming the json uses camel casing).
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TUpdateObject">The type of the update request object</typeparam>
        /// <param name="entityToUpdate">The entity to update</param>
        /// <param name="updateJson">The raw update request json from which the request object was deserialized</param>
        /// <param name="updateObject">The update request object</param>
        /// <param name="ignoreUnchangedProperties">Whether or not to ignore property values set in the update request
        /// but that are actually the same as current entity value.</param>
        /// <returns>A response object</returns>
        UpdateEntityResult<TEntity> UpdateEntity<TEntity, TUpdateObject>(
            TEntity entityToUpdate,
            string updateJson,
            TUpdateObject updateObject,
            bool ignoreUnchangedProperties)
                where TEntity : class
                where TUpdateObject : class;
    }
}
