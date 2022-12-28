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

        public IEnumerable<TEntity> Get()
        {
            return dbSet;
        }
        public IEnumerable<TEntity> Get(
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

            return entities;
        }
        public IEnumerable<TEntity> Get(
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

            return entities;
        }
        public IEnumerable<TEntity> Get(
            int page,
            int elementsOnPage,
            out int pages,
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

            pages = (int)Math.Ceiling((decimal)entities.Count() / (decimal)elementsOnPage);

            entities = entities.Skip((page - 1) * elementsOnPage).Take(elementsOnPage);

            return entities;
        }
        public IEnumerable<TEntity> Get(
            int page,
            int elementsOnPage,
            out int pages,
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

            pages = (int)Math.Ceiling((decimal)entities.Count() / (decimal)elementsOnPage);
            if (page > pages)
            {
                throw new ArgumentException("Page does not exist");
            }

            entities = entities.Skip((page - 1) * elementsOnPage).Take(elementsOnPage);

            return entities;
        }

        public TEntity? GetEntity(int id)
        {
            return dbSet.Find(id);
        }
        public TEntity? GetEntity(Expression<Func<TEntity, bool>>? filter, string includeProperties = "")
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

            return entities.FirstOrDefault();
        }

        public void Create(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public void Edit(TEntity entity)
        {
            if (dbSet.Contains(entity))
            {
                dbSet.Update(entity);
            }
            else
            {
                throw new InvalidOperationException("The entity to be altered does not exist");
            }
        }

        public void Delete(int id)
        {
            if (GetEntity(id) != null)
            {
                dbSet.Remove(GetEntity(id)!);
            }
            else
            {
                throw new InvalidOperationException("The entity to be deleted does not exist");
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
