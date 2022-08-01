
using EntegrasyonKuyruk.AppControl;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace EntegrasyonKuyruk
{
    public class Program
    {
        private readonly static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static List<int> blockList = new List<int>();
        public static List<string> BodyList = new List<string>();
        public static SpidyaQuery spidyaQuery = new SpidyaQuery();
        public static Tools tools = new Tools();
        public static List<TokenISS> listTokenISS = new List<TokenISS>();
        public  class TokenISS
        {
            public string token
            {
                get;
                set;
            }

            public int? wSSettings
            {
                get;
                set;
            }

        }


        public static void Main(string[] args)
        {
            List<dynamic> queueList = GetQueueList();
            var queueListGroupedByTicketID = queueList.GroupBy(u => (int)u.Ticket.TicketID).ToList();
            foreach (var group in queueListGroupedByTicketID)
            {
                if (!blockList.Contains(group.Key))
                {

                    foreach (var queue in group)
                    {
                        QueueListProcess(queue);
                    }
                }

            }
            if (queueList.Count == 0)
            {
                log.Info("Main wSSettingsTicketQueue Tablosunda işlenecek kayıt bulunamadı....");
                return;
            }
            log.Info("Main *****  wSSettingsTicketQueue Tablosunda işlendi *****");




        }
        public static List<dynamic> GetQueueList()
        {
            List<dynamic> queueList = JsonConvert.DeserializeObject<List<dynamic>>(spidyaQuery.SpidyaGetObjectList(log, "WSSettingsTicketQueue", "PK_WSSETTINGSTICKETQUEUEID == 19696 and ISDELETED == false", Program.tools));
            return queueList;
        }

        public static bool QueueListProcess(dynamic ticketQueue)
        {
            //bool flag = false;
            bool isError = false;
            bool.TryParse(ticketQueue.IsError.ToString() ,out isError);
            log.Info("TicketID :" + ticketQueue.Ticket.TicketID + " isErrorr :" + isError.ToString());
            if (isError)
            {
                blockList.Add((int)ticketQueue.Ticket.TicketID);
                int numberOfAttempts = 0;
                try
                {
                    numberOfAttempts = (int)ticketQueue.NumberOfAttempts;
                }
                catch (Exception ex)
                {
                }
                JObject jObject = (JObject)Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)ticketQueue.WSSettingsTicketQueueID, ticketQueue.strResponse, ticketQueue.strStatus, numberOfAttempts, false, numberOfAttempts % 5 == 0);
                spidyaQuery.spidyaUpdateObject(Program.log, "", jObject, "", Program.tools);
            }
            else
            {
                if (BodyList.Contains((string)ticketQueue.BodyText))
                {
                    try
                    {
                        JObject jObject1 = Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)ticketQueue.WSSettingsTicketQueueID, "Hatalı Kayıt", "İçerik Benzerliği", (int?)ticketQueue.NumberOfAttempts, true, true);
                        Program.spidyaQuery.spidyaUpdateObject(Program.log, "", jObject1, "", Program.tools);
                        Program.log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + ticketQueue.WSSettingsTicketQueueID + " ticketId:" + ticketQueue.Ticket.TicketID);
                    }
                    catch (Exception ex)
                    {
                        log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + ticketQueue.WSSettingsTicketQueueID + " ex:" + ex.Message);
                    }
                    return false;
                }
                else if (Program.blockList.Contains((int)ticketQueue.Ticket.TicketID))
                {
                    BodyList.Add((string)ticketQueue.BodyText);
                    try
                    {
                        JObject ticketQueueUpdeteObject = tools.wSSettingsTicketQueueUpdeteObject(log, (int)ticketQueue.WSSettingsTicketQueueID, "Blocked.", "TicketID Blocked", (int?)ticketQueue.NumberOfAttempts, false, true);
                        spidyaQuery.spidyaUpdateObject(log, "", ticketQueueUpdeteObject, "", tools);
                        log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + ticketQueue.WSSettingsTicketQueueID + " ticketId:" + ticketQueue.Ticket.TicketID);
                    }
                    catch (Exception ex)
                    {
                        log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + ticketQueue.WSSettingsTicketQueueID + " ex:" + ex.Message);
                    }
                }
                else
                {
                    int? wSSettingsLoginID = (int?)ticketQueue.WSSettings.WSSettingsLogin;
                    Program.log.Debug("TicketID :" + ticketQueue.Ticket.TicketID + " wSSettingsLoginID :" + wSSettingsLoginID);
                    Program.log.Debug("TicketID :" + ticketQueue.Ticket.TicketID + " wSSettingsName :" + ticketQueue.WSSettings.Name);
                    string token = GetToken(wSSettingsLoginID, (int)ticketQueue.Ticket.TicketID);
                    try
                    {
                        ticketQueue.Headers = ticketQueue.Headers.ToString().Replace("@Token", string.Concat(" ", token));
                    }
                    catch (Exception ex)
                    {
                    }
                    try
                    {
                        ticketQueue.BodyText = ticketQueue.BodyText.ToString().Replace("@Token", string.Concat(" ", token));
                    }
                    catch (Exception ex)
                    {
                    }
                    dynamic wsStatus = tools.jObjectPropVal(log, (string)ticketQueue.BodyText, "Statu");
                    if (string.IsNullOrEmpty(wsStatus?.ToString()))
                    {
                        wsStatus = tools.jObjectPropVal(log, (string)ticketQueue.BodyText, "RecordStatus");
                        wsStatus = wsStatus?.ToString();
                    }
                    log.Info("TicketID :" + ticketQueue.Ticket.TicketID + " wsStatus :" + wsStatus);
                    if (wsStatus == "PLANLAMA" || wsStatus == "PLANLANDI")
                    {

                        JObject bodyJObject = JObject.Parse((string)ticketQueue.BodyText);
                        foreach (JProperty jProperty in bodyJObject.Properties())
                        {
                            if (jProperty.Name == "PlannedDateTime" || jProperty.Name == "PlanlamaTarihi")
                            {
                                string plannedDateTime = (string)jProperty.Value;
                                string[] times = plannedDateTime.Replace(" ","-").Replace(":","-").Replace(".","-").Split('-');
                                DateTime dateTime = new DateTime(
                                                                    int.Parse(times[2]),
                                                                    int.Parse(times[1]),
                                                                    int.Parse(times[0]),
                                                                    int.Parse(times[3]), 
                                                                    int.Parse(times[4]), 
                                                                    0
                                                                 );

                                if (dateTime < DateTime.Now)
                                {
                                    dateTime = DateTime.Now.AddMinutes(10);
                                }
                                jProperty.Value = dateTime.ToString("dd.MM.yyyy HH:mm");
                                ticketQueue.BodyText = Newtonsoft.Json.JsonConvert.SerializeObject(bodyJObject);
                                break;

                            }

                        } 



                    }
                    log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " BodyText :" + (string)ticketQueue.BodyText);

                    string webRequestReponse = JsonConvert.SerializeObject(tools.HttpWebRequestSend(log, (int)ticketQueue.WSSettingsTicketQueueID, (string)ticketQueue.EndPointAddress, (string)ticketQueue.BodyText, (string)ticketQueue.Headers, (string)ticketQueue.RequestTypeText, (string)ticketQueue.WSSettings.Name));

                    log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " webRequestReponse :" + webRequestReponse);

                    string strStatus = (string)((dynamic)tools.jObjectPropVal(log, webRequestReponse, "strStatus"));

                    log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " strStatus :" + strStatus);

                    string strResponse = (string)((dynamic)tools.jObjectPropVal(Program.log, webRequestReponse, "strResponse"));

                    log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " strResponse :" + strResponse);


                    bool error = (bool)((dynamic)tools.jObjectPropVal(log, webRequestReponse, "error"));
                    log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " error :" + error);
                    bool isErorr = false;
                    dynamic resonseCodeIsError = new object();
                    string objeName = "";
                    string objeIdProd = "";
                    string updateObjectIDProd = "";
                    if (!error)
                    {
                        try
                        {
                            JObject mapping = tools.GetMapping(log, (string)ticketQueue.WSSettings.Name);
                            dynamic code = tools.jObjectPropVal(log, webRequestReponse, mapping["ResonseCodeProp"].ToString());
                            dynamic message = tools.jObjectPropVal(log, webRequestReponse, mapping["ResonseMessageProp"].ToString());
                            objeName = mapping["UpdateObject"]["objeName"].ToString();
                            objeIdProd = mapping["UpdateObject"]["objeIdProd"].ToString();
                            updateObjectIDProd = mapping["UpdateObject"]["UpdateObjectIDProd"].ToString();
                            foreach (var item in mapping["ResonseCodeIsError"])
                            {
                                if (code.ToString() != item["value"].ToString())
                                {
                                    continue;
                                }
                                resonseCodeIsError = item;
                                error = (bool)item["IsError"];
                                isErorr = error;
                                if (((string)message).Contains("TAMAMLANDI ->"))
                                {
                                    if (((string)message).Contains("TAMAMLANDI -> T_ALINDI"))
                                    {
                                        break;
                                    }
                                    error = false;
                                    isErorr = true;
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " if TAMAMLANDI -> error :" + error);
                                    break;
                                }
                                else if (((string)message).Contains("PLANLANDI -> TAMAMLANDI"))
                                {
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " if PLANLANDI -> TAMAMLANDI ");
                                    JObject bodyTextJObject = JObject.Parse((string)ticketQueue.BodyText);
                                    bodyTextJObject.Remove("Statu");
                                    bodyTextJObject.Add("Statu", "BASLADI");
                                    ticketQueue.BodyText = JsonConvert.SerializeObject(bodyTextJObject);
                                    Program.QueueListProcess(ticketQueue);
                                    error = false;
                                    isErorr = false;
                                    break;
                                }
                                else if (!((string)message).Contains("ALINDI -> BASLADI"))
                                {
                                    if (!((string)message).Contains("GONDERILDI -> PLANLANDI"))
                                    {
                                        break;
                                    }
                                    log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " if GONDERILDI -> PLANLANDI ");
                                    JObject bodyTextJObject = JObject.Parse((string)ticketQueue.BodyText);
                                    bodyTextJObject.Remove("Statu");
                                    bodyTextJObject.Add("Statu", "ALINDI");
                                    ticketQueue.BodyText = JsonConvert.SerializeObject(bodyTextJObject);
                                    QueueListProcess(ticketQueue);
                                    error = false;
                                    isErorr = false;
                                    break;
                                }
                                else
                                {
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + ticketQueue.Ticket.TicketID + " if ALINDI -> BASLADI ");
                                    JObject bodyTextJObject = JObject.Parse((string)ticketQueue.BodyText);
                                    bodyTextJObject.Remove("Statu");
                                    bodyTextJObject.Add("Statu", "PLANLANDI");
                                    ticketQueue.BodyText = JsonConvert.SerializeObject(bodyTextJObject);
                                    Program.QueueListProcess(ticketQueue);
                                    error = false;
                                    isErorr = false;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {log.Error("QueueListProcess GetMapping wSSettingsTicketQueueID :" + ticketQueue.WSSettingsTicketQueueID + " ex:" + ex.Message);
                        }
                    }
                    if (error)
                    {
                        blockList.Add((int)ticketQueue.Ticket.TicketID);
                    }
                    JObject ticketQueueUpdeteObject = Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)ticketQueue.WSSettingsTicketQueueID, strResponse, strStatus, (int?)ticketQueue.NumberOfAttempts, !error, isErorr);
                    Program.spidyaQuery.spidyaUpdateObject(Program.log, "", ticketQueueUpdeteObject, "", Program.tools);
                    JObject serviceLogObject = (JObject)Program.tools.webServiceLogObject(Program.log, ticketQueue, strResponse, strStatus);
                    Program.spidyaQuery.spidyaCreateObject(Program.log, "WSSettingsTicketLog", serviceLogObject, "WSSettingsTicketLogID", Program.tools);
                    if (!error)
                    {
                        error = true;
                        Program.getUpdateTicket(Newtonsoft.Json.JsonConvert.SerializeObject(ticketQueue), resonseCodeIsError, objeName, objeIdProd, updateObjectIDProd, strResponse, error);
                    }
                }
            }
            return isError;
        }

        public static string GetToken(int? wSSettingsLogin, int ticketId)
        {
            try
            {
                return listTokenISS.Where(x => x.wSSettings == wSSettingsLogin).FirstOrDefault().token;
            }
            catch (Exception)
            {
                return GetTokenWs(wSSettingsLogin, ticketId);
            }

        }

        public static string GetTokenWs(int? wSSettingsLogin, int ticketId)
        {
            string token = "";
            try
            {
                JObject ticketObject = tools.webServiceTicketObject(wSSettingsLogin, ticketId);
                string createObjectID = Program.spidyaQuery.spidyaCreateObject(log, "WSSettingsTicket", ticketObject, "WSSettingsTicketID", tools);
                if (createObjectID != null)
                {
                    try
                    {
                        string getObjectListStr = spidyaQuery.SpidyaGetObjectList(log, "WSSettingsTicket", string.Concat("PK_WSSETTINGSTICKETID == ", createObjectID, " and ISDELETED == false"), tools);
                        token = (JsonConvert.DeserializeObject<List<dynamic>>(getObjectListStr).FirstOrDefault()).ResponseDescription?.ToString();
                    }
                    catch (Exception)
                    {

                    }



                }
            }
            catch (Exception ex) 
            {
            
            
            }
            Program.log.Info(string.Concat("GetTokenWs TicketID :", ticketId.ToString(), " token:", token));
            if (token != "") {

                listTokenISS.Add(new Program.TokenISS()
                {
                    token = token,
                    wSSettings = wSSettingsLogin
                });


            }

            return token;
        }

        public static void getUpdateTicket(dynamic ticketQueueStr, dynamic codeControls, string objeName, string objeIdProd, string UpdateObjectIDProd, string Responsed, bool isError)
        {
            JObject jObject = new JObject();
            int ticketQueue = (int)tools.jObjectPropVal(log, JsonConvert.SerializeObject(ticketQueueStr), UpdateObjectIDProd.ToString());
            log.Info(string.Concat("QueueListProcess getUpdateTicket  UpdateObjectID :", ticketQueue.ToString()));
            jObject.Add("object_name", objeName);
            jObject.Add(objeIdProd, ticketQueue);
            jObject.Add("IsDeleted", false);
            bool flag = false;
            foreach (dynamic codeControl in codeControls["UpdateProps"])
            {
                try
                {
                    if (codeControl["valueType"].ToString() == "string")
                    {
                        string str = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str = (string)((dynamic)tools.jObjectPropVal(log, Responsed, str)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), str.ToString());
                    }
                    else if (codeControl["valueType"].ToString() == "integer")
                    {
                        string str1 = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str1 = (string)((dynamic)tools.jObjectPropVal(log, Responsed, str1)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), int.Parse(str1));
                    }
                    else if (codeControl["valueType"].ToString() == "boolean")
                    {
                        string str2 = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str2 = (string)((dynamic)tools.jObjectPropVal(log, Responsed, str2)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), bool.Parse(str2));
                    }
                    else if (codeControl["valueType"].ToString() == "date")
                    {
                        string str3 = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str3 = (string)((dynamic)tools.jObjectPropVal(log, Responsed, str3)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), Convert.ToDateTime(str3));
                    }
                    flag = true;
                }
                catch (Exception ex)
                {
                    log.Error(string.Concat("QueueListProcess getUpdateTicket  :", ticketQueue.ToString(), " ex:", ex.Message));
                }
            }
            log.Info(string.Concat("QueueListProcess getUpdateTicket  updateJObject :", JsonConvert.SerializeObject(jObject)));
            if (flag)
            {
                spidyaQuery.spidyaUpdateObject(log, "", jObject, "", tools);
            }
        }


    }


}