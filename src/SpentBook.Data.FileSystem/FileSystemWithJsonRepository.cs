using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Data.FileSystem
{
    public class FileSystemWithJsonRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity
    {
        private string _fileDb;
        private FileDataBase _fileDataBase;
        private List<TEntity> _set;

        public FileSystemWithJsonRepository(string fileDb, Func<FileDataBase, List<TEntity>> getProperty)
        {
            this._fileDb = fileDb;
            this._fileDataBase = FileDataBase.GetOrCreate(fileDb);
            this._set = this.GetSet(getProperty);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.Get(null, null);
        }
        
        public IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            var query = _set.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return query.ToList();
        }

        public IQueryable<TEntity> AsQueryable()
        {
            var query = _set.AsQueryable();
            return query;
        }

        public TEntity GetById(Guid id)
        {
            return this.Get(f => f.Id == id).FirstOrDefault();
        }

        public void Insert(TEntity entity)
        {
            entity.Id = Guid.NewGuid();
            this._set.Add(entity);
            FileDataBase.Persists(this._fileDb, this._fileDataBase);
        }

        public void Delete(Guid id)
        {
            var entity = this.GetById(id);
            Delete(entity);
        }

        public void Delete(TEntity entityToDelete)
        {
            if (entityToDelete != null)
            {
                this._set.Remove(entityToDelete);
                FileDataBase.Persists(this._fileDb, this._fileDataBase);
            }
        }

        public void Update(TEntity entityToUpdate)
        {
            if (entityToUpdate != null)
            {
                var entityFound = this.GetById(entityToUpdate.Id);
                var pos = this._set.IndexOf(entityFound);
                this._set.Remove(entityFound);
                this._set.Insert(pos, entityToUpdate);
                FileDataBase.Persists(this._fileDb, this._fileDataBase);
            }
        }

        private List<TEntity> GetSet(Func<FileDataBase, List<TEntity>> getProperty)
        {
            var listAux = new List<FileDataBase> { this._fileDataBase };
            return listAux.Select(getProperty).FirstOrDefault();
        }
    }
}