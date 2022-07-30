using EntegrasyonKuyruk.AppControl;
using log4net;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EntegrasyonKuyruk
{
    internal class Program
    {
        private const int SW_HIDE = 0;

        private const int SW_SHOW = 5;

        private readonly static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static List<Program.TokenISS> tokenISS = new List<Program.TokenISS>();

        public static SpidyaQuery spidyaQuery = new SpidyaQuery();

        public static Tools tools = new Tools();

        public static List<int> blockList = new List<int>();

        public static List<string> BodyList = new List<string>();


        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern IntPtr GetConsoleWindow();

        public static List<dynamic> GetQueueList()
        {
            List<object> objs = JsonConvert.DeserializeObject<List<object>>(Program.spidyaQuery.SpidyaGetObjectList(Program.log, "WSSettingsTicketQueue", "PK_WSSETTINGSTICKETQUEUEID != 0 and ISDELETED == false", Program.tools));
            List<int> queueTicketList = Program.GetQueueTicketList(objs);
            List<object> objs1 = new List<object>();
            foreach (int ınt32 in queueTicketList)
            {
                objs1.AddRange(objs.Where<object>((object x) => {
                    if (Program.<> o__11.<> p__3 == null)
                    {
                        Program.<> o__11.<> p__3 = CallSite<Func<CallSite, object, bool>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(Program)));
                    }
                    !0 target = Program.<> o__11.<> p__3.Target;
                    CallSite<Func<CallSite, object, bool>> u003cu003ep_3 = Program.<> o__11.<> p__3;
                    if (Program.<> o__11.<> p__2 == null)
                    {
                        Program.<> o__11.<> p__2 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Equal, typeof(Program), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) })));
                    }
                    !0 _u00210 = Program.<> o__11.<> p__2.Target;
                    CallSite<Func<CallSite, object, int, object>> u003cu003ep_2 = Program.<> o__11.<> p__2;
                    if (Program.<> o__11.<> p__1 == null)
                    {
                        Program.<> o__11.<> p__1 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "TicketID", typeof(Program), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
                    }
                    !0 target1 = Program.<> o__11.<> p__1.Target;
                    CallSite<Func<CallSite, object, object>> u003cu003ep_1 = Program.<> o__11.<> p__1;
                    if (Program.<> o__11.<> p__0 == null)
                    {
                        Program.<> o__11.<> p__0 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Ticket", typeof(Program), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
                    }
                    return target(u003cu003ep_3, _u00210(u003cu003ep_2, target1(u003cu003ep_1, Program.<> o__11.<> p__0.Target(Program.<> o__11.<> p__0, x)), ınt32));
                }).ToList<object>());
            }
            return objs1;
        }

        public static List<int> GetQueueTicketList(List<dynamic> wSSettingsTicketQueueList)
        {
            List<int> ınt32s = new List<int>();
            foreach (dynamic obj in wSSettingsTicketQueueList)
            {
                if (ınt32s.Contains((int)obj.Ticket.TicketID))
                {
                    continue;
                }
                ınt32s.Add((int)obj.Ticket.TicketID);
            }
            return ınt32s;
        }

        public static string GetToken(int? wSSettingsLogin, int ticketId)
        {
            string str = "";
            if (wSSettingsLogin.HasValue && wSSettingsLogin.HasValue)
            {
                str = (Program.tokenISS.Any<Program.TokenISS>((Program.TokenISS x) => {
                    int? nullable = x.wSSettings;
                    int? nullable1 = wSSettingsLogin;
                    return nullable.GetValueOrDefault() == nullable1.GetValueOrDefault() & nullable.HasValue == nullable1.HasValue;
                }) ? Program.tokenISS.Find((Program.TokenISS x) => {
                    int? nullable = x.wSSettings;
                    int? nullable1 = wSSettingsLogin;
                    return nullable.GetValueOrDefault() == nullable1.GetValueOrDefault() & nullable.HasValue == nullable1.HasValue;
                }).Token : Program.GetTokenWs(wSSettingsLogin, ticketId));
            }
            return str;
        }

        public static string GetTokenWs(int? wSSettingsLogin, int ticketId)
        {
            string str = "";
            try
            {
                JObject jObject = Program.tools.webServiceTicketObject(wSSettingsLogin, ticketId);
                string str1 = Program.spidyaQuery.spidyaCreateObject(Program.log, "WSSettingsTicket", jObject, "WSSettingsTicketID", Program.tools);
                foreach (dynamic obj in JsonConvert.DeserializeObject<List<object>>(Program.spidyaQuery.SpidyaGetObjectList(Program.log, "WSSettingsTicket", string.Concat("PK_WSSETTINGSTICKETID == ", str1.ToString(), " and ISDELETED == false"), Program.tools)))
                {
                    str = (string)obj.ResponseDescription;
                    Program.log.Debug(string.Concat("GetTokenWs TicketID :", ticketId.ToString()));
                }
                Program.tokenISS.Add(new Program.TokenISS()
                {
                    Token = str,
                    wSSettings = wSSettingsLogin
                });
            }
            catch (Exception exception)
            {
                throw;
            }
            Program.log.Info(string.Concat("GetTokenWs TicketID :", ticketId.ToString(), " token:", str));
            return str;
        }

        public static void getUpdateTicket(dynamic TicketQueue, dynamic codeControls, string objeName, string objeIdProd, string UpdateObjectIDProd, string Responsed, bool isError)
        {
            JObject jObject = new JObject();
            int ticketQueue = (int)Program.tools.jObjectPropVal(Program.log, JsonConvert.SerializeObject(TicketQueue), UpdateObjectIDProd.ToString());
            Program.log.Info(string.Concat("QueueListProcess getUpdateTicket  UpdateObjectID :", ticketQueue.ToString()));
            jObject.Add("object_name", objeName);
            jObject.Add(objeIdProd, ticketQueue);
            jObject.Add("IsDeleted", false);
            bool flag = false;
            foreach (dynamic codeControl in (IEnumerable)codeControls["UpdateProps"])
            {
                try
                {
                    if (codeControl["valueType"].ToString() == "string")
                    {
                        string str = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str = (string)((dynamic)Program.tools.jObjectPropVal(Program.log, Responsed, str)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), str.ToString());
                    }
                    else if (codeControl["valueType"].ToString() == "integer")
                    {
                        string str1 = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str1 = (string)((dynamic)Program.tools.jObjectPropVal(Program.log, Responsed, str1)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), int.Parse(str1));
                    }
                    else if (codeControl["valueType"].ToString() == "boolean")
                    {
                        string str2 = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str2 = (string)((dynamic)Program.tools.jObjectPropVal(Program.log, Responsed, str2)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), bool.Parse(str2));
                    }
                    else if (codeControl["valueType"].ToString() == "date")
                    {
                        string str3 = (string)codeControl["serviceField"].ToString();
                        if (codeControl["FieldType"].ToString() != "static")
                        {
                            str3 = (string)((dynamic)Program.tools.jObjectPropVal(Program.log, Responsed, str3)).ToString();
                        }
                        jObject.Add(codeControl["pltField"].ToString(), Convert.ToDateTime(str3));
                    }
                    flag = true;
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    Program.log.Error(string.Concat("QueueListProcess getUpdateTicket  :", ticketQueue.ToString(), " ex:", exception.Message));
                }
            }
            Program.log.Info(string.Concat("QueueListProcess getUpdateTicket  updateJObject :", JsonConvert.SerializeObject(jObject)));
            if (flag)
            {
                Program.spidyaQuery.spidyaUpdateObject(Program.log, "", jObject, "", Program.tools);
            }
        }

        private static void Main(string[] args)
        {
            List<object> objs = new List<object>();
            List<object> queueList = Program.GetQueueList();
            foreach (dynamic obj in queueList)
            {
                if (Program.blockList.Contains((int)obj.Ticket.TicketID))
                {
                    objs.Add(obj);
                }
                else
                {
                    Program.QueueListProcess(obj);
                }
            }
            Parallel.ForEach<object>(objs, (object TicketQueue) => {
                if (Program.<> o__17.<> p__5 == null)
                {
                    Program.<> o__17.<> p__5 = CallSite<Action<CallSite, Type, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "QueueListProcess", null, typeof(Program), (IEnumerable<CSharpArgumentInfo>)(new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));
                }
                Program.<> o__17.<> p__5.Target(Program.<> o__17.<> p__5, typeof(Program), TicketQueue);
            });
            if (queueList.Count == 0)
            {
                Program.log.Info("Main wSSettingsTicketQueue Tablosunda işlenecek kayıt bulunamadı....");
                return;
            }
            Program.log.Info("Main *****  wSSettingsTicketQueue Tablosunda işlendi *****");
        }

        public static bool QueueListProcess(dynamic TicketQueue)
        {
            bool flag = false;
            bool flag1 = false;
            typeof(bool).TryParse(TicketQueue.IsError.ToString(), &flag1);
            Program.log.Info("TicketID :" + TicketQueue.Ticket.TicketID + " isErrorr :" + flag1.ToString());
            if (flag1)
            {
                Program.blockList.Add((int)TicketQueue.Ticket.TicketID);
                flag = true;
                int ticketQueue = 0;
                try
                {
                    ticketQueue = (int)TicketQueue.NumberOfAttempts;
                }
                catch (Exception exception)
                {
                }
                JObject jObject = (JObject)Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)TicketQueue.WSSettingsTicketQueueID, TicketQueue.strResponse, TicketQueue.strStatus, ticketQueue, false, ticketQueue % 5 == 0);
                Program.spidyaQuery.spidyaUpdateObject(Program.log, "", jObject, "", Program.tools);
            }
            else
            {
                if (Program.BodyList.Contains((string)TicketQueue.BodyText))
                {
                    try
                    {
                        JObject jObject1 = Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)TicketQueue.WSSettingsTicketQueueID, "Hatalı Kayıt", "İçerik Benzerliği", (int?)TicketQueue.NumberOfAttempts, true, true);
                        Program.spidyaQuery.spidyaUpdateObject(Program.log, "", jObject1, "", Program.tools);
                        Program.log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + TicketQueue.WSSettingsTicketQueueID + " ticketId:" + TicketQueue.Ticket.TicketID);
                    }
                    catch (Exception exception2)
                    {
                        Exception exception1 = exception2;
                        Program.log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + TicketQueue.WSSettingsTicketQueueID + " ex:" + exception1.Message);
                    }
                    return false;
                }
                Program.BodyList.Add((string)TicketQueue.BodyText);
                if (Program.blockList.Contains((int)TicketQueue.Ticket.TicketID))
                {
                    try
                    {
                        JObject jObject2 = Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)TicketQueue.WSSettingsTicketQueueID, "Blocked.", "TicketID Blocked", (int?)TicketQueue.NumberOfAttempts, false, true);
                        Program.spidyaQuery.spidyaUpdateObject(Program.log, "", jObject2, "", Program.tools);
                        Program.log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + TicketQueue.WSSettingsTicketQueueID + " ticketId:" + TicketQueue.Ticket.TicketID);
                    }
                    catch (Exception exception4)
                    {
                        Exception exception3 = exception4;
                        Program.log.Error("QueueListProcess Blocked wSSettingsTicketQueueID :" + TicketQueue.WSSettingsTicketQueueID + " ex:" + exception3.Message);
                    }
                }
                else
                {
                    int? nullable = (int?)TicketQueue.WSSettings.WSSettingsLogin;
                    Program.log.Debug("TicketID :" + TicketQueue.Ticket.TicketID + " wSSettingsLoginID :" + nullable);
                    Program.log.Debug("TicketID :" + TicketQueue.Ticket.TicketID + " wSSettingsName :" + TicketQueue.WSSettings.Name);
                    string token = Program.GetToken(nullable, (int)TicketQueue.Ticket.TicketID);
                    try
                    {
                        TicketQueue.Headers = TicketQueue.Headers.ToString().Replace("@Token", string.Concat(" ", token));
                    }
                    catch (Exception exception5)
                    {
                    }
                    try
                    {
                        TicketQueue.BodyText = TicketQueue.BodyText.ToString().Replace("@Token", string.Concat(" ", token));
                    }
                    catch (Exception exception6)
                    {
                    }
                    dynamic obj = Program.tools.jObjectPropVal(Program.log, (string)TicketQueue.BodyText, "Statu");
                    string str = (string)((obj != null ? obj.ToString() : null));
                    if (string.IsNullOrEmpty(str))
                    {
                        obj = Program.tools.jObjectPropVal(Program.log, (string)TicketQueue.BodyText, "RecordStatus");
                        str = (string)((obj != null ? obj.ToString() : null));
                    }
                    Program.log.Info("TicketID :" + TicketQueue.Ticket.TicketID + " wsStatus :" + str);
                    if (str == "PLANLAMA")
                    {
                        JObject jObject3 = JObject.Parse((string)TicketQueue.BodyText);
                        string ıtem = (string)jObject3.get_Item("PlannedDateTime");
                        string str1 = ıtem.Split(new char[] { ' ' })[0];
                        string str2 = ıtem.Split(new char[] { ' ' })[1];
                        int ınt32 = int.Parse(str1.Split(new char[] { '.' })[0]);
                        int ınt321 = int.Parse(str1.Split(new char[] { '.' })[1]);
                        int ınt322 = int.Parse(str1.Split(new char[] { '.' })[2]);
                        int ınt323 = int.Parse(str2.Split(new char[] { ':' })[0]);
                        int ınt324 = int.Parse(str2.Split(new char[] { ':' })[1]);
                        DateTime dateTime = new DateTime(ınt322, ınt321, ınt32, ınt323, ınt324, 0);
                        if (dateTime < DateTime.Now)
                        {
                            dateTime = DateTime.Now.AddMinutes(10);
                        }
                        jObject3.Remove("PlannedDateTime");
                        jObject3.Add("PlannedDateTime", dateTime.ToString("dd.MM.yyyy HH:mm"));
                        TicketQueue.BodyText = JsonConvert.SerializeObject(jObject3);
                    }
                    else if (str == "PLANLANDI")
                    {
                        JObject jObject4 = JObject.Parse((string)TicketQueue.BodyText);
                        string ıtem1 = (string)jObject4.get_Item("PlanlamaTarihi");
                        string str3 = ıtem1.Split(new char[] { ' ' })[0];
                        string str4 = ıtem1.Split(new char[] { ' ' })[1];
                        int ınt325 = int.Parse(str3.Split(new char[] { '.' })[0]);
                        int ınt326 = int.Parse(str3.Split(new char[] { '.' })[1]);
                        int ınt327 = int.Parse(str3.Split(new char[] { '.' })[2]);
                        int ınt328 = int.Parse(str4.Split(new char[] { ':' })[0]);
                        int ınt329 = int.Parse(str4.Split(new char[] { ':' })[1]);
                        DateTime dateTime1 = new DateTime(ınt327, ınt326, ınt325, ınt328, ınt329, 0);
                        if (dateTime1 < DateTime.Now)
                        {
                            dateTime1 = DateTime.Now.AddMinutes(10);
                        }
                        jObject4.Remove("PlanlamaTarihi");
                        jObject4.Add("PlanlamaTarihi", dateTime1.ToString("dd.MM.yyyy HH:mm"));
                        TicketQueue.BodyText = JsonConvert.SerializeObject(jObject4);
                    }
                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " BodyText :" + (string)TicketQueue.BodyText);
                    string str5 = JsonConvert.SerializeObject(Program.tools.HttpWebRequestSend(Program.log, (int)TicketQueue.WSSettingsTicketQueueID, (string)TicketQueue.EndPointAddress, (string)TicketQueue.BodyText, (string)TicketQueue.Headers, (string)TicketQueue.RequestTypeText, (string)TicketQueue.WSSettings.Name));
                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " Responsed :" + str5);
                    string str6 = (string)((dynamic)Program.tools.jObjectPropVal(Program.log, str5, "strStatus"));
                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " strStatus :" + str6);
                    string str7 = (string)((dynamic)Program.tools.jObjectPropVal(Program.log, str5, "strResponse"));
                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " strResponse :" + str7);
                    bool flag2 = (bool)((dynamic)Program.tools.jObjectPropVal(Program.log, str5, "error"));
                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " error :" + flag2);
                    bool flag3 = false;
                    dynamic obj1 = new object();
                    string str8 = "";
                    string str9 = "";
                    string str10 = "";
                    if (!flag2)
                    {
                        try
                        {
                            JObject mapping = Program.tools.GetMapping(Program.log, (string)TicketQueue.WSSettings.Name);
                            dynamic obj2 = Program.tools.jObjectPropVal(Program.log, str5, mapping.get_Item("ResonseCodeProp").ToString());
                            dynamic obj3 = Program.tools.jObjectPropVal(Program.log, str5, mapping.get_Item("ResonseMessageProp").ToString());
                            str8 = mapping.get_Item("UpdateObject").get_Item("objeName").ToString();
                            str9 = mapping.get_Item("UpdateObject").get_Item("objeIdProd").ToString();
                            str10 = mapping.get_Item("UpdateObject").get_Item("UpdateObjectIDProd").ToString();
                            foreach (JToken jToken in mapping.get_Item("ResonseCodeIsError"))
                            {
                                if (obj2.ToString() != jToken.get_Item("value").ToString())
                                {
                                    continue;
                                }
                                obj1 = jToken;
                                bool ıtem2 = (bool)jToken.get_Item("IsError");
                                flag2 = ıtem2;
                                flag3 = ıtem2;
                                if (((string)obj3).Contains("TAMAMLANDI ->"))
                                {
                                    if (((string)obj3).Contains("TAMAMLANDI -> T_ALINDI"))
                                    {
                                        break;
                                    }
                                    flag2 = false;
                                    flag3 = true;
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " if TAMAMLANDI -> error :" + flag2);
                                    break;
                                }
                                else if (((string)obj3).Contains("PLANLANDI -> TAMAMLANDI"))
                                {
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " if PLANLANDI -> TAMAMLANDI ");
                                    JObject jObject5 = JObject.Parse((string)TicketQueue.BodyText);
                                    jObject5.Remove("Statu");
                                    jObject5.Add("Statu", "BASLADI");
                                    TicketQueue.BodyText = JsonConvert.SerializeObject(jObject5);
                                    Program.QueueListProcess(TicketQueue);
                                    flag2 = false;
                                    flag3 = false;
                                    break;
                                }
                                else if (!((string)obj3).Contains("ALINDI -> BASLADI"))
                                {
                                    if (!((string)obj3).Contains("GONDERILDI -> PLANLANDI"))
                                    {
                                        break;
                                    }
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " if GONDERILDI -> PLANLANDI ");
                                    JObject jObject6 = JObject.Parse((string)TicketQueue.BodyText);
                                    jObject6.Remove("Statu");
                                    jObject6.Add("Statu", "ALINDI");
                                    TicketQueue.BodyText = JsonConvert.SerializeObject(jObject6);
                                    Program.QueueListProcess(TicketQueue);
                                    flag2 = false;
                                    flag3 = false;
                                    break;
                                }
                                else
                                {
                                    Program.log.Info("TicketQueue HttpWebRequestSend TicketID :" + TicketQueue.Ticket.TicketID + " if ALINDI -> BASLADI ");
                                    JObject jObject7 = JObject.Parse((string)TicketQueue.BodyText);
                                    jObject7.Remove("Statu");
                                    jObject7.Add("Statu", "PLANLANDI");
                                    TicketQueue.BodyText = JsonConvert.SerializeObject(jObject7);
                                    Program.QueueListProcess(TicketQueue);
                                    flag2 = false;
                                    flag3 = false;
                                    break;
                                }
                            }
                        }
                        catch (Exception exception8)
                        {
                            Exception exception7 = exception8;
                            Program.log.Error("QueueListProcess GetMapping wSSettingsTicketQueueID :" + TicketQueue.WSSettingsTicketQueueID + " ex:" + exception7.Message);
                        }
                    }
                    if (flag2)
                    {
                        Program.blockList.Add((int)TicketQueue.Ticket.TicketID);
                    }
                    JObject jObject8 = Program.tools.wSSettingsTicketQueueUpdeteObject(Program.log, (int)TicketQueue.WSSettingsTicketQueueID, str7, str6, (int?)TicketQueue.NumberOfAttempts, !flag2, flag3);
                    Program.spidyaQuery.spidyaUpdateObject(Program.log, "", jObject8, "", Program.tools);
                    JObject ticketQueue1 = (JObject)Program.tools.webServiceLogObject(Program.log, TicketQueue, str7, str6);
                    Program.spidyaQuery.spidyaCreateObject(Program.log, "WSSettingsTicketLog", ticketQueue1, "WSSettingsTicketLogID", Program.tools);
                    if (!flag2)
                    {
                        flag = true;
                        Program.getUpdateTicket(TicketQueue, obj1, str8, str9, str10, str5, flag2);
                    }
                }
            }
            return flag;
        }

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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

            public TokenISS()
            {
            }
        }
    }
}