using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pvirtech.Framework.Common
{
    public partial class LogHelper
    {

        public static readonly ILog loginfo = LogManager.GetLogger("loginfo");

        public static readonly ILog logerror = LogManager.GetLogger("logerror");

        public static void WriteLog(string info)
        {
            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(info);
            }
        }
        /// <summary>
        /// 错误记录
        /// </summary>
        /// <param name="info">附加信息</param>
        /// <param name="ex">错误</param>
        public static void ErrorLog(Exception ex, string info = null)
        {
            if (!string.IsNullOrEmpty(info) && ex == null)
            {
                logerror.ErrorFormat("【附加信息】 : {0}<br>", new object[] { info });
            }
            else if (!string.IsNullOrEmpty(info) && ex != null)
            {
                string errorMsg = BeautyErrorMsg(ex);
                logerror.ErrorFormat("【附加信息】 : {0}<br>{1}", new object[] { info, errorMsg });
            }
            else if (string.IsNullOrEmpty(info) && ex != null)
            {
                string errorMsg = BeautyErrorMsg(ex);
                logerror.Error(errorMsg);
            }
        }
        /// <summary>
        /// 美化错误信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <returns>错误信息</returns>
        private static string BeautyErrorMsg(Exception ex)
        {
            string errorMsg = string.Format("【异常类型】：{0} <br>【异常信息】：{1} <br>【堆栈调用】：{2} <br/> <br/>【错误日志】:{3}：", new object[] { ex.GetType().Name, ex.Message, ex.StackTrace, ex.TargetSite + "<br><br/>" + ex.ToString() });
            errorMsg = errorMsg.Replace("\r\n", "<br>");
            errorMsg = errorMsg.Replace("位置", "<strong style=\"color:red\">位置</strong>");
            return errorMsg;
        }
    }
}
