using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFFC.Frame.Net.Base.Common;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Data.Base;
using System.Security.Cryptography;
using EFFC.Frame.Net.Global;
using Builder.Web.Helper;
using System.IO;
using System.Net;
using System.Globalization;

namespace Builder.Web.Logic
{
    public abstract partial class GoLogic
    {
        AlyPayHelper _alypay = null;
        /// <summary>
        /// 支付宝
        /// </summary>
        public AlyPayHelper AlyPay
        {
            get
            {
                if (_alypay == null) _alypay = new AlyPayHelper(this);
                return _alypay;
            }


        }
        /// <summary>
        /// 支付宝集成API
        /// </summary>
        public class AlyPayHelper
        {
            GoLogic _logic;
            public AlyPayHelper(GoLogic logic)
            {
                _logic = logic;
            }
            /// <summary>
            /// 开放平台的使用者ID
            /// </summary>
            public string DefaultPID
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["alipay_pid"]);
                }
            }
            /// <summary>
            /// 默认的应用ID号
            /// </summary>
            public string DefaultAPPID
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["alipay_appid"]);
                }
            }
            /// <summary>
            /// 默认RSA的私有密钥
            /// </summary>
            public string DefaultPrivateKey
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["alipay_private_key"]);
                }
            }
            /// <summary>
            /// 默认RSA的私有密钥 for 合作伙伴
            /// </summary>
            public string DefaultPID_PrivateKey
            {
                get
                {
                    return ComFunc.nvl(_logic.Configs["alipay_pid_private_key"]);
                }
            }

            #region wap交易接口https://mapi.alipay.com/gateway.do
            /// <summary>
            /// Wap交易退款
            /// </summary>
            /// <param name="trade_no">交易的订单号</param>
            /// <param name="amount">退款金额</param>
            /// <param name="ali_trade_no">支付宝交易号，从交易成功后返回的数据中获得</param>
            /// <param name="reason">退款理由</param>
            /// <param name="notify_url">后台通知url</param>
            /// <returns></returns>
            public dynamic WapRefund(string trade_no,double amount,string ali_trade_no, string reason, string notify_url)
            {
                var batch_no = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                var detail_date = ali_trade_no + "^" + ComFunc.nvl(amount) + "^" + reason;
                return GenWapRefund(trade_no, batch_no, 1, detail_date, notify_url);
            }

            /// <summary>
            /// Wap交易执行退款
            /// </summary>
            /// <param name="trade_no">交易的订单号</param>
            /// <param name="batch_no">批次编号：每进行一次即时到账批量退款，都需要提供一个批次号，通过该批次号可以查询这一批次的退款交易记录，对于每一个合作伙伴，传递的每一个批次号都必须保证唯一性。
            ///格式为：退款日期（8位）+流水号（3～24位）。
            ///不可重复，且退款日期必须是当天日期。流水号可以接受数字或英文字符，建议使用数字，但不可接受“000”。</param>
            /// <param name="batch_num">退款总笔数，即参数detail_data的值中，“#”字符出现的数量加1，最大支持1000笔（即“#”字符出现的最大数量为999个）。</param>
            /// <param name="detail_data">单笔数据集格式为：第一笔交易退款数据集#第二笔交易退款数据集#第三笔交易退款数据集…#第N笔交易退款数据集；
            ///交易退款数据集的格式为：原付款支付宝交易号^退款总金额^退款理由；如2011011201037066^5.00^协商退款
            ///不支持退分润功能。</param>
            /// <returns></returns>
            public dynamic GenWapRefund(string trade_no, string batch_no, int batch_num, string detail_data, string notify_url)
            {
                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;
                bizcontent.seller_user_id = DefaultPID;
                bizcontent.refund_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                bizcontent.batch_no = batch_no;
                bizcontent.batch_num = ComFunc.nvl(batch_num);
                bizcontent.detail_data = detail_data;

                return GenWapParameter(DefaultPID, "refund_fastpay_by_platform_pwd", notify_url, "", bizcontent);

            }
            /// <summary>
            /// 获取wap支付的参数集
            /// </summary>
            /// <param name="trade_no">订单号</param>
            /// <param name="amount">交易金额</param>
            /// <param name="title">商品名称</param>
            /// <param name="desc">商品描述</param>
            /// <param name="show_url">商品展示页面</param>
            /// <param name="notify_url">交易结果异步通知url</param>
            /// <param name="return_url">交易完成的跳转页面</param>
            /// <param name="timeexpress">交易超时设置，取值范围：1m～15d。
            ///m-分钟，h-小时，d-天，1c-当天（1c-当天的情况下，无论交易何时创建，都在0点关闭）。
            ///该参数数值不接受小数点，如1.5h，可转换为90m。
            ///当用户输入支付密码、点击确认付款后（即创建支付宝交易后）开始计时。
            ///支持绝对超时时间，格式为yyyy-MM-dd HH:mm。</param>
            /// <returns></returns>
            public dynamic WapPay(string trade_no, double amount, string title, string desc, string show_url,string notify_url,string return_url,string timeexpress)
            {
                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;
                if (!string.IsNullOrEmpty(title))
                {
                    bizcontent.subject = title;
                }
                if (!string.IsNullOrEmpty(desc))
                {
                    bizcontent.body = desc;
                }
                bizcontent.total_fee = ComFunc.nvl(amount);
                bizcontent.seller_id = DefaultPID;
                bizcontent.payment_type = "1";
                if (!string.IsNullOrEmpty(show_url))
                {
                    bizcontent.show_url = show_url;
                }
                if (!string.IsNullOrEmpty(timeexpress))
                {
                    bizcontent.it_b_pay = timeexpress;
                }

                return GenWapParameter(DefaultPID, "alipay.wap.create.direct.pay.by.user", notify_url, return_url, bizcontent);
            }
            /// <summary>
            /// 生成wap请求的参数集
            /// </summary>
            /// <param name="pid">合作伙伴编号</param>
            /// <param name="service">呼叫地方法</param>
            /// <param name="notify_url">异步通知url</param>
            /// <param name="return_url">交易完成后的显示url</param>
            /// <param name="bizcontent">业务参数</param>
            /// <returns></returns>
            public dynamic GenWapParameter(string pid, string service, string notify_url,string return_url, FrameDLRObject bizcontent)
            {
                var rtn = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                rtn.aliurl = "https://mapi.alipay.com/gateway.do?_input_charset=utf-8";
                var p = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);

                p.service = service;
                p.partner = pid;
                p._input_charset = "utf-8";
                if (!string.IsNullOrEmpty(notify_url))
                {
                    p.notify_url = notify_url;
                }
                if (!string.IsNullOrEmpty(return_url))
                {
                    p.return_url = return_url;
                }

                FrameDLRObject fp = (FrameDLRObject)p;
                foreach (var key in bizcontent.Keys)
                {
                    fp.SetValue(key, bizcontent.GetValue(key));
                }

                p.sign = RSASign(GetSignContent(fp), DefaultPID_PrivateKey, "utf-8", false, "RSA");
                p.sign_type = "RSA";
                rtn.parameters = p;

                return rtn;
            }
            #endregion

            #region F2F交易接口，https://openapi.alipay.com/gateway.do
            /// <summary>
            /// 订单查询
            /// 该接口提供所有支付宝支付订单的查询，商户可以通过该接口主动查询订单状态，完成下一步的业务逻辑。 
            /// 需要调用查询接口的情况： 当商户后台、网络、服务器等出现异常，商户系统最终未接收到支付通知； 调用支付接口后，返回系统错误或未知交易状态情况；
            /// 调用alipay.trade.pay，返回INPROCESS的状态； 调用alipay.trade.cancel之前，需确认支付状态；
            /// 参考文档https://doc.open.alipay.com/doc2/apiDetail.htm?spm=a219a.7395905.0.0.Rxqa0i&docType=4&apiId=757
            /// </summary>
            /// <param name="trade_no">支付时的订单号</param>
            /// <returns></returns>
            public dynamic Query(string trade_no)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;//与trade_no二选一
                bizcontent.trade_no = "";//与out_trade_no二选一

                var result = AliPay(DefaultAPPID, "alipay.trade.query", null, null, bizcontent);
                if (result.statuscode == 200)
                {
                    var code = result.alipay_trade_query_response.code;
                    if (code == "10000")
                    {
                        rtn.issuccess = true;
                        var qrurl = result.alipay_trade_query_response.qr_code;
                        var s = ComFunc.QRCode(qrurl);
                        rtn.qrcode = s;
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = result.alipay_trade_query_response.msg;
                    }
                    rtn.response = result.alipay_trade_query_response;
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;
            }
            /// <summary>
            /// 支付交易返回失败或支付系统超时，调用该接口撤销交易。如果此订单用户支付失败，支付宝系统会将此订单关闭；
            /// 如果用户支付成功，支付宝系统会将此订单资金退还给用户。 
            /// 注意：只有发生支付系统超时或者支付结果未知时可调用撤销，其他正常支付的单如需实现相同功能请调用申请退款API。提交支付交易后调用【查询订单API】，没有明确的支付结果再调用【撤销订单API】
            /// </summary>
            /// <param name="trade_no">订单号</param>
            /// <returns></returns>
            public dynamic Cancel(string trade_no)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;//与trade_no二选一
                bizcontent.trade_no = "";//与out_trade_no二选一

                var result = AliPay(DefaultAPPID, "alipay.trade.cancel", null, null, bizcontent);
                if (result.statuscode == 200)
                {
                    var code = result.alipay_trade_cancel_response.code;
                    if (code == "10000")
                    {
                        rtn.issuccess = true;
                        var qrurl = result.alipay_trade_cancel_response.qr_code;
                        var s = ComFunc.QRCode(qrurl);
                        rtn.qrcode = s;
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = result.alipay_trade_cancel_response.msg;
                    }
                    rtn.response = result.alipay_trade_cancel_response;
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;
            }
            /// <summary>
            /// 退款,当交易发生之后一段时间内，由于买家或者卖家的原因需要退款时，卖家可以通过退款接口将支付款退还给买家
            /// 相关接口细则请查看https://doc.open.alipay.com/doc2/apiDetail.htm?spm=a219a.7395905.0.0.c38BL9&docType=4&apiId=759
            /// </summary>
            /// <param name="trade_no">交易时的订单号</param>
            /// <param name="amount">退款金额</param>
            /// <param name="reason">退款理由</param>
            /// <returns></returns>
            public dynamic Refund(string trade_no, double amount,string reason)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;//与trade_no二选一
                bizcontent.trade_no = "";//与out_trade_no二选一
                bizcontent.refund_amount = ComFunc.nvl(amount);
                bizcontent.refund_reason = reason;
                bizcontent.operator_id = "jumi";

                var result = AliPay(DefaultAPPID, "alipay.trade.refund", null, null, bizcontent);
                if (result.statuscode == 200)
                {
                    var code = result.alipay_trade_refund_response.code;
                    if (code == "10000")
                    {
                        rtn.issuccess = true;
                        var qrurl = result.alipay_trade_refund_response.qr_code;
                        var s = ComFunc.QRCode(qrurl);
                        rtn.qrcode = s;
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = result.alipay_trade_refund_response.msg;
                    }
                    rtn.response = result.alipay_trade_refund_response;
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;
            }
            /// <summary>
            /// 用户扫码支付
            /// </summary>
            /// <param name="trade_no">商户订单号</param>
            /// <param name="amount">交易金额</param>
            /// <param name="trade_title">订单标题</param>
            /// <param name="trade_desc">订单描述</param>
            /// <param name="timeout_express">该笔订单允许的最晚付款时间，逾期将关闭交易。默认15m;取值范围：1m～15d。m-分钟，h-小时，d-天，1c-当天（1c-当天的情况下，无论交易何时创建，都在0点关闭）。 该参数数值不接受小数点， 如 1.5h，可转换为 90m</param>
            /// <param name="seller_id">卖家支付宝账号，为空时则支付到商户绑定的支付宝账号上</param>
            /// <param name="goodsdetail">订单包含的商品列表信息,没有则设为null
            /// [{
            ///   "goods_id": 商品的编号,
            ///   "alipay_goods_id": 支付宝定义的统一商品编号,可以为null,
            ///   "goods_name": 商品名称,
            ///   "quantity": 商品数量,
            ///   "price": 商品单价，单位为元,
            ///   "goods_category": 商品类目,可以为null,
            ///   "body": 商品描述信息,可以为null,
            ///   "show_url":商品的展示地址
            /// }]
            /// </param>
            /// <returns></returns>
            public dynamic QRCodePay(string trade_no, double amount, string trade_title, string trade_desc, string timeout_express, string seller_id, object[] goodsdetail, string notify_url)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");
                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;
                if (!string.IsNullOrEmpty(seller_id))
                {
                    bizcontent.seller_id = seller_id;
                }
                else
                {
                    bizcontent.seller_id = DefaultPID;
                }

                bizcontent.total_amount = ComFunc.nvl(amount);
                bizcontent.discountable_amount = ComFunc.nvl(amount);
                bizcontent.undiscountable_amount = "0";
                bizcontent.subject = trade_title;
                bizcontent.body = trade_desc;
                bizcontent.goods_detail = goodsdetail;
                bizcontent.operator_id = "jumi";
                bizcontent.timeout_express = string.IsNullOrEmpty(timeout_express) ? "15m" : timeout_express;

                var result = AliPay(DefaultAPPID, "alipay.trade.precreate", notify_url, null, bizcontent);
                if (result.statuscode == 200)
                {
                    var code = result.alipay_trade_precreate_response.code;
                    if (code == "10000")
                    {
                        rtn.issuccess = true;
                        var qrurl = result.alipay_trade_precreate_response.qr_code;
                        var s = ComFunc.QRCode(qrurl);
                        rtn.qrcode = s;
                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = result.alipay_trade_precreate_response.msg;
                    }
                    rtn.response = result.alipay_trade_precreate_response;
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;
            }
            /// <summary>
            /// 用户付款码支付
            /// </summary>
            /// <param name="trade_no">商户订单号</param>
            /// <param name="auth_code">买家的授权码（来自用户付款时动态产生的条形码编号）</param>
            /// <param name="amount">交易金额</param>
            /// <param name="trade_title">订单标题</param>
            /// <param name="trade_desc">订单描述</param>
            /// <param name="timeout_express">该笔订单允许的最晚付款时间，逾期将关闭交易。默认15m;取值范围：1m～15d。m-分钟，h-小时，d-天，1c-当天（1c-当天的情况下，无论交易何时创建，都在0点关闭）。 该参数数值不接受小数点， 如 1.5h，可转换为 90m</param>
            /// <param name="seller_id">卖家支付宝账号，为空时则支付到商户绑定的支付宝账号上</param>
            /// <param name="goodsdetail">订单包含的商品列表信息,没有则设为null
            /// [{
            ///   "goods_id": 商品的编号,
            ///   "alipay_goods_id": 支付宝定义的统一商品编号,可以为null,
            ///   "goods_name": 商品名称,
            ///   "quantity": 商品数量,
            ///   "price": 商品单价，单位为元,
            ///   "goods_category": 商品类目,可以为null,
            ///   "body": 商品描述信息,可以为null,
            ///   "show_url":商品的展示地址
            /// }]
            /// </param>
            /// <returns></returns>
            public dynamic BarcodePay(string trade_no, string auth_code, double amount, string trade_title, string trade_desc, string timeout_express,string seller_id, object[] goodsdetail,string notify_url)
            {
                var rtn = FrameDLRObject.CreateInstance(@"{
issuccess:true,
msg:''
}");

                var bizcontent = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                bizcontent.out_trade_no = trade_no;
                bizcontent.scene = "bar_code";
                bizcontent.auth_code = auth_code;
                if (!string.IsNullOrEmpty(seller_id))
                {
                    bizcontent.seller_id = seller_id;
                }else{
                    bizcontent.seller_id = DefaultPID;
                }

                bizcontent.total_amount = ComFunc.nvl(amount);
                bizcontent.discountable_amount = ComFunc.nvl(amount);
                bizcontent.undiscountable_amount = "0";
                bizcontent.subject = trade_title;
                bizcontent.body = trade_desc;
                bizcontent.goods_detail = goodsdetail;
                bizcontent.operator_id = "jumi";
                bizcontent.timeout_express = string.IsNullOrEmpty(timeout_express) ? "15m" : timeout_express;

                var result = AliPay(DefaultAPPID, "alipay.trade.pay", notify_url, null, bizcontent);
                if (result.statuscode == 200)
                {
                    var code = result.alipay_trade_pay_response.code;
                    if (code == "10000")
                    {
                        rtn.issuccess = true;

                    }
                    else
                    {
                        rtn.issuccess = false;
                        rtn.msg = result.alipay_trade_pay_response.msg;
                    }
                    rtn.response = result.alipay_trade_pay_response;
                }
                else
                {
                    rtn.issuccess = false;
                    rtn.msg = result.Message;
                    rtn.content = result;
                }

                return rtn;
            }
            /// <summary>
            /// 支付宝支付接口呼叫
            /// </summary>
            /// <param name="appid">支付宝分配给开发者的应用ID</param>
            /// <param name="method">接口名称</param>
            /// <param name="notify_url">回调页面，没有则不填</param>
            /// <param name="app_auth_token">三方应用授权，没有则不填</param>
            /// <param name="bizcontent"></param>
            /// <returns></returns>
            public dynamic AliPay(string appid, string method,string notify_url,string app_auth_token, FrameDLRObject bizcontent)
            {
                var url = "https://openapi.alipay.com/gateway.do?charset=utf-8";

                FrameDLRObject header = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                FrameDLRObject data = FrameDLRObject.CreateInstance(FrameDLRFlags.SensitiveCase);
                data.SetValue("app_id", appid);
                data.SetValue("method", method);
                data.SetValue("format", "json");
                data.SetValue("charset", "utf-8");
                data.SetValue("sign_type", "RSA");
                data.SetValue("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                data.SetValue("version", "1.0");
                if (!string.IsNullOrEmpty(notify_url))
                {
                    data.SetValue("notify_url", notify_url);
                }
                if (!string.IsNullOrEmpty(app_auth_token))
                {
                    data.SetValue("app_auth_token", app_auth_token);
                }
                data.SetValue("biz_content", bizcontent.ToJSONString());
                var keyfromfile = false;
                if (File.Exists(DefaultPrivateKey))
                {
                    keyfromfile = true;
                }
                
                data.SetValue("sign", RSASign(GetSignContent(data), DefaultPrivateKey, "utf-8", keyfromfile, "RSA"));

                dynamic result = _logic.OuterInterface.CallAliService(url, "POST", "application/x-www-form-urlencoded;charset=utf-8", header, data);

                return result;
            }
            #endregion
            private string GetSignContentWithURLEncode(FrameDLRObject parameters)
            {
                // 第一步：把字典按Key的字母顺序排序
                IDictionary<string, string> sortedParams = new SortedDictionary<string, string>();
                foreach (var k in parameters.Keys)
                {
                    sortedParams.Add(k, ComFunc.nvl(parameters.GetValue(k)));
                }
                IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

                // 第二步：把所有参数名和参数值串在一起
                StringBuilder query = new StringBuilder("");
                while (dem.MoveNext())
                {
                    string key = dem.Current.Key;
                    string value = ComFunc.UrlEncode(dem.Current.Value);
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        query.Append(key).Append("=").Append(value).Append("&");
                    }
                }
                string content = query.ToString().Substring(0, query.Length - 1);

                return content;
            }
            /// <summary>
            /// 对参数进行内容加密
            /// </summary>
            /// <param name="parameters"></param>
            /// <returns></returns>
            private string GetSignContent(FrameDLRObject parameters)
            {
                // 第一步：把字典按Key的字母顺序排序
                IDictionary<string, string> sortedParams = new SortedDictionary<string, string>();
                foreach (var k in parameters.Keys)
                {
                    sortedParams.Add(k, ComFunc.nvl(parameters.GetValue(k)));
                }
                IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

                // 第二步：把所有参数名和参数值串在一起
                StringBuilder query = new StringBuilder("");
                while (dem.MoveNext())
                {
                    string key = dem.Current.Key;
                    string value = dem.Current.Value;
                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {
                        query.Append(key).Append("=").Append(value).Append("&");
                    }
                }
                string content = query.ToString().Substring(0, query.Length - 1);

                return content;
            }
            /// <summary>
            /// 进行数字签名
            /// </summary>
            /// <param name="data"></param>
            /// <param name="privateKeyPem">秘钥，如果为路径则keyFromFile=true</param>
            /// <param name="charset"></param>
            /// <param name="keyFromFile"></param>
            /// <param name="signType">RSA或MD5</param>
            /// <returns></returns>
            private string RSASign(string data, string privateKeyPem, string charset, bool keyFromFile, string signType)
            {

                byte[] signatureBytes = null;

                RSACryptoServiceProvider rsaCsp = null;
                if (keyFromFile)
                {//文件读取
                    rsaCsp = LoadCertificateFile(privateKeyPem, signType);
                }
                else
                {
                    //字符串获取
                    rsaCsp = LoadCertificateString(privateKeyPem, signType);
                }

                byte[] dataBytes = null;
                if (string.IsNullOrEmpty(charset))
                {
                    dataBytes = Encoding.UTF8.GetBytes(data);
                }
                else
                {
                    dataBytes = Encoding.GetEncoding(charset).GetBytes(data);
                }

                signatureBytes = rsaCsp.SignData(dataBytes, "SHA1");

                return Convert.ToBase64String(signatureBytes);
            }


            private RSACryptoServiceProvider LoadCertificateFile(string filename, string signType)
            {
                using (System.IO.FileStream fs = System.IO.File.OpenRead(filename))
                {
                    byte[] data = new byte[fs.Length];
                    byte[] res = null;
                    fs.Read(data, 0, data.Length);
                    if (data[0] != 0x30)
                    {
                        res = GetPem("RSA PRIVATE KEY", data);
                    }

                    RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(res, signType);
                    return rsa;
                }
            }
            private RSACryptoServiceProvider LoadCertificateString(string strKey, string signType)
            {
                byte[] data = null;
                //读取带
                //ata = Encoding.Default.GetBytes(strKey);
                data = Convert.FromBase64String(strKey);
                //data = GetPem("RSA PRIVATE KEY", data);
                RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(data, signType);
                return rsa;
            }

            private RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey, string signType)
            {
                byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

                // --------- Set up stream to decode the asn.1 encoded RSA private key ------
                MemoryStream mem = new MemoryStream(privkey);
                BinaryReader binr = new BinaryReader(mem);  //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;
                int elems = 0;
                try
                {
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();    //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102) //version number
                        return null;
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;


                    //------ all private key components are Integer sequences ----
                    elems = GetIntegerSize(binr);
                    MODULUS = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    E = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    D = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    P = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    Q = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DP = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DQ = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    IQ = binr.ReadBytes(elems);


                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    CspParameters CspParameters = new CspParameters();
                    CspParameters.Flags = CspProviderFlags.UseMachineKeyStore;

                    int bitLen = 1024;
                    if ("RSA2".Equals(signType))
                    {
                        bitLen = 2048;
                    }

                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(bitLen, CspParameters);
                    RSAParameters RSAparams = new RSAParameters();
                    RSAparams.Modulus = MODULUS;
                    RSAparams.Exponent = E;
                    RSAparams.D = D;
                    RSAparams.P = P;
                    RSAparams.Q = Q;
                    RSAparams.DP = DP;
                    RSAparams.DQ = DQ;
                    RSAparams.InverseQ = IQ;
                    RSA.ImportParameters(RSAparams);
                    return RSA;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    binr.Close();
                }
            }

            private int GetIntegerSize(BinaryReader binr)
            {
                byte bt = 0;
                byte lowbyte = 0x00;
                byte highbyte = 0x00;
                int count = 0;
                bt = binr.ReadByte();
                if (bt != 0x02)		//expect integer
                    return 0;
                bt = binr.ReadByte();

                if (bt == 0x81)
                    count = binr.ReadByte();	// data size in next byte
                else
                    if (bt == 0x82)
                    {
                        highbyte = binr.ReadByte();	// data size in next 2 bytes
                        lowbyte = binr.ReadByte();
                        byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                        count = BitConverter.ToInt32(modint, 0);
                    }
                    else
                    {
                        count = bt;		// we already have the data size
                    }

                while (binr.ReadByte() == 0x00)
                {	//remove high order zeros in data
                    count -= 1;
                }
                binr.BaseStream.Seek(-1, SeekOrigin.Current);		//last ReadByte wasn't a removed zero, so back up a byte
                return count;
            }

            private byte[] GetPem(string type, byte[] data)
            {
                string pem = Encoding.UTF8.GetString(data);
                string header = String.Format("-----BEGIN {0}-----\\n", type);
                string footer = String.Format("-----END {0}-----", type);
                int start = pem.IndexOf(header) + header.Length;
                int end = pem.IndexOf(footer, start);
                string base64 = pem.Substring(start, (end - start));

                return Convert.FromBase64String(base64);
            }
        }

    }
}

