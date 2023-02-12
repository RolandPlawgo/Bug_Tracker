using System.Linq.Expressions;

namespace Bug_Tracker.Data
{
    public interface IGenericRepository<TEntity> 
        where TEntity : class
    {
        /// <summary>
        /// Begins tracking the given entity, so that it will be added when <c>IGenericRepository.Save()</c> is called
        /// </summary>
        /// <param name="entity"></param>
        Task CreateAsync(TEntity entity);
        /// <summary>
        /// Begins tracking the given entity, so that it will be deleted when <c>IGenericRepository.Save()</c> is called
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="InvalidOperationException">Thrown when no entity with the specified <paramref name="id"/> can be found</exception>
        Task DeleteAsync(int id);
        /// <summary>
        /// Gets all the entities in the database
        /// </summary>
        Task<IEnumerable<TEntity>> GetAsync();
        /// <summary>
        /// Get all the entities, that satisfy the condition specified in <paramref name="filter"/> ordered by <paramref name="orderBy"/>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <param name="filter">If it's null, no filtering will be performed and all entities will be returned</param>
        /// <param name="orderBy">If it's null, the entities will not be ordered</param>
        Task<IEnumerable<TEntity>> GetAsync(string includeProperties = "", Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);
        /// <summary>
        /// Get all the entities, that satisfy each of the conditions specified in <paramref name="filters"/> ordered by <paramref name="orderBy"/>
        /// </summary>
        /// <param name="includeProperties"></param>
        /// <param name="filters">If it's null, no filtering will be performed and all entities will be returned</param>
        /// <param name="orderBy">If it's null, the entities will not be ordered</param>
        Task<IEnumerable<TEntity>> GetAsync(string includeProperties = "", List<Expression<Func<TEntity, bool>>>? filters = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);
        /// <summary>
        /// Get entities, that satisfy the condition specified in <paramref name="filter"/> ordered by <paramref name="orderBy"/>, to be displayed on the page <paramref name="page"/> of <paramref name="pages"/>
        /// </summary>
        /// <param name="page">The page, for which the entities will be returned</param>
        /// <param name="elementsOnPage">Number of elements on each page</param>
        /// <param name="pages">The number of pages</param>
        /// <param name="includeProperties"></param>
        /// <param name="filter">If it's null, no filtering will be performed and all entities will be returned</param>
        /// <param name="orderBy">If it's null, the entities will not be ordered</param>
        Task<IEnumerable<TEntity>> GetAsync(int page, int elementsOnPage, string includeProperties = "", Expression<Func<TEntity, bool>>? filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);
        /// <summary>
        /// Get entities, that satisfy each of the conditions specified in <paramref name="filters"/> ordered by <paramref name="orderBy"/>, to be displayed on the page <paramref name="page"/> of <paramref name="pages"/>
        /// </summary>
        /// <param name="page">The page, for which the entities will be returned</param>
        /// <param name="elementsOnPage">Number of elements on each page</param>
        /// <param name="pages">The number of pages</param>
        /// <param name="includeProperties"></param>
        /// <param name="filters">If it's null, no filtering will be performed and all entities will be returned</param>
        /// <param name="orderBy">If it's null, the entities will not be ordered</param>
        Task<IEnumerable<TEntity>> GetAsync(int page, int elementsOnPage, string includeProperties = "", List<Expression<Func<TEntity, bool>>>? filters = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);
        /// <summary>
        /// Gets an entity that satisfies the condition
        /// </summary>
        /// <param name="id">Primary key</param>
        /// <returns>The entity with the specified <paramref name="id"/></returns>
        Task<TEntity?> GetEntityAsync(int id);
        /// <summary>
        /// Gets an entity that satisfies the condition
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="includeProperties"></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null</exception>
        /// <returns>The first entity that satisfies the condition specified in <paramref name="filter"/></returns>
        Task<TEntity?> GetEntityAsync(Expression<Func<TEntity, bool>> filter, string includeProperties = "");
        /// <summary>
        /// Saves all changes, that have been made in this context, to the database
        /// </summary>
        Task SaveAsync();
        /// <summary>
        /// Begins tracking the given entity, so that it will be edited when <c>IGenericRepository.Save()</c> is called
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="InvalidOperationException">Thrown when the entity can not be found</exception>
        Task EditAsync(TEntity entity);
    }
}