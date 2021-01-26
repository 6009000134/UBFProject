using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.Model
{
    /// <summary>
    /// 传给OA的基本信息
    /// </summary>
    public class BaseInfo
    {
        /// <summary>
        /// 即需要创建OA的哪个流程
        /// </summary>
        public string workflowid { get; set; }
        /// <summary>
        /// OA流程创建人信息
        /// </summary>
        public Dictionary<string, object> creator { get; set; }
        /// <summary>
        /// 0：一般  1：重要   2：紧急若未传则默认为0
        /// </summary>
        public int requestlevel { get; set; }
        /// <summary>
        /// 0：触发的OA流程停留在第一个节点
        ///1：触发的OA流程自动提交流转到下一个节点
        ///若未传则默认为1
        /// </summary>
        public int isnextflow { get; set; }
        /// <summary>
        /// 若需要更新流程则此字段必须传入，创建流程此字段没作用，传空即可
        /// </summary>
        public string requestid { get; set; }
        /// <summary>
        /// 若外部系统未传标题给OA，则OA自动生成标题，格式为：流程类型名+创建人+日期，例如采购订单审批流程-张三-2020-03-11
        /// </summary>
        public string requestname { get; set; }
        /// <summary>
        /// 单据主键ID
        /// </summary>
        public string fpkid { get; set; }
    }
}
