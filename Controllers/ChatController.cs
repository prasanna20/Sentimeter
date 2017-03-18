using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_JQuery_Chat.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentimeter.Models;
using System.Data.Entity;
using System.Collections;

namespace MVC_JQuery_Chat.Controllers
{
public class ChatController : Controller
{

    static ChatModel chatModel;
        private const  string URLLink = "http://localhost:8080/sm/sentiment";
        string URL = HttpUtility.UrlDecode(URLLink);
       

        /// <summary>
        /// When the method is called with no arguments, just return the view
        /// When argument logOn is true, a user logged on
        /// When argument logOff is true, a user closed their browser or navigated away (log off)
        /// When argument chatMessage is specified, the user typed something in the chat
        /// </summary>
 public ActionResult Index(string user,string Asset, bool? logOn, bool? logOff, string chatMessage)
    {
        try
        {
            if (chatModel == null) chatModel = new ChatModel();
                
            //trim chat history if needed
            if (chatModel.ChatHistory.Count > 100)
                chatModel.ChatHistory.RemoveRange(0, 90);

            if (!Request.IsAjaxRequest())
            {
                //first time loading
                return View(chatModel);
            }
            else if (logOn != null && (bool)logOn)
            {
                //check if nickname already exists
                if (chatModel.Users.FirstOrDefault(u => u.NickName == user) != null)
                {
                    throw new Exception("This nickname already exists");
                }
                else if (chatModel.Users.Count > 10)
                {
                    throw new Exception("The room is full!");
                }
                else
                {
                    #region create new user and add to lobby
                    chatModel.Users.Add( new ChatModel.ChatUser()
                    {
                        NickName = user,
                        assetid = Asset,
                        LoggedOnTime = DateTime.Now,
                        LastPing = DateTime.Now
                    });

                    //inform lobby of new user
                    //chatModel.ChatHistory.Add(new ChatModel.ChatMessage()
                    //{
                    //    //Message = "User '" + user + "' logged on.",
                    //    //When = DateTime.Now
                    //});
                    #endregion

                }

                return PartialView("Lobby", chatModel);
            }
            else if (logOff != null && (bool)logOff)
            {
                LogOffUser( chatModel.Users.FirstOrDefault( u=>u.NickName==user) );
                return PartialView("Lobby", chatModel);
            }
            else
            {

                ChatModel.ChatUser currentUser = chatModel.Users.FirstOrDefault(u => u.NickName == user);
                //remember each user's last ping time
                currentUser.LastPing = DateTime.Now;

                #region remove inactive users
                List<ChatModel.ChatUser> removeThese = new List<ChatModel.ChatUser>();
                foreach (Models.ChatModel.ChatUser usr in chatModel.Users)
                {
                    TimeSpan span = DateTime.Now - usr.LastPing;
                    if (span.TotalSeconds > 15)
                        removeThese.Add(usr);
                }
                foreach (ChatModel.ChatUser usr in removeThese)
                {
                    LogOffUser(usr);
                }
                #endregion
        
                #region if there is a new message, append it to the chat
                if (!string.IsNullOrEmpty(chatMessage))
                {
                        decimal? positivityOutput = 0;
                        int sentimeterOutput = 0;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                        request.Method = "POST";
                        request.ContentType = "application/json";
                        request.ContentLength = chatMessage.Length;
                        //request.ProtocolVersion = HttpVersion.Version10; // fix 1
                        //request.KeepAlive = false; // fix 2
                        //request.Timeout = 1000000000; // fix 3
                        //request.ReadWriteTimeout = 1000000000; // fix 4
                        using (Stream webStream = request.GetRequestStream())
                        using (StreamWriter requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
                        {
                            requestWriter.Write(chatMessage);
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
                                        var data = (JObject)JsonConvert.DeserializeObject(response);

                                        positivityOutput = data["positivity"].Value<decimal>();
                                        sentimeterOutput = data["sentimeter"].Value<int>();
                                    }
                                }
                            }
                          
                            using (SentiAnalysisTable context = new SentiAnalysisTable())
                            {
                                SentiAnalysi dataModel = new SentiAnalysi();

                                dataModel.positivity = positivityOutput;
                                dataModel.AnalysisCode = sentimeterOutput;
                                dataModel.Comment = chatMessage;
                                dataModel.Assetid = currentUser.assetid;
                                context.Entry(dataModel).State = EntityState.Added;
                                context.SaveChanges();
                                
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Out.WriteLine("-----------------");
                            Console.Out.WriteLine(e.Message);
                        }
                        // Getvalues();
                        chatModel.ChatHistory.Add(new ChatModel.ChatMessage()
                        {
                            ByUser = currentUser,
                            Message = chatMessage,
                            When = DateTime.Now,
                            positivity = positivityOutput,
                            sentimeter =sentimeterOutput
                        });

                    }
                #endregion

                return PartialView("ChatHistory", chatModel);
            }
        }
        catch (Exception ex)
        {
            //return error to AJAX function
            Response.StatusCode = 500;
            return Content(ex.Message);
        }
    }

        public ActionResult ChatChart()
        {
            SentiAnalysisTable context = new SentiAnalysisTable();
            SentiAnalysi dataModel = new SentiAnalysi();
            
                var query = (from x in context.SentiAnalysis
                             select x);
            List<SentiAnalysi> SentiAnalysiData = new List<SentiAnalysi>();
            SentiAnalysiData = query.ToList();
            var AnalysisCode ="" ;
            var AssetID = "";

          

            foreach (string SentiAnalysi in SentiAnalysiData.Select(x => x.Assetid).Distinct())
                {
                AnalysisCode = AnalysisCode + SentiAnalysiData.Where(x => x.Assetid == SentiAnalysi).Count().ToString() + ',';

                    //AnalysisCode + (from x in SentiAnalysiData
                    //                           where x.Assetid.Equals(SentiAnalysi.Assetid)
                    //            select x.Assetid.Count() ).ToList().ToString();
                AssetID  = AssetID + SentiAnalysi + ',';
                }


            ViewBag.AnalysisCode = AnalysisCode.Trim();
            ViewBag.AssetID = AssetID.Trim();

            return View("ChatChart");


        }
            //static async void Getvalues()
            //{
            //    using (var client = new HttpClient())
            //    {
            //        client.BaseAddress = new Uri("http://localhost:8080/sm/sentiment/prasanna");
            //        client.DefaultRequestHeaders.Accept.Clear();
            //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //        HttpResponseMessage response = await client.GetAsync();
            //        if(response.IsSuccessStatusCode)
            //        {

            //        }

            //    }

            //}

            /// <summary>
            /// Remove this user from the lobby and inform others that he logged off
            /// </summary>
            /// <param name="user"></param>
            public void LogOffUser(ChatModel.ChatUser user)
    {
        chatModel.Users.Remove(user);
        //chatModel.ChatHistory.Add(new ChatModel.ChatMessage()
        //{
        //    Message = "User '" + user.NickName + "' logged off.",
        //    When = DateTime.Now
        //});
    }

}
}
