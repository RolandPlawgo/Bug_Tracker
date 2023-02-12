using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bug_Tracker.Data
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> 
        where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            dbSet = _context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAsync()
        {
            return await dbSet.ToListAsync();
        }
        public async Task<IEnumerable<TEntity>> GetAsync(
            string includeProperties = "",
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
        {
            IQueryable<TEntity> entities = dbSet;

            if (includeProperties != "")
            {
                entities = entities.Include(includeProperties);
            }

            if (filter != null)
            {
                entities = entities.Where(filter);
            }

            if (orderBy != null)
            {
                entities = orderBy(entities);
            }

            return await entities.ToListAsync();
        }
        public async Task<IEnumerable<TEntity>> GetAsync(
            string includeProperties = "",
            List<Expression<Func<TEntity, bool>>>? filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
        {
            IQueryable<TEntity> entities = dbSet;

            if (includeProperties != "")
            {
                entities = entities.Include(includeProperties);
            }

            if (filters != null && filters.Count() != 0)
            {
                for (int i = 0; i < filters.Count(); i++)
                {
                    entities = entities.Where(filters[i]);
                }
            }

            if (orderBy != null)
            {
                entities = orderBy(entities);
            }

            return await entities.ToListAsync();
        }
        public async Task<IEnumerable<TEntity>> GetAsync(
            int page,
            int elementsOnPage,
            string includeProperties = "",
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
        {
            if (page <= 0)
            {
                throw new ArgumentException("Number of pages cannot be lower or equal to 0");
            }
            if (elementsOnPage <= 0)
            {
                throw new ArgumentException("Number elements in page cannot lower or equal to 0");
            }

            IQueryable<TEntity> entities = dbSet;

            if (includeProperties != "")
            {
                entities = entities.Include(includeProperties);
            }

            if (filter != null)
            {
                entities = entities.Where(filter);
            }

            if (orderBy != null)
            {
                entities = orderBy(entities);
            }

            //pages = (int)Math.Ceiling((decimal)entities.Count() / (decimal)elementsOnPage);
            int pages = (int)Math.Ceiling((decimal)entities.Count() / (decimal)elementsOnPage);
            if (pages == 0) pages = 1;

            if (page > pages)
            {
                throw new ArgumentException("Page does not exist");
            }

            entities = entities.Skip((page - 1) * elementsOnPage).Take(elementsOnPage);

            return await entities.ToListAsync();
        }
        public async Task<IEnumerable<TEntity>> GetAsync(
            int page,
            int elementsOnPage,
            string includeProperties = "",
            List<Expression<Func<TEntity, bool>>>? filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
        {
            if (page <= 0)
            {
                throw new ArgumentException("Number of pages cannot be lower or equal to 0");
            }
            if (elementsOnPage <= 0)
            {
                throw new ArgumentException("Number of elements on page cannot lower or equal to 0");
            }


            IQueryable<TEntity> entities = dbSet;

            if (includeProperties != "")
            {
                entities = entities.Include(includeProperties);
            }

            if (filters != null && filters.Count() != 0)
            {
                for (int i = 0; i < filters.Count(); i++)
                {
                    entities = entities.Where(filters[i]);
                }
            }

            if (orderBy != null)
            {
                entities = orderBy(entities);
            }

            //pages = (int)Math.Ceiling((decimal)entities.Count() / (decimal)elementsOnPage);
            int pages = (int)Math.Ceiling((decimal)entities.Count() / (decimal)elementsOnPage);
            if (pages == 0) pages = 1;

            if (page > pages)
            {
                throw new ArgumentException("Page does not exist");
            }

            entities = entities.Skip((page - 1) * elementsOnPage).Take(elementsOnPage);

            return await entities.ToListAsync();
        }

        public async Task<TEntity?> GetEntityAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
        public async Task <TEntity?> GetEntityAsync(Expression<Func<TEntity, bool>>? filter, string includeProperties = "")
        {
            IQueryable<TEntity> entities = dbSet;

            if (includeProperties != "")
            {
                entities = entities.Include(includeProperties);
            }

            if (filter != null)
            {
                entities = entities.Where(filter);
            }

            return await entities.FirstOrDefaultAsync();
        }

        public async Task CreateAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
        }

        public async Task EditAsync(TEntity entity)
        {
            if (await dbSet.ContainsAsync(entity))
            {
                dbSet.Update(entity);
            }
            else
            {
                throw new InvalidOperationException("The entity to be altered does not exist");
            }
        }

        public async Task DeleteAsync(int id)
        {
            TEntity? entity = await GetEntityAsync(id);
            if (entity != null)
            {
                dbSet.Remove(entity);
            }
            else
            {
                throw new InvalidOperationException("The entity to be deleted does not exist");
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
