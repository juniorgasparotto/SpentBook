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
using SpentBook.Domain.Imports;

namespace SpentBook.Web
{
    public class TransactionCSVRepository : IRepository<Transaction>
    {
        public TransactionCSVRepository()
        {
            
        }

        public IEnumerable<Transaction> GetAll()
        {
            return this.Get(null, null);
        }

        public IEnumerable<Transaction> Get(
            Expression<Func<Transaction, bool>> filter = null,
            Func<IQueryable<Transaction>, IOrderedQueryable<Transaction>> orderBy = null,
            string includeProperties = "")
        {
            var import = new TransactionImportDefaultCSV();
            var userName = "admin"; // User.Identity.Name;
            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Not logged");

            //var uploadPath = System.Web.HttpContext.Current.Server.MapPath("/Data");
            var uploadPath = "/Data";
            var userPath = uploadPath + "/" + userName + "/Spents";
            var _set = import.GetTransactionsFromFolder(userPath);

            var query = _set.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return query.ToList();
        }

        public IQueryable<Transaction> AsQueryable()
        {
            return this.GetAll().AsQueryable();
        }

        public Transaction GetById(Guid id)
        {
            return this.Get(f => f.Id == id).FirstOrDefault();
        }

        public void Insert(Transaction entity)
        {
            
        }

        public void Delete(Guid id)
        {
            
        }

        public void Delete(Transaction entityToDelete)
        {
            
        }

        public void Update(Transaction entityToUpdate)
        {
            
        }
    }
}