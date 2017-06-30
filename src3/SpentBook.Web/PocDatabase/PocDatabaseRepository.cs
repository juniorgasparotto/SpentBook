using PocDatabase;
using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SpentBook.Web
{
    internal class PocDatabaseRepository<TEntity> : IRepository<TEntity>
    {
        private PocFile<PocDatabaseUoW.Schema> pocFile;
        private PocRepository<PocDatabaseUoW.Schema, TEntity> repository;

        public PocDatabaseRepository(PocFile<PocDatabaseUoW.Schema> pocFile)
        {
            this.pocFile = pocFile;
            this.repository = this.pocFile.GetRepository<TEntity>();
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return this.repository.AsQueryable();
        }

        public void Delete(Guid id)
        {
            this.repository.Delete(id);
        }

        public void Delete(TEntity entityToDelete)
        {
            this.repository.Delete(entityToDelete);
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            return this.repository.Get(filter, orderBy, includeProperties);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.repository.GetAll();
        }

        public TEntity GetById(Guid id)
        {
            return this.repository.GetById(id);
        }

        public void Insert(TEntity entity)
        {
            this.repository.Insert(entity);
        }

        public void Update(TEntity entityToUpdate)
        {
            this.repository.Update(entityToUpdate);
        }
    }
}
