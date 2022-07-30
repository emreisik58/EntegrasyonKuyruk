using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EntegrasyonKuyruk.AppControl
{
    public class SpidyaQuery
    {
        public string sId { get; set; }

        public string SpidyaHttpWebRequest(ILog log, string method, string requestBody, string Prop, Tools tools)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string strResponse = "";
            bool isError = false;
            string url = tools.AppSettingsPropValue(log, "SpidyaServiceUrl");
            int timeOut = 100000;
            int.TryParse(tools.AppSettingsPropValue(log, "TimeOut"), out timeOut);
            try
            {
                try
                {
                    url = string.Concat(url, method);
                    HttpWebRequest httpWebRequest  = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.Timeout = timeOut;
                    log.Debug(string.Concat("SpidyaHttpWebRequest SpidyaServiceUrl:", url));
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    log.Debug(string.Concat("SpidyaHttpWebRequest requestBody:", requestBody));
                    byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBody);
                    httpWebRequest.ContentLength = (long)((int)requestBodyBytes.Length);
                    Stream requestStream = httpWebRequest.GetRequestStream();
                    requestStream.Write(requestBodyBytes, 0, (int)requestBodyBytes.Length);
                    requestStream.Close();
                    HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        strResponse = response.StatusCode.ToString();
                        isError = true;
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            strResponse = streamReader.ReadToEnd();
                        }
                        log.Error(string.Concat("SpidyaHttpWebRequest response.strResponsex:", strResponse));
                    }
                    else
                    {
                        using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                        {
                            strResponse = streamReader1.ReadToEnd();
                        }
                        log.Debug(string.Concat("SpidyaHttpWebRequest response.strResponsex:", strResponse));
                    }
                }
                catch (WebException webEx)
                {
                    isError = true;
                    log.Error(string.Concat("SpidyaHttpWebRequest response.WebException:", webEx.ToString()));
                    if (webEx.Response != null)
                    {
                        using (HttpWebResponse httpWebResponse = (HttpWebResponse)webEx.Response)
                        {
                            using (StreamReader streamReader2 = new StreamReader(httpWebResponse.GetResponseStream()))
                            {
                                strResponse = streamReader2.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isError = true;
                log.Error(string.Concat("SpidyaHttpWebRequest Exception:", ex.Message));
            }
            log.Debug(string.Concat("SpidyaHttpWebRequest errorx:", isError.ToString()));
            if (isError)
            {
                return null;
            }
            JObject jObject = JsonConvert.DeserializeObject<JObject>(strResponse);
            log.Debug(string.Concat("SpidyaHttpWebRequest IsSuccessful:", jObject["IsSuccessful"].ToString()));
            if (bool.Parse(jObject["IsSuccessful"].ToString()))
            {
                log.Debug(string.Concat("SpidyaHttpWebRequest response.strResponsex:", jObject["Message"].ToString()));
                return jObject[Prop].ToString();
            }
            log.Error(string.Concat("SpidyaHttpWebRequest response.Message:", jObject["Message"].ToString()));
            sId = null;
            return null;
        }


        public string spidyaCreateObject(ILog log, string object_name, JObject obj, string primarykey, Tools tools)
        {
            string pkUId;
            if (sId == null)
            {
                spidyaLogin(log, tools);
            }
            log.Debug(string.Concat("spidyaCreateObject sId:", sId));
            try
            {
                JObject jObject = new JObject();
                jObject.Add("sessionId", sId);
                jObject.Add("obj", JsonConvert.SerializeObject(obj));
                string Message = SpidyaHttpWebRequest(log, "createObject", JsonConvert.SerializeObject(jObject), "Message", tools);
                log.Debug(string.Concat("spidyaCreateObject msg:", Message));
                pkUId = JsonConvert.DeserializeObject<JObject>(Message)[primarykey].ToString();
            }
            catch (Exception exception)
            {
                log.Error(string.Concat("spidyaCreateObject Exception:", exception.ToString()));
                pkUId = null;
            }
            return pkUId;
        }

        public void spidyaLogin(ILog log, Tools tools)
        {
            try
            {
                string userName = tools.AppSettingsPropValue(log, "User");
                string password = tools.AppSettingsPropValue(log, "Password");
                JObject jObject = new JObject();
                jObject.Add("username", userName);
                jObject.Add("pwd", password);
                sId = this.SpidyaHttpWebRequest(log, "Login", JsonConvert.SerializeObject(jObject), "Message", tools);
            }
            catch (Exception ex)
            {
                log.Error(string.Concat("spidyaLogin Exception:", ex.ToString()));
                sId = null;
            }
        }

        public string spidyaUpdateObject(ILog log, string object_name, JObject obj, string primarykey, Tools tools)
        {
            string response = null;
            if (sId == null)
            {
                spidyaLogin(log, tools);
            }
            log.Debug(string.Concat("spidyaCreateObject sId:", sId));
            try
            {
                JObject jObject = new JObject();
                jObject.Add("sessionId", sId);
                jObject.Add("obj", JsonConvert.SerializeObject(obj));
                response = SpidyaHttpWebRequest(log, "updateObject", JsonConvert.SerializeObject(jObject), "Message", tools);
                if (string.IsNullOrEmpty(response) || !(response == "Operation has been successful."))
                {
                    response = (string.IsNullOrEmpty(response) ? false.ToString() : response);
                }
                else
                {
                    response = true.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Concat("spidyaCreateObject Exception:", ex.Message));
                response = null;
            }
            return response;
        }

        public string SpidyaGetObjectList(ILog log, string object_name, string where, Tools tools)
        {
            string response =null;
            if (sId == null)
            {
                spidyaLogin(log, tools);
            }
            log.Debug(string.Concat("SpidyaGetObjectList sId:", sId));
            try
            {
                JObject jObject = new JObject();
                jObject.Add("sessionId", sId);
                JObject jObject1 = new JObject();
                jObject1.Add("object_name", object_name);
                jObject1.Add("where", where);
                jObject.Add("query", JsonConvert.SerializeObject(jObject1));
                response = SpidyaHttpWebRequest(log, "getObjectList", JsonConvert.SerializeObject(jObject), "Message", tools);
                try
                {
                    log.Info(string.Concat("SpidyaGetObjectList msg:", response.Substring(0, 25), "....."));
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception exception1)
            {
                log.Error(string.Concat("SpidyaGetObjectList Exception:", exception1.ToString()));
                response = null;
            }
            return response;
        }


    }
}
