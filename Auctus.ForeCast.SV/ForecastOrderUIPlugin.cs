using System;
using System.Collections.Generic;
using System.Data;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.Util.DataAccess;
using UFIDA.U9.CBO.Pub.Controller;
using UFIDA.U9.Base;
using UFSoft.UBF.Business;

namespace Auctus.ForeCast.SV
{
    /// <summary>
    /// 预测订单创建按钮
    /// 点击按钮创建预测订单
    /// </summary>
    public class ForecastOrderUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart iPart;
        #region 添加按钮控件
        /// <summary>
        /// UI界面布局，添加按钮控件
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="args"></param>
        public override void AfterInit(IPart Part, EventArgs args)
        {
            base.AfterInit(Part, args);
            iPart = Part;
            // 增加备料预留量查询按钮
            IUFButton btnCreateForecast = new UFWebButtonAdapter();
            btnCreateForecast.Text = "创建预测订单";
            btnCreateForecast.ID = "btnCreateForecast";
            btnCreateForecast.AutoPostBack = true;
            btnCreateForecast.ToolTip = "创建变更单";
            btnCreateForecast.Visible = true;

            // 将按钮加入到按钮栏
            IUFCard iCard = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");
            iCard.Controls.Add(btnCreateForecast);
            CommonFunction.Layout(iCard, btnCreateForecast, 16, 0);
            btnCreateForecast.Click += new EventHandler(BtnCreateForecast_Click2);
        }
        #endregion


