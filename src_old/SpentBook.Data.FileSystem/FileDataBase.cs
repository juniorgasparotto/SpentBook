using Newtonsoft.Json;
using SpentBook.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Data.FileSystem
{
    public class FileDataBase
    {
        private static Dictionary<string, FileDataBase> _staticFilesDataBases = new Dictionary<string, FileDataBase>();
        
        #region Schema

        public List<Dashboard> Dashboards = new List<Dashboard>();
        public List<Transaction> Transactions = new List<Transaction>();
        
        #endregion

        private FileDataBase() { }

        public static FileDataBase GetOrCreate(string fileDb)
        {
            {
                FileDataBase fdb = null;

                if (_staticFilesDataBases.ContainsKey(fileDb))
                {
                    fdb = _staticFilesDataBases[fileDb];
                }
                else 
                {
                    fdb = FileDataBaseHelper.GetAllJsonDataBases<FileDataBase>(fileDb).FirstOrDefault();

                    if (fdb == null)
                        fdb = new FileDataBase();

                    _staticFilesDataBases.Add(fileDb, fdb);
                }

                return fdb;
            }
        }

        public static void Persists(string fileDb, FileDataBase fdb)
        {
            using (FileStream fs = File.Open(fileDb, FileMode.Truncate))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jw, fdb);
            }
        }

        public static void Refresh(string fileDb)
        {
            if (_staticFilesDataBases.ContainsKey(fileDb))
                _staticFilesDataBases.Remove(fileDb);
        }
    }
}
