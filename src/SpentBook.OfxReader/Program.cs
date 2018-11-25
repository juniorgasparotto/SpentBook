// using System;
// using System.IO;

// namespace SpentBook.OfxReader
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             var parser = new OFXDocumentParser();
//             var fileName = "Bradesco_11072017_143221.ofx";
//             var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
//             var ofxDocument = parser.Import(new FileStream(path, FileMode.Open));

//             foreach(var a in ofxDocument.Transactions)
//             {
//                 Console.WriteLine("Amount: " + a.Amount);
//                 Console.WriteLine("CheckNum: " + a.CheckNum);
//                 Console.WriteLine("Currency: " + a.Currency);
//                 Console.WriteLine("Date: " + a.Date);
//                 Console.WriteLine("FundAvaliabilityDate: " + a.FundAvaliabilityDate);
//                 Console.WriteLine("IncorrectTransactionID: " + a.IncorrectTransactionID);
//                 Console.WriteLine("Memo: " + a.Memo);
//                 Console.WriteLine("PayeeID: " + a.PayeeID);
//                 Console.WriteLine("ReferenceNumber: " + a.ReferenceNumber);
//                 Console.WriteLine("ServerTransactionID: " + a.ServerTransactionID);
//                 Console.WriteLine("Sic: " + a.Sic);
//                 Console.WriteLine("TransactionCorrectionAction: " + a.TransactionCorrectionAction);
//                 Console.WriteLine("TransactionID: " + a.TransactionID);
//                 Console.WriteLine("TransactionInitializationDate: " + a.TransactionInitializationDate);
//                 Console.WriteLine("TransactionSenderAccount: " + a.TransactionSenderAccount);
//                 Console.WriteLine("TransType: " + a.TransType);
//                 Console.WriteLine("----------------------------------------------");
//             }

//             Console.ReadLine();
//         }
//     }
// }
