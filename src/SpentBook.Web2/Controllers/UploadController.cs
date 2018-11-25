using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace SpentBook.Web.Controllers
{
    public class UploadFileResult
    { 
        public int IDArquivo { get; set; }
        public string Nome { get; set; } 
        public int Tamanho { get; set; } 
        public string Tipo { get; set; } 
        public string Caminho { get; set; } 
    }

    public class FileUploadController : Controller 
    { 
        // // GET: /FileUpload/ 
        public ActionResult Index()
        { 
            return View(); 
        } 
        
        public ActionResult FileUpload() 
        { 
            int arquivosSalvos = 0; 
            for (int i = 0; i < Request.Form.Files.Count; i++)
            { 
                var arquivo = Request.Form.Files[i];
 
                //Suas validações ...... 
                //Salva o arquivo 
                if (arquivo.Length > 0) 
                { 
                    //var uploadPath = Server.MapPath("~/Uploads"); 
                    var uploadPath = "~/Uploads";
                    string caminhoArquivo = Path.Combine(@uploadPath, Path.GetFileName(arquivo.FileName)); 
                    //arquivo.SaveAs(caminhoArquivo);
                    arquivosSalvos++; 
                }
            } 
            
            ViewData["Message"] = String.Format("{0} arquivo(s) salvo(s) com sucesso.", arquivosSalvos); 
            
            return View("Upload");
        } 
    }
}