using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.Model
{
    /// <summary>
    /// OA返回结果
    /// </summary>
    public class ResultList
    {
        /// <summary>
        /// 外部系统每个流程的主键ID，若创建多个流程时可通过此字段定位每个流程是否成功以及成功的requestid
        /// </summary>
        public string fpkid { get; set; }
        /// <summary>
        /// 返回结果
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 成功触发了OA审批流后会返回一个唯一ID，外部系统若有条件可将此ID存储起来，因为可以根据此字段来定位对应到OA具体哪个单据。外部系统若不方便操作则可忽略此字段
        /// </summary>
        public string requestid { get; set; }
        /// <summary>
        /// 0表示成功   1表示失败
        /// </summary>
        public string status { get; set; }
    }
}
