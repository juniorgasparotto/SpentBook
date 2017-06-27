using SysCommand.ConsoleApp;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Admin.Commands
{
    public class ManagerProjectCommand : Command
    {
        public void CleanMigrationsFiles()
        {
            var files = Directory.GetFiles("Migrations");
            foreach(var file in files)
            {
                if (Path.GetFileName(file).StartsWith("2")) 
                    File.Delete(file);
            }
        }

        public void CleanEmptyMigrationsFiles()
        {
            var files = Directory.GetFiles("Migrations");
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.StartsWith("2") && !fileName.Contains(".Design")) 
                {
                    var content = File.ReadAllText(file);
                    if (!content.Contains("migrationBuilder."))
                    {
                        File.Delete(file);
                        File.Delete(Path.Combine(Path.GetDirectoryName(file), fileName + ".Designer.cs"));
                    }
                }
            }
        }
    }
}
