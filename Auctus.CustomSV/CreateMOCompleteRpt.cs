using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.Context;
using www.ufida.org.EntityData;

namespace Auctus.CustomSV
{
    public class CreateMOCompleteRpt
    {
        public void CreateMC()
        {
            UFIDAU9ISVMOICreateCompRptSrvClient c = new UFIDAU9ISVMOICreateCompRptSrvClient();
            UFSoft.UBF.Exceptions.MessageBase[] m;
            www.ufida.org.EntityData.UFIDAU9ISVMOCompRptDTOData[] datas=new www.ufida.org.EntityData.UFIDAU9ISVMOCompRptDTOData[1];
            
            www.ufida.org.EntityData.UFIDAU9ISVMOCompRptDTOData d = new www.ufida.org.EntityData.UFIDAU9ISVMOCompRptDTOData();
            #region 赋值
            d.m_actualRcvTime = DateTime.Now;
            d.m_businessDate = DateTime.Now;
            d.m_completeDate = DateTime.Now;            
            //单据类型
            d.m_completeDocType = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_completeDocType.m_iD = 1001708070116426;
            // d.m_completeOp= // 完工工序
            d.m_completeQty = 10;
            d.m_completeQtyCoUOM = 10;
            d.m_completeQtyWhUOM = 10;
            //入库行
            // d.m_completeRptRcvLines = new UFIDAU9ISVMOCompRptRcvLineDTOData[0];
            d.m_description = "descprition";
            d.m_direction = 0;
            // d.m_docNo = "";
            d.m_eligibleQty = 10;//合格数量
            d.m_eligibleQtyCoUOM = 10;
            //入库部门
            d.m_handleDept = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_handleDept.m_iD = 1001708040000060;
            //入库人
            d.m_handlePerson = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_handlePerson.m_iD = 1001708040000597;
            //料品信息
            d.m_itemMaster = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_itemMaster.m_iD = 1001903230017003;
            //工单信息
            d.m_mO = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_mO.m_iD = 1001910280056739;
            d.m_org = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_org.m_iD = 1001708020135665;
            d.m_ownerOrg = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_ownerOrg.m_iD = 1001708020135665;
            // 生产线日计划
            d.m_pLS = new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            // 生产副单位
            d.m_productBaseUOM= new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_productBaseUOM.m_iD = 1001708030115592;
            // 生产单位
            d.m_productUOM= new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_productUOM.m_iD = 1001708030115592;
            d.m_pUToPBURate = 1;
            // 入库组织
            d.m_rcvOrg= new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_rcvOrg.m_iD = 1001708020135665;
            // 存储地点
            d.m_rcvWh= new UFIDAU9CBOPubControllerCommonArchiveDataDTOData();
            d.m_rcvWh.m_iD = 1001708070007744;
            // 待返工数量
            d.m_reworkingQty = 0;
            d.m_reworkingQtyCoUOM = 0;
            // 返工原因
            // d.m_reworkReason = -1;
            // 报废数量
            d.m_scrapQty = 0;
            d.m_scrapQtyCoUOM = 0;
            // d.m_scrapReason = -1;
            // d.m_sourceDoc
            //退库理由
            // d.m_whshipmentReason = -1;
            // d.sysState=

            
            #endregion
            datas[0] = d;
            try
            {
                // MyContext context = new MyContext() { CultureName="zh-CN",EntCode="20",OrgID= 1001708020135665, OrgCode="300",UserID= 1001708020000008, UserCode="daniel"};
                ThreadContext context = new ThreadContext();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                context.nameValueHas = new Dictionary<object, object>();
                context.nameValueHas.Add("CultureName","zh-CN");
                // context.nameValueHas.Add("Culture","zh-CN");
                context.nameValueHas.Add("DefaultCultureName","zh-CN");
                context.nameValueHas.Add("EnterpriseID", "10");
                context.nameValueHas.Add("OrgID", "1001708020135665");
                context.nameValueHas.Add("OrgCode", "300");
                context.nameValueHas.Add("UserID", "1001708020000008");
                context.nameValueHas.Add("UserCode", "Daniel");
                // ArrayOfKeyValueOfanyTypeanyTypeKeyValueOfanyTypeanyType
                UFIDAU9ISVMOCompRptKeyDTOData[]  arr=c.Do(context, datas, out m);
            }
            catch (Exception ex)
            {
                string s = ex.Message;                
            }
        }
    }
    //proxy.ContextDTO = new UFIDA.U9.CBO.Pub.Controller.ContextDTOData();
    //            proxy.ContextDTO.CultureName = "zh-CN";
    //            proxy.ContextDTO.EntCode = "20";
    //            proxy.ContextDTO.OrgID = 1001708020000209;
    //            proxy.ContextDTO.OrgCode = "100";
    //            proxy.ContextDTO.UserID = 1001708030131373;
    //            proxy.ContextDTO.UserCode = "sys_u9";
    public class MyContext {
        public string CultureName { get; set; }
        public string EntCode { get; set; }
        public string OrgCode { get; set; }
        public long OrgID { get; set; }
        public string UserClientID { get; set; }
        public string UserCode { get; set; }
        public long UserID { get; set; }
        public string UserPwd { get; set; }
    }
}
