using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.Model
{
    public class ResultInfo
    {
        /// <summary>
        /// 返回信息 调用接口时前期的校验结果，例如传入的数据不是JSON格式；准入码为空或者不合法等；
        /// </summary>
        public string backmsg { get; set; }
        /// <summary>
        /// 类型 1表示失败，不是1则循环遍历resultlist中的每行status的值
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 返回数组结果，针对所有字段的逻辑校验结果
        /// </summary>
        public List<ResultList> resultlist { get; set; }
    }
}

