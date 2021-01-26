using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using UFIDA.U9.UI.PDHelper;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.UI;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomUI
{
    public class MOUI : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart uiPart;
        //private IUFDataGrid datagrid;
        //private IUFFldReference ctrlSelect;
        private string ctrlSelectClientID = string.Empty;

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="e"></param>
        public override void AfterInit(IPart Part, EventArgs e)
        {
            base.AfterInit(Part, e);
            uiPart = Part;
            //OA流程按钮
            IUFButton btnOAFlow = new UFWebButtonAdapter();
            btnOAFlow.Text = "查看流程";
            btnOAFlow.ToolTip = "查看流程";
            btnOAFlow.ID = "btnOAFlow";
            btnOAFlow.AutoPostBack = true;
            btnOAFlow.Visible = true;

            //OA交期变更按钮
            IUFButton btnOAMOModify = new UFWebButtonAdapter();
            btnOAMOModify.Text = "OA交期变更";
            btnOAMOModify.ToolTip = "OA交期变更";
            btnOAMOModify.ID = "btnOAMOModify";
            btnOAMOModify.AutoPostBack = true;
            btnOAMOModify.Visible = true;
            btnOAMOModify.Enabled = true;

            IUFCard card = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card1");
            card.Controls.Add(btnOAFlow);
            CommonFunction.Layout(card, btnOAFlow, 18, 0);
            btnOAFlow.Click += btnOAFlow_Click;

            card.Controls.Add(btnOAMOModify);
            CommonFunction.Layout(card, btnOAMOModify, 20, 0);
            btnOAMOModify.Click += btnOAMOModify_Click;

        }
        /// <summary>
        /// 页面加载后事件
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="args"></param>
        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            IUFControl ctrl = CommonFunction.FindControl(Part, "Card1", "btnOAFlow");
            if (ctrl != null)
            {
                IUIRecord rec = uiPart.Model.Views["MO"].FocusedRecord;
                if (rec != null && Int64.Parse(rec["ID"].ToString()) > 0)
                {
                    ctrl.Enabled = true;
                }
                else
                {
                    ctrl.Enabled = false;
                }
                if (ctrl.Enabled)
                {
                    //查询OA的流程ID
                    string sql = string.Format(@"SELECT a.DocState,a.DescFlexField_PrivateDescSeg7 AS OAFlowID,b.DescFlexField_PrivateDescSeg1 IsToOA
                        FROM dbo.MO_MO a INNER JOIN dbo.MO_MODocType b ON a.MODocType=b.ID WHERE a.ID={0}", rec["ID"].ToString());
                    DataSet ds = new DataSet();
                    string OAFlowID = "";
                    string IsToOA = "";
                    string docState = "";
                    DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        OAFlowID = row["OAFlowID"].ToString();
                        IsToOA = row["IsToOA"].ToString();
                        docState = row["DocState"].ToString();                        
                    }
                    if (IsToOA=="1")
                    {
                        IUFToolbar bar = (IUFToolbar)Part.GetUFControlByName(Part.TopLevelContainer, "Toolbar1");
                        foreach (IUFControl ctrl1 in bar.Controls)
                        {
                            switch (ctrl1.ID)
                            {
                                case "BtnApprove":
                                    ctrl1.Enabled = false;
                                    break;
                                case "BtnDelete":
                                    if (docState=="4")
                                    {
                                        ctrl1.Enabled = false;
                                    }
                                    break;
                                case "BtnSave":
                                    if (docState=="4")
                                    {
                                        ctrl1.Enabled = false;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        //OA流程的工单，审核中不允许修改
                    }
                    if (string.IsNullOrEmpty(OAFlowID))
                    {
                        ctrl.Enabled = false;
                    }
                    else
                    {                 
                        ctrl.Enabled = true;
                    }
                }
            }
        }
        /// <summary>
        /// 查看OA流程按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnOAFlow_Click(object sender, EventArgs e)
        {
            IUIRecord headRec = uiPart.Model.Views["MO"].FocusedRecord;
            if (headRec == null)
                return;
            //查询OA的流程ID
            string sql = string.Format(@"SELECT DescFlexField_PrivateDescSeg7 AS OAFlowID
                        FROM dbo.MO_MO WHERE ID={0}", headRec["ID"].ToString());
            DataSet ds = new DataSet();
            string OAFlowID = "";
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OAFlowID = row["OAFlowID"].ToString();
            }
            string userCode = PDContext.Current.UserCode;
            //OA流程页面Url
            string script1 = PubFunction.GetOAFlowScript(userCode, OAFlowID);
            AtlasHelper.RegisterAtlasStartupScript
             ((Control)this.uiPart.TopLevelContainer, this.uiPart.GetType(), "ReferenceReturn", script1, false);

        }
        /// <summary>
        /// 查看OA交期变更报表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnOAMOModify_Click(object sender, EventArgs e)
        {
            IUIRecord headRec = uiPart.Model.Views["MO"].FocusedRecord;
            if (headRec == null)
                return;
            string userCode = PDContext.Current.UserCode;
            //OA流程页面Url
            string UserCodeBase64 = CommonFunction.Base64Encode(Encoding.UTF8, userCode);
            string OAIP = CommonFunction.GetOAIP();
            string url = "http://" + OAIP + string.Format("/Auctus/jsp/ModifyDoc.jsp?usercode={0}", System.Web.HttpUtility.UrlEncode(UserCodeBase64, Encoding.UTF8));
            string script1 = "<script language=\"javascript\">";
            script1 += string.Format(" window.open(\"{0}\");", url);
            script1 += "</script>";
            AtlasHelper.RegisterAtlasStartupScript
             ((Control)this.uiPart.TopLevelContainer, this.uiPart.GetType(), "ReferenceReturn", script1, false);

        }
    }
}
