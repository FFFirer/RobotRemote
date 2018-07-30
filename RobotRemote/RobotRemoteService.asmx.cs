using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Threading.Tasks;

namespace RobotRemote
{
    /// <summary>
    /// RobotRemoteService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class RobotRemoteService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        //导购、查券
        [WebMethod]
        public string GetQuan(string Message)
        {
            string result = HandleCenter.Handle4(Message);
            return result;
        }

        //批量获取好券
        public List<String> PostQuans(string Q, long page_no)
        {
            List<string> Results = HandleCenter.PostQuans(q, page_no);
            return Results;
        }

    }
}
