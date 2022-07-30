using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace EntegrasyonKuyruk.AppControl
{
    public class Tools
    {
        private List<TokenISS> tokenISS = new List<Tools.TokenISS>();

        public string AppSettingsPropValue(ILog log, string key)
        {
            string ıtem = "";
            try
            {
                ıtem = ConfigurationManager.AppSettings[key];
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                log.Error(string.Concat("AppSettingsPropValue key:", key, "  Exception:", exception.Message));
                ıtem = "";
            }
            log.Debug(string.Concat("AppSettingsPropValue key:", key, "  Value:", ıtem));
            return ıtem;
        }

        public JObject GetMapping(ILog log, string wSSettingsName)
        {
            JObject jObject;
            try
            {
                string str = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/AppResonseMaps/", wSSettingsName, ".json");
                log.Info(string.Concat("GetMapping path", str));
                jObject = JObject.FromObject(JsonConvert.DeserializeObject<JObject>(File.ReadAllText(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "/AppResonseMaps/", wSSettingsName, ".json"))));
            }
            catch (Exception exception)
            {
                throw;
            }
            return jObject;
        }

        public JObject HttpWebRequestSend(ILog log, int ticketQueueID, string endPointAddress, string bodyText, string headers, string requestTypeText, string RequestName)
        {
            JObject jObject = new JObject();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            log.Debug(string.Concat("**** ", RequestName, " \u00a0- endPointAddress:", endPointAddress));
            log.Debug(string.Concat("**** ", RequestName, " \u00a0- BodyText:", bodyText));
            log.Debug(string.Concat("**** ", RequestName, " \u00a0- headers:", headers));
            log.Debug(string.Concat("**** ", RequestName, " \u00a0- requestTypeText:", requestTypeText));
            JObject jObject1 = new JObject();
            try
            {
                jObject1 = JObject.Parse(headers);
            }
            catch (Exception exception)
            {
            }
            string end = "";
            string str = "NOT OK.";
            bool flag = false;
            try
            {
                HttpWebRequest length = (HttpWebRequest)WebRequest.Create(endPointAddress);
                length.Method = requestTypeText;
                int ınt32 = 100000;
                int.TryParse(AppSettingsPropValue(log, "TimeOut"), out ınt32);
                length.Timeout = ınt32;
                foreach (JProperty jProperty in jObject1.Properties())
                {
                    if (jProperty.Name != "Content-Type")
                    {
                        length.Headers.Add(jProperty.Name, jProperty.Name.ToString());
                    }
                    else
                    {
                        length.ContentType = jProperty.Name.ToString();
                    }
                }
                if (requestTypeText != "Get" && !string.IsNullOrEmpty(bodyText))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(bodyText);
                    length.ContentLength = (long)((int)bytes.Length);
                    Stream requestStream = length.GetRequestStream();
                    requestStream.Write(bytes, 0, (int)bytes.Length);
                    requestStream.Close();
                }
                try
                {
                    HttpWebResponse response = (HttpWebResponse)length.GetResponse();
                    HttpStatusCode statusCode = response.StatusCode;
                    log.Debug(string.Concat("**** ", RequestName, " \u00a0- response.StatusCode: ", statusCode.ToString()));
                    str = response.StatusCode.ToString();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            end = streamReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                        {
                            flag = true;
                            end = streamReader1.ReadToEnd();
                        }
                    }
                }
                catch (WebException webException)
                {
                    str = webException.Status.ToString();
                    log.Info(string.Concat("**** ", RequestName, " \u00a0- \u00a0WebException StatusCode:", str));
                    end = webException.Message;
                    log.Info(string.Concat("**** ", RequestName, " \u00a0- WebException strResponse:", end));
                }
            }
            catch (Exception exception2)
            {
                Exception exception1 = exception2;
                log.Info(string.Concat("**** ", RequestName, " \u00a0- \u00a0Exception:", exception1.ToString()));
                end = exception1.Message;
            }
            jObject.Add("strStatus", str);
            jObject.Add("strResponse", end);
            jObject.Add("error", new JValue(!flag));
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
                    catch (Exception exception)
                    {
                    }
                }
                obj = obj1;
            }
            catch (Exception exception2)
            {
                Exception exception1 = exception2;
                log.Error(string.Concat("****  jObjectPropVal propx:", propx, "-  Exception:", exception1.ToString()));
                obj = null;
            }
            return obj;
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
            try
            {
                jObject.Add("object_name", "WSSettingsTicket");
                jObject.Add("IsDeleted", false);
                jObject.Add("WSSettings", WSSettingsLogin);
                jObject.Add("Ticket", TicketID);
            }
            catch (Exception exception)
            {
                throw;
            }
            return jObject;
        }

        public JObject wSSettingsTicketQueueUpdeteObject(ILog log, int ticketQueueID, string lastReponseText, string lastStatusCodeText, int? numberOfAttempts, bool isDeleted, bool isError)
        {
            int? nullable;
            int? nullable1;
            if (numberOfAttempts.HasValue)
            {
                nullable = numberOfAttempts;
                if (nullable.HasValue)
                {
                    nullable1 = new int?(nullable.GetValueOrDefault() + 1);
                }
                else
                {
                    nullable1 = null;
                }
                numberOfAttempts = nullable1;
            }
            else
            {
                numberOfAttempts = new int?(1);
            }
            string[] str = new string[] { "**** ticketQueueID:", ticketQueueID.ToString(), " lastStatusCodeText:", lastStatusCodeText, " numberOfAttempts:", null, null, null };
            nullable = numberOfAttempts;
            str[5] = nullable.ToString();
            str[6] = " lastReponseText:";
            str[7] = lastReponseText;
            log.Info(string.Concat(str));
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
    }
}