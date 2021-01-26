using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.MiscShipmentBE
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
        public ValueTrans creator { get; set; }
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
    /// <summary>
    /// 值和转换规则
    /// </summary>
    public class ValueTrans
    {
        /// <summary>
        /// 值
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 转换规则
        /// </summary>
        public string transrule { get; set; }
    }
    /// <summary>
    /// 值
    /// </summary>
    public class Value
    {
        /// <summary>
        /// 值
        /// </summary>
        public string value { get; set; }
    }
    /// <summary>
    /// 杂发单
    /// </summary>
    public class MishShipDoc
    {
        public BaseInfo @base { get; set; }
        public MishShipHeadDto main { get; set; }
        public List<MishShipLineDto> dt1 { get; set; }
    }
    /// <summary>
    /// 杂发单头
    /// </summary>
    public class MishShipHeadDto
    {
        /// <summary>
        /// 申请人
        /// </summary>
        public ValueTrans sqr { get; set; }

        /// <summary>
        /// 申请人工号
        /// </summary>
        public ValueTrans sqrgh { get; set; }
        /// <summary>
        /// 申请人岗位
        /// </summary>
        public ValueTrans sqrgw { get; set; }
        /// <summary>
        /// 申请人部门
        /// </summary>
        public ValueTrans ssbm { get; set; }
        /// <summary>
        /// 申请人分部
        /// </summary>
        public ValueTrans ssgs { get; set; }
        /// <summary>
        /// 申请日期
        /// </summary>
        public Value sqrq { get; set; }
        /// <summary>
        /// 流程编号
        /// </summary>
        public ValueTrans lcbh { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public Value djlx { get; set; }
        /// <summary>
        /// 单号
        /// </summary>
        public Value dh { get; set; }
        /// <summary>
        /// 存储地点
        /// </summary>
        public Value ccdd { get; set; }
        /// <summary>
        /// 受益存储地点
        /// </summary>
        public Value syccdd { get; set; }
        /// <summary>
        /// 受益人
        /// </summary>
        public Value syr { get; set; }
        /// <summary>
        /// 受益部门
        /// </summary>
        public Value sybm { get; set; }
        /// <summary>
        /// 记账时间
        /// </summary>
        public Value jzsj { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public Value gys { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public Value kh { get; set; }
        /// <summary>
        /// 库管员
        /// </summary>
        public Value kgy { get; set; }
        /// <summary>
        /// 高新项目
        /// </summary>
        public Value gxxm { get; set; }
        /// <summary>
        /// 研发项目
        /// </summary>
        public Value yfxm { get; set; }
        /// <summary>
        /// U9组织
        /// </summary>
        public Value u9zz1 { get; set; }
        /// <summary>
        /// 关联流程
        /// </summary>
        public Value gllc { get; set; }

    }
    /// <summary>
    /// 杂发单行
    /// </summary>
    public class MishShipLineDto
    {
        /// <summary>
        /// 料号
        /// </summary>
        public Value lh { get; set; }
        /// <summary>
        /// 品名
        /// </summary>
        public Value pm { get; set; }
        /// <summary>
        /// 存储地点
        /// </summary>
        public Value ccdd { get; set; }
        /// <summary>
        /// 杂发量
        /// </summary>
        public Value zfl { get; set; }
        /// <summary>
        /// 成本
        /// </summary>
        public Value cb { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public Value ph { get; set; }
        /// <summary>
        /// 生产订单号
        /// </summary>
        public Value scddh { get; set; }
        /// <summary>
        /// 生产相关
        /// </summary>
        public Value scxg { get; set; }
        /// <summary>
        /// 零成本
        /// </summary>
        public Value lcb { get; set; }
        /// <summary>
        /// 受益人
        /// </summary>
        public Value syr { get; set; }
        /// <summary>
        /// 受益部门
        /// </summary>
        public Value sybm { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public Value gys { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public Value bz { get; set; }

    }

}
