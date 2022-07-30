using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace EntegrasyonKuyruk.AppControl
{
    public class SpidyaQuery
    {
        public string sId;


        public string spidyaCreateObject(ILog log, string object_name, JObject obj, string primarykey, Tools tools)
        {
            string str;
            if (this.sId == null)
            {
                this.sId = this.spidyaLogin(log, tools);
            }
            log.Debug(string.Concat("spidyaCreateObject sId:", this.sId));
            try
            {
                JObject jObject = new JObject();
                jObject.Add("sessionId", this.sId);
                jObject.Add("obj", JsonConvert.SerializeObject(obj));
                string str1 = this.SpidyaHttpWebRequest(log, "createObject", JsonConvert.SerializeObject(jObject), "Message", tools);
                log.Debug(string.Concat("spidyaCreateObject msg:", str1));
                str = JsonConvert.DeserializeObject<JObject>(str1)[primarykey].ToString();
            }
            catch (Exception exception)
            {
                log.Error(string.Concat("spidyaCreateObject Exception:", exception.ToString()));
                str = null;
            }
            return str;
        }

        public string SpidyaGetObjectList(ILog log, string object_name, string where, Tools tools)
        {
            string str;
            if (this.sId == null)
            {
                this.sId = this.spidyaLogin(log, tools);
            }
            log.Debug(string.Concat("SpidyaGetObjectList sId:", this.sId));
            try
            {
                JObject jObject = new JObject();
                jObject.Add("sessionId", this.sId);
                JObject jObject1 = new JObject();
                jObject1.Add("object_name", object_name);
                jObject1.Add("where", where);
                jObject.Add("query", JsonConvert.SerializeObject(jObject1));
                string str1 = this.SpidyaHttpWebRequest(log, "getObjectList", JsonConvert.SerializeObject(jObject), "Message", tools);
                try
                {
                    log.Info(string.Concat("SpidyaGetObjectList msg:", str1.Substring(0, 25), "....."));
                }
                catch (Exception exception)
                {
                }
                str = str1;
            }
            catch (Exception exception1)
            {
                log.Error(string.Concat("SpidyaGetObjectList Exception:", exception1.ToString()));
                str = null;
            }
            return str;
        }

        public string SpidyaHttpWebRequest(ILog log, string Method, string requestBody, string Prop, Tools tools)
        {
            HttpStatusCode statusCode;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string str = "";
            bool flag = false;
            string str1 = tools.AppSettingsPropValue(log, "SpidyaServiceUrl");
            int ınt32 = 100000;
            int.TryParse(tools.AppSettingsPropValue(log, "TimeOut"), out ınt32);
            try
            {
                try
                {
                    str1 = string.Concat(str1, Method);
                    HttpWebRequest length = (HttpWebRequest)WebRequest.Create(str1);
                    length.Timeout = ınt32;
                    log.Debug(string.Concat("SpidyaHttpWebRequest SpidyaServiceUrl:", str1));
                    length.ContentType = "application/json";
                    length.Method = "POST";
                    log.Debug(string.Concat("SpidyaHttpWebRequest requestBody:", requestBody));
                    byte[] bytes = Encoding.UTF8.GetBytes(requestBody);
                    length.ContentLength = (long)((int)bytes.Length);
                    Stream requestStream = length.GetRequestStream();
                    requestStream.Write(bytes, 0, (int)bytes.Length);
                    requestStream.Close();
                    HttpWebResponse response = (HttpWebResponse)length.GetResponse();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        statusCode = response.StatusCode;
                        log.Error(string.Concat("SpidyaHttpWebRequest response.StatusCode:", statusCode.ToString()));
                        str = response.StatusCode.ToString();
                        flag = true;
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            str = streamReader.ReadToEnd();
                        }
                        log.Error(string.Concat("SpidyaHttpWebRequest response.strResponsex:", str));
                    }
                    else
                    {
                        statusCode = response.StatusCode;
                        log.Debug(string.Concat("SpidyaHttpWebRequest response.StatusCode:", statusCode.ToString()));
                        using (StreamReader streamReader1 = new StreamReader(response.GetResponseStream()))
                        {
                            str = streamReader1.ReadToEnd();
                        }
                        log.Debug(string.Concat("SpidyaHttpWebRequest response.strResponsex:", str));
                    }
                }
                catch (WebException webException1)
                {
                    WebException webException = webException1;
                    flag = true;
                    log.Error(string.Concat("SpidyaHttpWebRequest response.WebException:", webException.ToString()));
                    if (webException.Response != null)
                    {
                        using (HttpWebResponse httpWebResponse = (HttpWebResponse)webException.Response)
                        {
                            using (StreamReader streamReader2 = new StreamReader(httpWebResponse.GetResponseStream()))
                            {
                                str = streamReader2.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                flag = true;
                log.Error(string.Concat("SpidyaHttpWebRequest Exception:", exception.ToString()));
            }
            log.Debug(string.Concat("SpidyaHttpWebRequest errorx:", flag.ToString()));
            if (flag)
            {
                return null;
            }
            JObject jObject = JsonConvert.DeserializeObject<JObject>(str);
            log.Debug(string.Concat("SpidyaHttpWebRequest IsSuccessful:", jObject["IsSuccessful"].ToString()));
            if (bool.Parse(jObject["IsSuccessful"].ToString()))
            {
                log.Debug(string.Concat("SpidyaHttpWebRequest response.strResponsex:", jObject["Message"].ToString()));
                return jObject[Prop].ToString();
            }
            log.Error(string.Concat("SpidyaHttpWebRequest response.Message:", jObject["Message"].ToString()));
            this.sId = null;
            return null;
        }

        public string spidyaLogin(ILog log, Tools tools)
        {
            string str;
            try
            {
                string str1 = tools.AppSettingsPropValue(log, "User");
                string str2 = tools.AppSettingsPropValue(log, "Password");
                JObject jObject = new JObject();
                jObject.Add("username", str1);
                jObject.Add("pwd", str2);
                str = this.SpidyaHttpWebRequest(log, "Login", JsonConvert.SerializeObject(jObject), "Message", tools);
            }
            catch (Exception exception)
            {
                log.Error(string.Concat("spidyaLogin Exception:", exception.ToString()));
                str = null;
            }
            return str;
        }

        public string spidyaUpdateObject(ILog log, string object_name, JObject obj, string primarykey, Tools tools)
        {
            string str;
            if (this.sId == null)
            {
                this.sId = this.spidyaLogin(log, tools);
            }
            log.Debug(string.Concat("spidyaCreateObject sId:", this.sId));
            try
            {
                JObject jObject = new JObject();
                jObject.Add("sessionId", this.sId);
                jObject.Add("obj", JsonConvert.SerializeObject(obj));
                string str1 = this.SpidyaHttpWebRequest(log, "updateObject", JsonConvert.SerializeObject(jObject), "Message", tools);
                if (string.IsNullOrEmpty(str1) || !(str1 == "Operation has been successful."))
                {
                    str = (string.IsNullOrEmpty(str1) ? false.ToString() : str1);
                }
                else
                {
                    str = true.ToString();
                }
            }
            catch (Exception exception)
            {
                log.Error(string.Concat("spidyaCreateObject Exception:", exception.ToString()));
                str = null;
            }
            return str;
        }
    }
}