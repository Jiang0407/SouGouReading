using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;  

namespace webbrowser代理ip
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //重要！
            //重要！根据线程实际使用情况调整。
            //修改HTTP请求默认连接数，默认是2。
            System.Net.ServicePointManager.DefaultConnectionLimit = 64;


            //遇到417错误请使用以下代码。
            //System.Net.ServicePointManager.Expect100Continue = false;
        }      

        #region 设置代理IP
        private void button2_Click(object sender, EventArgs e)
        {
            string proxy = this.textBox1.Text;
            RefreshIESettings(proxy);
            IEProxy ie = new IEProxy(proxy); 
        }
        #endregion
        #region 取消代理IP
        private void button3_Click(object sender, EventArgs e)
        {
            IEProxy ie = new IEProxy(null);
            ie.DisableIEProxy();
        }
        #endregion
        #region 打开网页
        private void button1_Click(object sender, EventArgs e)
        {
            this.webBrowser1.Navigate("http://www.ip138.com/", null, null, null);
        }
        #endregion
        #region 代理IP
        public struct Struct_INTERNET_PROXY_INFO
        {
            public int dwAccessType;
            public IntPtr proxy;
            public IntPtr proxyBypass;
        };
        //strProxy为代理IP:端口
        private void RefreshIESettings(string strProxy)
        {
            const int INTERNET_OPTION_PROXY = 38;
            const int INTERNET_OPEN_TYPE_PROXY = 3;
            const int INTERNET_OPEN_TYPE_DIRECT = 1;

            Struct_INTERNET_PROXY_INFO struct_IPI;
            // Filling in structure
            struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
            struct_IPI.proxy = Marshal.StringToHGlobalAnsi(strProxy);
            struct_IPI.proxyBypass = Marshal.StringToHGlobalAnsi("local");

            // Allocating memory
            IntPtr intptrStruct = Marshal.AllocCoTaskMem(Marshal.SizeOf(struct_IPI));
            if (string.IsNullOrEmpty(strProxy) || strProxy.Trim().Length == 0)
            {
                strProxy = string.Empty;
                struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_DIRECT;

            }
            // Converting structure to IntPtr
            Marshal.StructureToPtr(struct_IPI, intptrStruct, true);

            bool iReturn = InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, Marshal.SizeOf(struct_IPI));
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
        public class IEProxy
        {
            private const int INTERNET_OPTION_PROXY = 38;
            private const int INTERNET_OPEN_TYPE_PROXY = 3;
            private const int INTERNET_OPEN_TYPE_DIRECT = 1;

            private string ProxyStr;


            [DllImport("wininet.dll", SetLastError = true)]

            private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);

            public struct Struct_INTERNET_PROXY_INFO
            {
                public int dwAccessType;
                public IntPtr proxy;
                public IntPtr proxyBypass;
            }

            private bool InternetSetOption(string strProxy)
            {
                int bufferLength;
                IntPtr intptrStruct;
                Struct_INTERNET_PROXY_INFO struct_IPI;

                if (string.IsNullOrEmpty(strProxy) || strProxy.Trim().Length == 0)
                {
                    strProxy = string.Empty;
                    struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_DIRECT;
                }
                else
                {
                    struct_IPI.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
                }
                struct_IPI.proxy = Marshal.StringToHGlobalAnsi(strProxy);
                struct_IPI.proxyBypass = Marshal.StringToHGlobalAnsi("local");
                bufferLength = Marshal.SizeOf(struct_IPI);
                intptrStruct = Marshal.AllocCoTaskMem(bufferLength);
                Marshal.StructureToPtr(struct_IPI, intptrStruct, true);
                return InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, intptrStruct, bufferLength);

            }
            public IEProxy(string strProxy)
            {
                this.ProxyStr = strProxy;
            }
            //设置代理
            public bool RefreshIESettings()
            {
                return InternetSetOption(this.ProxyStr);
            }
            //取消代理
            public bool DisableIEProxy()
            {
                return InternetSetOption(string.Empty);
            }
        }
        #endregion

        private void button4_Click(object sender, EventArgs e)
        {
            string web = this.textBox2.Text;
            this.webBrowser1.Navigate("http://"+web, null, null,null);
            if (webBrowser1.Document == null)
                statebox.AppendText(web+"连接失败");
        } 
        #region 提取并验证代理ip

        public static int MultiThreadCount;
        public List<string> IP = new List<string>();
        public List<string> IP_ok = new List<string>();
        public List<string> IP_done = new List<string>();
        System.Timers.Timer timer;
        int timercount;
        private void IPCheckThread(object obj)
        {
            string n = (string)obj;
            int num = Convert.ToInt16(n);
            int size = (int)IP.Count / Convert.ToInt16(MutiThread.Text);
            List<string> tp_tmp = checkIP(num * size, (num + 1) * size);
            if (tp_tmp.Count != 0)
            {
                Object thisLock = new Object();
                lock (thisLock)
                {
                    foreach (string m in tp_tmp)
                    {
                        IP_ok.Add(m);
                    }
                }
            }
            MultiThreadCount++;
        }
        private List<string> checkIP(int s, int e)
        {
            List<string> ret = new List<string>();
            for (int i = s; i < e; i++)
            {
                string[] sArr = IP[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                WebProxy proxyObject = new WebProxy(sArr[0], Convert.ToInt16(sArr[1]));// port为端口号 整数型
                WebRequest Req = WebRequest.Create("http://www.baidu.com") as HttpWebRequest;
                Req.Proxy = proxyObject; //设置代理
                Req.Timeout = 5000;   //超时
                HttpWebResponse Resp;
                try
                {
                    Resp = (HttpWebResponse)Req.GetResponse();                
                    Encoding bin = Encoding.GetEncoding("UTF-8");
                    StreamReader sr = new StreamReader(Resp.GetResponseStream(), bin);
                    string str = sr.ReadToEnd();
                    if (str.Contains("百度"))
                    {
                        ret.Add(IP[i]);
                    } 
                    sr.Close();
                    sr.Dispose();
                }
                catch
                {
                    continue;
                }
               
            }
            return ret;
        } 
        private void button5_Click(object sender, EventArgs e)
        {

            string address = "http://www.youdaili.net/Daili/http/6953.html";   
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
            req.Method = "GET";
            WebResponse wr = req.GetResponse(); 

            Stream respStream = wr.GetResponseStream();
            StreamReader reader = new System.IO.StreamReader(respStream);
            string ret = reader.ReadToEnd();
            MatchCollection mc = Regex.Matches(ret, @"(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9]):\d{1,4}");
             
            foreach (Match m in mc)
            {
                IP.Add(m.Value);
            }
            if (MutiThread.Text == "")
                MutiThread.Text = "100";
            
            timer = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为1000毫秒；
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timerout);//到达时间的时候执行事件；
            timer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            timer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            
            MultiThreadCount = 0;
            timercount = 0;
            int tmpsize=Convert.ToInt16(MutiThread.Text);
            for (int i = 0; i < tmpsize; i++)
            {
                Thread t= new Thread(new ParameterizedThreadStart(IPCheckThread));
                t.IsBackground = true;
                t.Start(i.ToString());
            }  
        }
        public void Timerout(object source, System.Timers.ElapsedEventArgs e)
        {
            timercount++;
            Console.WriteLine(timercount.ToString() + "       " + MultiThreadCount.ToString()); 
            if (MultiThreadCount+2 >= Convert.ToInt16(MutiThread.Text))
            {
                timer.Stop();
                IP_ok.Sort();
                IP_done = IP_ok.Distinct().ToList();
                GoodIPBox.Text = "";
                foreach (string m in IP_done)
                {
                    this.Invoke(new Action(() =>
                    {
                        GoodIPBox.AppendText(m + "\n");
                    }));
                }
                this.Invoke(new Action(() =>
                {
                    GoodIPBox.AppendText("\n ***************  " + IP_done.Count.ToString() + "  FINISHED  **************");
                }));
            }
            //写入文本
            StreamWriter sw = new StreamWriter("./IP.txt");
            string w = "";
            sw.Write(w);
            sw.Close();
            sw = File.AppendText("./IP.txt");
            foreach (string ip in IP)
                sw.Write(ip+"\n");
            sw.Close(); 

            this.Invoke(new Action(() =>
            {
                statebox.AppendText("用时" + timercount.ToString() + "s\n");
            })); 

        }
        #endregion


        #region 传客美文      
        
        int vvv=0;
        private void chuankeButton_Click(object sender, EventArgs e)
        {
            foreach (string ip in IP_done)
            {
                Random ran = new Random();
                vvv = ran.Next(10, 99);
                Thread.Sleep(vvv % 10 * 1302);
                string address = "http://www.weo9.top/user/iplist/rd";
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
                string[] sArr = ip.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                WebProxy proxyObject = new WebProxy(sArr[0], Convert.ToInt32(sArr[1]));// port为端口号 整数型  
                req.Proxy = proxyObject; //设置代理 
                req.Timeout = 5000;   //超时
                req.Headers.Add("Accept-Encoding", "gzip, deflate");
                req.Headers.Add("Accept-Language", "zh-CN,en-US;q=0.8");
                req.Headers.Add("Origin", "http://www.weo11.top");
                req.Headers.Add("X-Requested-With", "XMLHttpRequest");
                req.Method = "POST";
                req.Host = "www.weo9.top";
                req.Accept = @"*/*";
                string u1 = "Mozilla/5.4 (Linux; Android 4." + vvv.ToString() + ".2; Huawei G7" + (vvv + 5).ToString() + "40-T01 Build/KO5T439H) AppleWebK5it/537.36 ";
                string u2 = "(KHTML, like Gecko) Version/e4.0 Chrome/37." + (vvv + 5).ToString() + ".0.0 Mobile MQQBreowser/6.8 TBS/036824e Safari/537.36 ";
                string u3 = "V1_AND_SQ_6." + (vvv + 5).ToString() + ".5e_410_YYB_D QQ/6.5.5.2880 NetType/WIFI WebP/0.3.0 Pixel/720";
                req.UserAgent = @u1 + u2 + u3;
                req.Referer = "http://www.weo100.top/show/detail/1759171/615722?r=www.greenlinkin.cn";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                req.ServicePoint.Expect100Continue = false;


                string host = "http://www.weo9.top";
                CookieContainer cc = new CookieContainer();
                cc.Add(new Uri(host), new Cookie("JSESSIONID", "9DC3032DA13835FD6B98509C" + (vvv % 100).ToString() + "4F0923"));
                cc.Add(new Uri(host), new Cookie("SERVERID", "9fff4c02c4e28b3e0a4e85cc" + (vvv % 100).ToString() + "a9dbfd|1476545" + (vvv % 100).ToString() + "4|1476545" + (vvv % 100).ToString() + "4"));
                cc.Add(new Uri(host), new Cookie("CNZZDATA1260371107", "9994" + (vvv % 100).ToString() + "966-147" + (vvv % 100).ToString() + "55945-http%253A%252F%252Fwww.acolaw.com.cn%252F%7C147" + (vvv % 100).ToString() + "55945"));
                req.CookieContainer = cc;

                string postdata = "id=1759171&uid=615722&w=412&h=732&u=http%3A%2F%2Fwww.weo100.top%2Fshow%2Fdetail%2F1759171%2F615722%3Fr%3Dwww.greenlinkin.cn&r=www.greenlinkin.cn";
                byte[] byteArray = Encoding.UTF8.GetBytes(postdata);
                req.ContentLength = byteArray.Length;
                try
                {
                    Stream newStream = req.GetRequestStream();
                    // Send the data. 
                    newStream.Write(byteArray, 0, byteArray.Length); //写入参数 
                    //newStream.Close(); 
                    WebResponse wr;
               
                    wr = req.GetResponse();
                    Stream respStream = wr.GetResponseStream();
                    StreamReader reader = new System.IO.StreamReader(respStream);
                    string ret = reader.ReadToEnd();
                    statebox.AppendText(ip + "   success\n");
                }
                catch {
                    statebox.AppendText(ip + "   failed\n");
                    continue;
                }               
            }
        }

        private void loadIP_Click(object sender, EventArgs e)
        {
            string[] alllines = File.ReadAllLines(("./IP.txt"),Encoding.Default);           
            IP_done.Clear();
            for(int i=0;i<alllines.Length;i++)
            {
                IP_done.Add(alllines[i]);
            }
            statebox.AppendText("**导入代理ip "+alllines.Length.ToString()+ "条**\n");
        }
        #endregion
       
        #region 搜狗阅读
        private string getString(int count)
        {
            int number;
            string checkCode = String.Empty;
            System.Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                number = random.Next();
                number = number % 36;
                if (number < 10)
                    number += 48;    //数字0-9编码在48-57 
                else
                    number += 55;    //字母A-Z编码在65-90
                checkCode += ((char)number).ToString();
            }
            return checkCode;
        }
        
        private string SG_getCookies()
        {
            string address = "http://yuedu.sogou.com/user/reg";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address); 

            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            req.Headers.Add("Upgrade-Insecure-Requests", "1"); 
            req.Host = "yuedu.sogou.com";
            req.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0"; 

            req.Method = "GET";
             
            HttpWebResponse wr = (HttpWebResponse)req.GetResponse();
             
            string cookieHeader = "";
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.SetCookies(new Uri(address), cookieHeader);
            HttpWebResponse myresponse = (HttpWebResponse)req.GetResponse();
            string cookies = myresponse.Headers["Set-Cookie"];//获取验证码页面的Cookies

            string cookie1 = cookies.Substring(0, cookies.IndexOf(";") + 1);
            int index1 = cookies.IndexOf("JSESSIONID");
            int index2 = cookies.IndexOf(";", index1);
            string cookie2 = cookies.Substring(index1, index2 - index1 + 1);
            index1 = cookies.IndexOf("SUV");
            index2 = cookies.IndexOf(";", index1);
            string cookie3 = cookies.Substring(index1, index2 - index1 + 1);
            return cookie1 + cookie2 + "nickname=c56d4ebfcb66d5c3d89c90e4ca9d9b36cXcxMjMwMDk4;" + cookie3;
        }
        private bool ImageCodeCheck(string Code)
        {
            string address = "http://yuedu.sogou.com/ajax/reg/captchacheck?captcha=" + Code;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);

            req.Host = "yuedu.sogou.com";
            req.Accept = @"application/json, text/javascript, */*";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0";
            req.Method = "GET";
            req.Referer = "http://yuedu.sogou.com/user/reg";

            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            req.Headers.Add("Cookie", cookies);

            HttpWebResponse wr = (HttpWebResponse)req.GetResponse();

            Stream respStream = wr.GetResponseStream();
            if (wr.ContentEncoding.ToLower().Contains("gzip"))
            {
                respStream = new GZipStream(respStream, CompressionMode.Decompress);
            }
            StreamReader reader = new StreamReader(respStream);

            return reader.ReadToEnd().Contains("succ");
        } 
        private string SG_getImgCheckCode()
        {     
            string address = "http://yuedu.sogou.com/reg/captcha?t=1477724405374";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address); 
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Accept = @"*/*";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0";
            req.Referer = "http://yuedu.sogou.com/user/reg";
            req.Host ="yuedu.sogou.com";
            req.Method = "GET";

            req.Headers.Add("Cookie", cookies);

            HttpWebResponse wr = (HttpWebResponse)req.GetResponse();

            Stream stream = wr.GetResponseStream();//得到验证码数据流
            Bitmap sourcebm = new Bitmap(stream);//初始化Bitmap图片
            //pictureBox1.Image = sourcebm;
            MemoryStream ms = new MemoryStream();
            sourcebm.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();  //byte[]   bytes=   ms.ToArray(); 
                                            //MemoryStream的GetBuffer并不是得到这个流所存储的内容，而是返回这个流的基础字节数组，
                                            //可能包括在扩充的时候一些没有使用到的字节。
            ms.Close();  
             
            string Code = RuoKuaiGetImgCode(bytes);
            if (Code == "")
                SG_getImgCheckCode();
            if(!ImageCodeCheck(Code))
                SG_getImgCheckCode();
            return Code;
        }
        private bool SG_CheckUsrName(string name,string cookies)
        { 
            string address = "http://yuedu.sogou.com/ajax/passport/checkmail?mail=" + name + "%40sogou.com";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);

            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            req.Headers.Add("Cookie", cookies);

            req.Host = "yuedu.sogou.com";
            req.Accept = @"application/json, text/javascript, */*";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0";
            req.Method = "GET";
            req.Referer = "http://yuedu.sogou.com/user/reg";

            HttpWebResponse wr = (HttpWebResponse)req.GetResponse();

            Stream respStream = wr.GetResponseStream();
            if (wr.ContentEncoding.ToLower().Contains("gzip"))
            {
                respStream = new GZipStream(respStream, CompressionMode.Decompress);
            }
            StreamReader reader = new StreamReader(respStream);
            string ret = reader.ReadToEnd();
            return !ret.Contains("fail"); 
        } 
        string ssggName;
        string ssggPasswd;
        string cookies;
        private void SGstart_Click(object sender, EventArgs e)
        {
            if (MaxNum.Text == "")
                MaxNum.Text = "10";
            if (Delaytime.Text == "")
                Delaytime.Text = "10";
            int Num = Convert.ToInt16(MaxNum.Text);
            int time = Convert.ToInt16(Delaytime.Text);
            List<string> Result = new List<string>();
            Thread t = new Thread(new ThreadStart(delegate
            {
                for (int i = 0; i < Num; i++)
                {
                    string tmp = SG_main();
                    if (tmp != "")
                    {
                        SGstatus.BeginInvoke(new EventHandler(delegate
                        {
                            SGstatus.AppendText(tmp);
                        }));
                        Result.Add(tmp);
                    }
                    else
                        i--;
                    Thread.Sleep(time * 1000);
                }
                WriteListToTextFile(Result, "./save.txt");
                SGstatus.BeginInvoke(new EventHandler(delegate
                {
                    SGstatus.AppendText("任务完成\n");
                }));
            }));
            t.IsBackground = true;
            t.Start();
        }
        private string SG_main()
        {
            cookies= SG_getCookies();
            string Code = SG_getImgCheckCode(); 
            ssggName = RKname.Text + getString(6);
            while(SG_CheckUsrName(ssggName, cookies)!=true)
                ssggName = RKname.Text + getString(6);         
            
            if (RKpasswd.Text == "")
            {
                ssggPasswd = getString(8);
            } 
            string address = "http://yuedu.sogou.com/ajax/passport/register?type=1&mail=" + ssggName + "@sogou.com&captcha=" + Code;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
              
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");
            req.Headers.Add("password", ssggPasswd);
            req.Headers.Add("Cookie", cookies); 

            req.Host = "yuedu.sogou.com";
            req.Accept = @"application/json, text/javascript, */*";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0";
            req.Method = "GET";
            req.Referer = "http://yuedu.sogou.com/user/reg"; 

            HttpWebResponse wr = (HttpWebResponse)req.GetResponse();

            Stream respStream = wr.GetResponseStream();
            if (wr.ContentEncoding.ToLower().Contains("gzip"))
            {
                respStream = new GZipStream(respStream, CompressionMode.Decompress);
            }
            StreamReader reader = new StreamReader(respStream); 
            string  ret = reader.ReadToEnd();
            if (ret.Contains("succ"))
            {
                return ssggName + "@sogou.com----" + ssggPasswd + "\n";
            }
            return "";
        }

        #endregion

        #region 上传答题
        private string RuoKuaiGetImgCode(byte[] data)
        {             
            //必要的参数
            var param = new Dictionary<object, object>
            {                
                {"username",RKname.Text},
                {"password",RKpasswd.Text},
                {"typeid","3050"},
                {"timeout","90"},
                {"softid","69796"},
                {"softkey","534064de78924f36b5bb8a95c297dcb6"}
            };
            string httpResult = RuoKuai.RuoKuaiHttp.Post("http://api.ruokuai.com/create.xml", param, data);
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(httpResult);
            }
            catch
            {
                return "";
                //SGstatus.AppendText("返回格式有误\r\n"); 
            }
            XmlNode idNode = xmlDoc.SelectSingleNode("Root/Id");
            XmlNode resultNode = xmlDoc.SelectSingleNode("Root/Result");
            XmlNode errorNode = xmlDoc.SelectSingleNode("Root/Error");
            string result = string.Empty;
            string topidid = string.Empty;
            if (resultNode != null && idNode != null)
            { 
                result = resultNode.InnerText;  
                //SGstatus.AppendText("识别结果：" + result + "\r\n");
                return result;
            }
            //else if (errorNode != null)
            //    SGstatus.AppendText("识别错误：" + errorNode.InnerText + "\r\n");
            //else
            //    SGstatus.AppendText("未知问题\r\n");
            return "";
        }
        
        //帐号信息查询
         
         private void RKSignIn_Click(object sender, EventArgs e)
        { 
            if (RKname.Text == "" || RKpasswd.Text == "")
            {
                SGstatus.AppendText("请输入若快账号与密码\n");
                return;
            }
            var param = new Dictionary<object, object>
            {        
                {"username",RKname.Text},
                {"password",RKpasswd.Text}
            };


            Thread t = new Thread(new ThreadStart(delegate
            {
                //提交服务器
                string httpResult = RuoKuai.RuoKuaiHttp.Post("http://api.ruokuai.com/info.xml", param);
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(httpResult);
                }
                catch
                {
                    SGstatus.BeginInvoke(new EventHandler(delegate
                    {
                        SGstatus.AppendText("返回格式有误\r\n");
                        SGstatus.Select(SGstatus.TextLength, SGstatus.TextLength);
                        SGstatus.ScrollToCaret();
                    }));
                }


                XmlNode scoreNode = xmlDoc.SelectSingleNode("Root/Score");
                XmlNode historyScoreNode = xmlDoc.SelectSingleNode("Root/HistoryScore");
                XmlNode totalTopicNode = xmlDoc.SelectSingleNode("Root/TotalTopic");

                XmlNode errorNode = xmlDoc.SelectSingleNode("Root/Error");

                if (scoreNode != null && historyScoreNode != null && totalTopicNode != null)
                {
                    SGstatus.BeginInvoke(new EventHandler(delegate
                    {
                        SGstatus.AppendText("剩余快豆：" + scoreNode.InnerText + "\r\n");
                        SGstatus.AppendText("历史快豆：" + historyScoreNode.InnerText + "\r\n");
                        SGstatus.AppendText("答题总数：" + totalTopicNode.InnerText + "\r\n");
                        SGstatus.Select(SGstatus.TextLength, SGstatus.TextLength);
                        SGstatus.ScrollToCaret();
                    }));
                }
                else if (errorNode != null)
                {
                    SGstatus.BeginInvoke(new EventHandler(delegate
                    {
                        SGstatus.AppendText("错误：" + errorNode.InnerText + "\r\n");
                        SGstatus.Select(SGstatus.TextLength, SGstatus.TextLength);
                        SGstatus.ScrollToCaret();
                    }));
                }
                else
                {
                    SGstatus.BeginInvoke(new EventHandler(delegate
                    {
                        SGstatus.AppendText("未知问题\r\n");
                        SGstatus.Select(SGstatus.TextLength, SGstatus.TextLength);
                        SGstatus.ScrollToCaret();
                    }));
                }
            }));
            t.IsBackground = true;
            t.Start();
        } 
        #endregion

        #region List写入TXT文件
        private void WriteListToTextFile(List<string> list, string txtFile)
        { 
            StreamWriter sw = new StreamWriter(txtFile,true);
            sw.Flush();
            for (int i = 0; i < list.Count; i++) sw.WriteLine(list[i]); 
            sw.Flush();
            sw.Close(); 
        }

        #endregion

       
    }
}