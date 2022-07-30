using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EntegrasyonKuyruk.AppControl
{
    public class Tools
    {



        public class TokenISS
        {
            public string Token
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



        public string AppSettingsPropValue(ILog log, string key)
        {
            string value = "";
            try
            {
                value = ConfigurationManager.AppSettings[key];
            }
            catch (Exception ex)
            {
                log.Error(string.Concat("AppSettingsPropValue key:", key, "  Exception:", ex.Message));
                value = "";
            }
            log.Debug(string.Concat("AppSettingsPropValue key:", key, "  Value:", value));
            return value;
        }
        public JObject GetMapping(ILog log, string wSSettingsName)
        {
            JObject jObject = new JObject();
            try
            {
                string str = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/AppResonseMaps/", wSSettingsName, ".json");
                log.Info(string.Concat("GetMapping path", str));
                jObject = JObject.FromObject(JsonConvert.DeserializeObject<JObject>(File.ReadAllText(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/AppResonseMaps/", wSSettingsName, ".json"))));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("AppSettingsPropValue wSSettingsName:{0}  Exception:{1}", wSSettingsName, ex.Message));
            }
            return jObject;
        }
        public JObject HttpWebRequestSend(ILog log, int ticketQueueID, string endPointAddress, string bodyText, string headers, string requestTypeText, string RequestName)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            JObject jObject = new JObject();
            JObject headersJObject = JObject.Parse(headers);
            string strResponse = "";
            string strStatus = "NOT OK.";
            bool IsError = false;
            try
            {
                int timeOut = 100000;
                int.TryParse(AppSettingsPropValue(log, "TimeOut"), out timeOut);
                HttpWebRequest httpWebRequest  = (HttpWebRequest)WebRequest.Create(endPointAddress);
                httpWebRequest.Method = requestTypeText;
                httpWebRequest.Timeout = timeOut;
                foreach (JProperty header in headersJObject.Properties())
                {
                    if (header.Name!= "Content-Type")
                    {
                        httpWebRequest.Headers.Add(header.Name, header.Value.ToString());
                    }
                    else
                    {
                        httpWebRequest.ContentType = header.Value.ToString();
                    }
                }
                if (requestTypeText != "Get" && !string.IsNullOrEmpty(bodyText))
                {
                    byte[] bodyTextBytes = Encoding.UTF8.GetBytes(bodyText);
                    httpWebRequest.ContentLength = (long)((int)bodyTextBytes.Length);
                    Stream requestStream = httpWebRequest.GetRequestStream();
                    requestStream.Write(bodyTextBytes, 0, (int)bodyTextBytes.Length);
                    requestStream.Close();
                }
                try
                {
                    HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                    HttpStatusCode statusCode = response.StatusCode;
                    //log.Debug(string.Concat("**** ", RequestName, " \u00a0- response.StatusCode: ", statusCode.ToString()));
                    strStatus = response.StatusCode.ToString();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            strResponse = streamReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                        {
                            IsError = true;
                            strResponse = streamReader1.ReadToEnd();
                        }
                    }
                }
                catch (WebException webException)
                {
                    strStatus = webException.Status.ToString();
                    strResponse = webException.Message;
                }
            }
            catch (Exception ex)
            {
                strResponse = ex.Message;
            }
            jObject.Add("strStatus", strStatus);
            jObject.Add("strResponse", strResponse);
            jObject.Add("error", new JValue(!IsError));
            return jObject;
        }
        public JObject webServiceLogObject(ILog log, dynamic TicketQueue, string strResponsex, string statusx)
        {
            JObject jObject = new JObject();
            jObject.Add("object_name", "WSSettingsTicketLog");
            jObject.Add("IsDeleted", false);
            jObject.Add("BodyText", TicketQueue.BodyText.ToString());
            jObject.Add("Headers", TicketQueue.Headers.ToString());
            jObject.Add("WSSettings", (int)TicketQueue.WSSettings.WSSettingsID);
            jObject.Add("Ticket", (int)TicketQueue.Ticket.TicketID);
            jObject.Add("EndPointAddress", (string)TicketQueue.EndPointAddress);
            jObject.Add("RequestTypeText", (string)TicketQueue.RequestTypeText);
            jObject.Add("ReponseText", strResponsex);
            jObject.Add("IsRun", false);
            jObject.Add("StatusCodeText", statusx);
            return jObject;
        }
        public JObject webServiceTicketObject(int? WSSettingsLogin, int TicketID)
        {
            JObject jObject = new JObject();
            jObject.Add("object_name", "WSSettingsTicket");
            jObject.Add("IsDeleted", false);
            jObject.Add("WSSettings", WSSettingsLogin);
            jObject.Add("Ticket", TicketID);
            return jObject;
        }

        public JObject wSSettingsTicketQueueUpdeteObject(ILog log, int ticketQueueID, string lastReponseText, string lastStatusCodeText, int? numberOfAttempts, bool isDeleted, bool isError)
        {
            if (numberOfAttempts != null)
                numberOfAttempts = 0;
            numberOfAttempts = numberOfAttempts + 1;
            JObject jObject = new JObject();
            jObject.Add("object_name", "WSSettingsTicketQueue");
            jObject.Add("WSSettingsTicketQueueID", ticketQueueID);
            jObject.Add("IsDeleted", isDeleted);
            jObject.Add("IsError", isError);
            if (!string.IsNullOrEmpty(lastReponseText))
            {
                jObject.Add("LastReponseText", lastReponseText);
            }
            if (!string.IsNullOrEmpty(lastStatusCodeText))
            {
                jObject.Add("LastStatusCodeText", lastStatusCodeText);
            }
            jObject.Add("NumberOfAttempts", numberOfAttempts);
            jObject.Add("DuzenlenmeTarihi", DateTime.Now.ToString());
            return jObject;
        }

        public dynamic jObjectPropVal(ILog log, string jObject, string propx)
        {
            object obj = new object();
            try
            {
                dynamic obj1 = JObject.Parse(jObject);
                int ınt32 = 0;
                string[] strArrays = propx.Split(new char[] { '.' });
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    string str = strArrays[i];
                    ınt32++;
                    obj1 = obj1[str];
                    try
                    {
                        if (ınt32 != (int)propx.Split(new char[] { '.' }).Length)
                        {
                            obj1 = JObject.Parse(obj1.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                obj = obj1;
            }
            catch (Exception ex)
            {
                log.Error(string.Concat("****  jObjectPropVal propx:", propx, "-  Exception:", ex.ToString()));
                obj = null;
            }
            return obj;
        }

    }
}
