using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentimeter.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Sentimeter.Controllers
{
    public class RiskController : Controller
    {
        //
        // GET: /Risk/
        private const string URLLink = "http://localhost:8080/sm/sentiment";
        string URL = HttpUtility.UrlDecode(URLLink);

        public ActionResult Index()
        {
            RiskSenti context = new RiskSenti();
            var query = (from x in context.RiskSentiAnalysis
                         group x by x.AssetId into a
                         select a.OrderByDescending(f => f.updateddate).FirstOrDefault()
                         ); 
            List<RiskSentiAnalysi> data =  query.ToList();

             return View(data);
        }

        public ActionResult Details(int Id)
        {
            RiskSenti context = new RiskSenti();
            RiskSentiAnalysi RiskSentiAnalysiData = new RiskSentiAnalysi();
            var query = (from x in context.RiskSentiAnalysis
                         where x.id == Id
                         select x  );
            foreach(RiskSentiAnalysi RiskSentiAnalysi in query.ToList())
            {
                RiskSentiAnalysiData = RiskSentiAnalysi;
                break;
            }

            return PartialView("_Details", RiskSentiAnalysiData);
        }
        public ActionResult updateComments(Sentimeter.Models.RiskSentiAnalysi data)
        {

            RiskSentiAnalysi dataModel = new RiskSentiAnalysi();


            //Checking the sentiments of new data - start
            decimal? positivityOutput = 0;
            int sentimeterOutput = 0;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Comment.Length;
            //request.ProtocolVersion = HttpVersion.Version10; // fix 1
            //request.KeepAlive = false; // fix 2
            //request.Timeout = 1000000000; // fix 3
            //request.ReadWriteTimeout = 1000000000; // fix 4
            using (Stream webStream = request.GetRequestStream())
            using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                requestWriter.Write(data.Comment);
            }

            try
            {
                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    if (webStream != null)
                    {
                        using (StreamReader responseReader = new StreamReader(webStream))
                        {
                            string response = responseReader.ReadToEnd();
                            var data2 = (JObject)JsonConvert.DeserializeObject(response);

                            positivityOutput = data2["positivity"].Value<decimal>();
                            sentimeterOutput = data2["sentimeter"].Value<int>();
                        }
                    }
                }

               
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("-----------------");
                Console.Out.WriteLine(e.Message);
            }

            //End



            using (RiskSenti context = new RiskSenti())
            {
                DateTime now = DateTime.Now;
                dataModel.CurrentStatus = data.CurrentStatus;
                dataModel.Comment = data.Comment;
                dataModel.id = data.id;
                dataModel.positivity = positivityOutput;
                dataModel.AnalysisCode = sentimeterOutput;
                dataModel.AssetId = data.AssetId;
                dataModel.updateddate = now;
                context.Entry(dataModel).State = EntityState.Added;
                context.SaveChanges();
            }

            RiskSenti context1 = new RiskSenti();
            var query = (from x in context1.RiskSentiAnalysis
                         group x by x.AssetId into a
                         select a.OrderByDescending(f => f.updateddate).FirstOrDefault()
                );
            List<RiskSentiAnalysi> data1 = query.ToList();

            return View("Index",data1);
        }

    }
}
