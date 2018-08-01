using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using Top;
using Top.Api;
using Top.Tmc;
using Top.Api.Request;
using Top.Api.Response;
using Top.Api.Domain;

namespace RobotRemote
{
    public class HandleCenter
    {
        //查券
        //链接转换
        public static string Handle4(string message)
        {
            string serverUrl = "http://gw.api.taobao.com/router/rest";
            string appkey = "24984684";
            string secret = "b52c6c517d488ca9ccb95411daede241";
            long adzonrid = 11262850424L;
            ITopClient client = new DefaultTopClient(serverUrl, appkey, secret);

            //分析语句中的操作关键词
            //提取关键词，返回操作结果
            //1、有没有，还有，我要，我想要，我要买，我想买，优惠
            string keyword = string.Empty;
            string CallbackMessage;
            keyword = GetKeyword(message);
            if(keyword == "NoKey")
            {
                //抛出异常
                CallbackMessage = "warning:无法识别所要识别的商品，" + message;
            }
            else
            {
                //调用好券清单API
                TbkDgItemCouponGetRequest req = new TbkDgItemCouponGetRequest();
                req.AdzoneId = adzonrid;
                req.Platform = 1L;
                //req.Cat = "16,18";    
                req.PageSize = 1L;
                req.Q = keyword;
                req.PageNo = 1L;
                TbkDgItemCouponGetResponse rsp = client.Execute(req);

                if(rsp.Results.Count > 0)
                {
                    var item = rsp.Results.FirstOrDefault();  
                    //调用淘口令生成API生成淘口令
                    TbkTpwdCreateRequest treq = new TbkTpwdCreateRequest();
                    treq.Text = item.Title;
                    treq.Url = item.CouponClickUrl;
                    treq.Logo = item.SmallImages.FirstOrDefault();
                    TbkTpwdCreateResponse trsp = client.Execute(treq);

                    //调用接口生成商品详情信息
                    TbkItemInfoGetRequest ireq = new TbkItemInfoGetRequest();
                    ireq.NumIids = item.NumIid.ToString();
                    ireq.Platform = 1;
                    TbkItemInfoGetResponse irsp = client.Execute(ireq);
                    
                    if(trsp.Data != null && irsp.Results.Count > 0)
                    {
                        //CallbackMessage = string.Format("【{0}】\n{1}\n在浏览器里打开链接或者复制这段描述{2}在淘宝里打开", item.Title, item.CouponClickUrl, trsp.Data.Model);
                        string searchUrl = "http://52lequan.cn/index.php?r=l&kw=" + System.Web.HttpUtility.UrlEncode(keyword, System.Text.Encoding.UTF8);
                        CallbackMessage = string.Format("【{0}】\n" +
                            "[CQ:image,file={6}]\n" +
                            "现价：{1}\n" +
                            "券后价：{2}\n" +
                            //"【下单链接】\n" +
                            "——————————\n" +
                            "复制这条信息，{4},打开【手机淘宝】即可查看\n" +
                            "更多优惠请点击{5}", item.Title, item.ZkFinalPrice, ZHPrice(item.ZkFinalPrice, item.CouponInfo), item.CouponClickUrl, trsp.Data.Model, searchUrl, item.PictUrl);
                    }
                    else
                    {
                        CallbackMessage = "error:获取淘口令出错，" + trsp.SubErrMsg;
                    }

                }
                else
                {
                    CallbackMessage = "error:获取优惠信息出错，" + rsp.SubErrMsg;
                }


                //CallbackMessage = string.Format("{0}\n券后价：{1}\n在售价：{2}\n【下单链接】\n{3}\n——————————\n复制这条信息，{4},打开【手机淘宝】即可查看\n更多优惠请点击{5}[CQ:image,file={6}\n测试:{7}", item.Title, item.ZkFinalPrice, item.ReservePrice, item.ItemUrl, tbkTpwdCreateResponse.Data.Model, searchurl, item.PictUrl, item.ClickUrl);

            }
            return CallbackMessage;

            //2、链接转换-暂时不实现

        }

