using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Domain
{
    public interface IRepository<TEntity>
    {
        IEnumerable<TEntity> GetAll();

        IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = ""
        );


        TEntity GetById(Guid id);
        void Insert(TEntity entity);
        void Delete(Guid id);
        void Delete(TEntity entityToDelete);
        void Update(TEntity entityToUpdate);
    }
}