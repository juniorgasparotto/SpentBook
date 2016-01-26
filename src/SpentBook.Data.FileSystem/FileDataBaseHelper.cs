using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpentBook.Data.FileSystem
{
    public static class FileDataBaseHelper
    {
        public static List<TSchema> GetAllJsonDataBases<TSchema>(string folderOrFileName)
        {
            var listRet = new List<TSchema>();
            var filesNames = new List<string>();

            if (File.Exists(folderOrFileName))
                filesNames.Add(folderOrFileName);
            else if (Directory.Exists(folderOrFileName))
                filesNames = Directory.GetFiles(folderOrFileName, "*.json", SearchOption.AllDirectories).ToList();

            if (filesNames.Count > 0)
            {
                foreach (var fileName in filesNames)
                {
                    using (var file = File.OpenText(fileName))
                    {
                        //var data = (FileDataBase)JsonConvert.DeserializeObject(file, typeof(FileDataBase),
                        //new JsonSerializerSettings
                        //{
                        //    Error = delegate(object sender, ErrorEventArgs args)
                        //    {
                        //        Debug.WriteLine(args.ErrorContext.Error.Message);
                        //        args.ErrorContext.Handled = true;
                        //    }
                        //});

                        JsonSerializer serializer = new JsonSerializer();
                        serializer.NullValueHandling = NullValueHandling.Include;
                        //serializer.Error += (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e) =>
                        //                    {
                        //                        e.ErrorContext.Handled = true;
                        //                    };

                        var fileDb = (TSchema)serializer.Deserialize(file, typeof(TSchema));
                        listRet.Add(fileDb);
                    }
                }
            }

            return listRet;
        }
    }
}
