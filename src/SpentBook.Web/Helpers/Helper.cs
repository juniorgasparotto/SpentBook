using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpentBook.Domain;
using System.Security.Principal;
using SpentBook.Data.FileSystem;
using System.Text.RegularExpressions;
using System.Text;

namespace SpentBook.Web.Controllers
{
    public static class Helper
    {
        private static IUnitOfWork UoW;

        public static string GetUserDataBase(IPrincipal user)
        {
            var userName = user.Identity.Name;
            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Not logged");

            var uploadPath = HttpContext.Current.Server.MapPath(@"\Data");
            var userPath = uploadPath + @"\" + userName + @"\database.json";
            return userPath;
        }

        public static IUnitOfWork GetUnitOfWorkByCurrentUser()
        {
            if (UoW == null)
            {
                var databaseFile = Helper.GetUserDataBase(HttpContext.Current.User);
                UoW = new FileSystemWithJsonUnitOfWork(databaseFile);
            }

            return UoW;
        }

        public static string CreateFriendlyURL(string value)
        {
            if (String.IsNullOrEmpty(value)) return "";

            // remove accents
            value = RemoveAccents(value);

            // remove entities
            value = Regex.Replace(value, @"&\w+;", "");
            // remove anything that is not letters, numbers, dash, or space
            value = Regex.Replace(value, @"[^A-Za-z0-9\-\s]", "");
            // remove any leading or trailing spaces left over
            value = value.Trim();
            // replace spaces with single dash
            value = Regex.Replace(value, @"\s+", "-");
            // if we end up with multiple dashes, collapse to single dash            
            value = Regex.Replace(value, @"\-{2,}", "-");
            // make it all lower case
            value = value.ToLower();
            // if it's too long, clip it
            if (value.Length > 80)
                value = value.Substring(0, 79);
            // remove trailing dash, if there is one
            if (value.EndsWith("-"))
                value = value.Substring(0, value.Length - 1);

            return value;
        }

        public static string RemoveAccents(string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        } 
    }
}