        public static List<string> PostQuans(string q, long PageNo)
        {
            List<string> ReturnInfos = new List<string>();
            string serverUrl = "http://gw.api.taobao.com/router/rest";
            string appkey = "24984684";
            string secret = "b52c6c517d488ca9ccb95411daede241";
            long adzonrid = 11262850424L;
            ITopClient client = new DefaultTopClient(serverUrl, appkey, secret);
            //调用好券清单API，返回一个列表
            TbkDgItemCouponGetRequest GetReq = new TbkDgItemCouponGetRequest();
            GetReq.AdzoneId = adzonrid;
            GetReq.Platform = 1;
            GetReq.PageSize = 15L;
            GetReq.Q = q;
            GetReq.PageNo = PageNo;
            TbkDgItemCouponGetResponse GetRsp = client.Execute(GetReq);     //获取一个结果列表
            foreach(var item in GetRsp.Results)
            {
                //获取每个宝贝的现价、优惠价
                TbkItemInfoGetRequest InfoReq = new TbkItemInfoGetRequest();
                InfoReq.NumIids = item.NumIid.ToString();
                InfoReq.Platform = 1;
                TbkItemInfoGetResponse InfoRsp = client.Execute(InfoReq);

                //生成淘口令
                TbkTpwdCreateRequest TpwdReq = new TbkTpwdCreateRequest();
                TpwdReq.Text = item.Title;
                TpwdReq.Url = item.CouponClickUrl;
                TpwdReq.Logo = item.PictUrl;
                TbkTpwdCreateResponse TpwdRsp = client.Execute(TpwdReq);

                //淘口令短链接
                TbkSpreadGetRequest SpreadReq = new TbkSpreadGetRequest();
                List<TbkSpreadGetRequest.TbkSpreadRequestDomain> urls = new List<TbkSpreadGetRequest.TbkSpreadRequestDomain>();
                TbkSpreadGetRequest.TbkSpreadRequestDomain native_url = new TbkSpreadGetRequest.TbkSpreadRequestDomain();
                urls.Add(native_url);
                native_url.Url = item.CouponClickUrl;
                SpreadReq.Requests_ = urls;
                TbkSpreadGetResponse SpreadRsp = client.Execute(SpreadReq);

                if(SpreadRsp.Results.FirstOrDefault().ErrMsg == "OK")
                {
                    //拼接字符串：商品标题，图片，现价，折后价，下单链接，淘口令
                    string Detail_Info = string.Empty;
                    Detail_Info += string.Format("【{0}】\n", item.Title);
                    Detail_Info += string.Format("[CQ:emoji,id=128073]{0}\n", item.ItemDescription); 
                    Detail_Info += string.Format("[CQ:image,file={0}]\n", item.PictUrl);
                    Detail_Info += string.Format("现价：￥{0}\n", item.ZkFinalPrice);
                    Detail_Info += string.Format("券后价：￥{0}\n", ZHPrice(item.ZkFinalPrice, item.CouponInfo));
                    Detail_Info += string.Format("【领券下单链接】{0}", SpreadRsp.Results.FirstOrDefault().Content);
                    Detail_Info += string.Format("点击链接，再选择浏览器打开，或者复制这段描述{0}后到淘宝", TpwdRsp.Data.Model);
                    ReturnInfos.Add(Detail_Info);
                }
                else
                {
                    continue;
                }
            }

            return ReturnInfos;
        }

        public static string ZHPrice(string price, string couponinfo)
        {
            string man = @"满(.*?)元";
            string jian = @"减(.*?)元";
            Match Man = Regex.Match(couponinfo, man);
            Match Jian = Regex.Match(couponinfo, jian);
            GroupCollection Mgroups = Man.Groups;
            GroupCollection Jgroups = Jian.Groups;
            if(Convert.ToDouble(price) >= Convert.ToDouble(Mgroups[1].Value))
            {
                return (Convert.ToDouble(price) - Convert.ToDouble(Jgroups[1].Value)).ToString("f2");
            }
            else
            {
                return price;
            }
        }

        public static string GetKeyword(string RawString)
        {
            string KeyPattern = @"([我有].*?[有要买个])([\w]+)[\W]?";
            if (Regex.IsMatch(RawString, KeyPattern))
            {
                Match match = Regex.Match(RawString, KeyPattern);
                string keyword = match.Groups[2].Value;
                return keyword;
            }
            else
            {
                return "NoKey";
            }
        }
    }
}