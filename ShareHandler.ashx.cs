using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace lc
{
    /// <summary>
    /// ShareHandler 的摘要说明
    /// 分享的一般处理程序
    /// </summary>
    public class ShareHandler : IHttpHandler
    {
        public static string APPID = "wxc46584f2997a0481"; //微信的APPID
        public static string APPSECRET = "c59e279b064464b24a364d8c1431d0b2"; //微信的Appsecret
        public static string ACCESS_TOKEN = "";//微信的ACCESS_TOKEN
        public static string JSAPI_TICKET = ""; //微信的JSAPI_TICKET

        //随机字符串取值区
        private static char[] constant =   
        {   
            '0','1','2','3','4','5','6','7','8','9',  
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',   
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'   
        };

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");

            string url = context.Request.Params["dataurl"];
            context.Response.Write(CreateContent(url));
        }

        /// <summary>
        /// 创建 分享的签名
        /// </summary>
        /// <returns></returns>
        public string CreateContent(string url) 
        {
            //第一 时间戳
            string timestamp = DateTime.Now.ToFileTime().ToString().Substring(0, 10);

            //第二 appid
            string wxappid = APPID;

            //第三 随机字符串
            string noncestr = GenerateRandomNumber(16);

            //第四 sign
            string rawstring = "jsapi_ticket=" + GetJSAPI_TICKET(GetACCESS_TOKEN()) + "&noncestr=" + noncestr + "&timestamp=" + timestamp + "&url=" + url + "";
            string signature = getSign(rawstring);

            //返回
            string str = "{\"timestamp\":\"" + timestamp + "\",\"wxappid\":\"" + wxappid + "\",\"noncestr\":\"" + noncestr + "\",\"strSige\":\"" + signature + "\",\"ACCESS_TOKEN\":\"" + ACCESS_TOKEN + "\",\"JSAPI_TICKET\":\"" + JSAPI_TICKET + "\",\"url\":\"" + url + "\",\"rawstring\":\"" + rawstring + "\"}";
            return str;
        }

        /// <summary>
        /// 获得微信的ACCESS_TOKEN
        /// </summary>
        /// <returns></returns>
        public string GetACCESS_TOKEN()
        {
            if (ACCESS_TOKEN.Length == 0)
            {

                string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", APPID, APPSECRET);
                WebClient wc = new WebClient();
                wc.Encoding = System.Text.Encoding.Default;
                string pageData = wc.DownloadString(url);
                JObject obj = (JObject)JsonConvert.DeserializeObject(pageData);
                if (obj["access_token"] != null)
                {
                    ACCESS_TOKEN = obj["access_token"].ToString();

                    System.Timers.Timer myTimer = new System.Timers.Timer();
                    myTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                    myTimer.Interval = 7199 * 1000; //7200秒
                    myTimer.Enabled = true;
                }
                else
                {
                    ACCESS_TOKEN = "";
                }
            }
            return ACCESS_TOKEN;
        }

        /// <summary>
        /// 获得微信的JSAPI_TICKET
        /// </summary>
        /// <returns></returns>
        public string GetJSAPI_TICKET(string access_token)
        {
            if (JSAPI_TICKET.Length == 0)
            {
                string url = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", access_token);
                WebClient wc = new WebClient();
                wc.Encoding = System.Text.Encoding.Default;
                string pageData = wc.DownloadString(url);
                JObject obj = (JObject)JsonConvert.DeserializeObject(pageData);
                if (obj["ticket"] != null)
                {
                    JSAPI_TICKET = obj["ticket"].ToString();

                    System.Timers.Timer myTimer = new System.Timers.Timer();
                    myTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent1);
                    myTimer.Interval = 7199 * 1000; //7200秒
                    myTimer.Enabled = true;
                }
                else
                {
                    JSAPI_TICKET = "";
                }
            }
            return JSAPI_TICKET;
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateRandomNumber(int length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }

        /// <summary>
        /// 获得签名
        /// </summary>
        /// <param name="str_sha1_in"></param>
        /// <returns></returns>
        public string getSign(string str_sha1_in)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_sha1_in = System.Text.UTF8Encoding.Default.GetBytes(str_sha1_in);
            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
            string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
            str_sha1_out = str_sha1_out.Replace("-", "").ToLower();
            return str_sha1_out;
        }

        public void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            ACCESS_TOKEN = "";
        }

        public void OnTimedEvent1(object source, System.Timers.ElapsedEventArgs e)
        {
            JSAPI_TICKET = "";
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}