        public void BtnCreateForecast_Click2(object o , EventArgs sender)
        {
            DataParamList dp = new DataParamList();
            DataSet ds = new DataSet();
            DataAccessor.RunSP("sp_Auctus_BE_GetForeCastList", dp,out ds);
            if (ds.Tables.Count>0)
            {
                if (ds.Tables[0].Rows.Count>0)//有数据
                {
                    UFIDA.U9.ISV.SM.Proxy.CreateForecastOrderSRVProxy sv = new UFIDA.U9.ISV.SM.Proxy.CreateForecastOrderSRVProxy();
                    //预测订单列表
                    sv.ForecastOrderDTOs = new List<UFIDA.U9.ISV.SM.ForecastOrderDTOData>();
                    UFIDA.U9.ISV.SM.ForecastOrderDTOData fc = new UFIDA.U9.ISV.SM.ForecastOrderDTOData();
                    fc.ForecastOrderLine = new List<UFIDA.U9.ISV.SM.ForecastOrderLineDTOData>();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        //预测订单行
                        UFIDA.U9.ISV.SM.ForecastOrderLineDTOData fcLine = new UFIDA.U9.ISV.SM.ForecastOrderLineDTOData();
                        #region 预测订单行赋值
                        fcLine.ItemInfo = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//料号信息
                        //UFIDA.U9.CBO.SCM.Item.ItemMaster item = UFIDA.U9.CBO.SCM.Item.ItemMaster.Finder.Find();
                        fcLine.ItemInfo.ID = Convert.ToInt64(ds.Tables[0].Rows[i]["Itemmaster"].ToString());
                        fcLine.ItemInfo.Code = ds.Tables[0].Rows[i]["Code"].ToString();
                        fcLine.ItemInfo.Name = ds.Tables[0].Rows[i]["Name"].ToString();
                        fcLine.ShipPlanDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["DeliveryDate"]);//计划出货日期
                        fcLine.TU = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//单位
                        fcLine.TU.ID = 1001708030115592;
                        fcLine.TU.Code = "SL01";
                        fcLine.TU.Name = "PCS";
                        fcLine.Num = Convert.ToInt32(ds.Tables[0].Rows[i]["Qty"]);//数量
                        fcLine.Customer = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//客户信息
                        fcLine.Customer.ID = Convert.ToInt64(ds.Tables[0].Rows[i]["CustomerID"].ToString()); 
                        fcLine.Customer.Code = ds.Tables[0].Rows[i]["CustomerCode"].ToString();
                        fcLine.Customer.Name = ds.Tables[0].Rows[i]["CustomerName"].ToString();
                        fcLine.ShiperOrg = new CommonArchiveDataDTOData();//货主组织
                        //fcLine.ShiperOrg.ID = Context.LoginOrg.ID;
                        //fcLine.ShiperOrg.Code = Context.LoginOrg.Code;
                        //fcLine.ShiperOrg.Name = Context.LoginOrg.Name;
                        fcLine.ShiperOrg.ID = Convert.ToInt64(ds.Tables[0].Rows[i]["OrgID"].ToString());
                        fcLine.ShiperOrg.Code = ds.Tables[0].Rows[i]["OrgCode"].ToString();
                        fcLine.ShiperOrg.Name = ds.Tables[0].Rows[i]["OrgName"].ToString();
                        fcLine.SupplyOrg = new CommonArchiveDataDTOData();//供应组织
                        fcLine.SupplyOrg.ID = Convert.ToInt64(ds.Tables[0].Rows[i]["OrgID"].ToString());
                        fcLine.SupplyOrg.Code = ds.Tables[0].Rows[i]["OrgCode"].ToString();
                        fcLine.SupplyOrg.Name = ds.Tables[0].Rows[i]["OrgName"].ToString();
                        fcLine.ShipToSite = new CommonArchiveDataDTOData();//客户收货位置，数据在客户档案下的位置页签
                        fcLine.ShipToSite.ID = Convert.ToInt64(ds.Tables[0].Rows[i]["ShipToSiteID"].ToString());
                        fcLine.ShipToSite.Code = ds.Tables[0].Rows[i]["ShipToSiteCode"].ToString();
                        fcLine.ShipToSite.Name = ds.Tables[0].Rows[i]["ShipToSiteName"].ToString();
                        #endregion
                        fc.ForecastOrderLine.Add(fcLine);
                    }
                    #region 预测订单头赋值
                    fc.DocumentTypeDTO = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//单据类型
                    fc.DocumentTypeDTO.ID = Convert.ToInt64(ds.Tables[0].Rows[0]["DocTypeID"].ToString()); ;
                    fc.DocumentTypeDTO.Code = ds.Tables[0].Rows[0]["DocTypeCode"].ToString();
                    fc.DocumentTypeDTO.Name = ds.Tables[0].Rows[0]["DocTypeName"].ToString();
                    fc.CustomerDTO = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//客户
                    fc.CustomerDTO.ID = Convert.ToInt64(ds.Tables[0].Rows[0]["CustomerID"].ToString()); 
                    fc.CustomerDTO.Code = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
                    fc.CustomerDTO.Name = ds.Tables[0].Rows[0]["CustomerName"].ToString();
                    fc.OrderOperator = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//业务员
                    fc.OrderOperator.ID = Convert.ToInt64(ds.Tables[0].Rows[0]["OperatorID"].ToString()); 
                    fc.OrderOperator.Code = ds.Tables[0].Rows[0]["OperatorCode"].ToString();
                    fc.OrderOperator.Name = ds.Tables[0].Rows[0]["OperatorName"].ToString();
                    fc.Department = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//部门
                    fc.Department.ID = Convert.ToInt64(ds.Tables[0].Rows[0]["DeptID"].ToString()); 
                    fc.Department.Code = ds.Tables[0].Rows[0]["DeptCode"].ToString();
                    fc.Department.Name = ds.Tables[0].Rows[0]["DeptName"].ToString();
                    fc.BusinessDate = DateTime.Now;//业务日期                    
                    fc.ShipPlanDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["DeliveryDate"]);//计划出货日期
                    fc.Note = "系统生成"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//备注                    
                    #endregion
                    sv.ForecastOrderDTOs.Add(fc);
                    sv.Do();
                }
            }
        }
    }
}
