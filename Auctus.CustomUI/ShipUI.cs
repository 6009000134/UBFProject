using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// <summary>
    /// 出货单UI插件
    /// </summary>
    public class ShipUI : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart uiPart;
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
            btnOAFlow.Enabled = false;

            IUFCard card = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");
            card.Controls.Add(btnOAFlow);
            CommonFunction.Layout(card, btnOAFlow, 18, 0);
            btnOAFlow.Click += btnOAFlow_Click;
        }
        /// <summary>
        /// 页面加载后事件
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="args"></param>
        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            IUFControl btnOAFlow = CommonFunction.FindControl(Part, "Card0", "btnOAFlow");
            IUIRecord rec = uiPart.Model.Views["Ship"].FocusedRecord;
            if (rec != null && Convert.ToInt64(rec["ID"].ToString()) > 0)
            {
                //btnQuickCreate
                //查询OA的流程ID
                string sql = string.Format(@"SELECT 
a.DescFlexField_PrivateDescSeg6 OAFlowID,b.DescFlexField_PrivateDescSeg1 IsToOA,a.Status
FROM dbo.SM_Ship a INNER JOIN dbo.SM_ShipDocType b ON a.DocumentType=b.ID WHERE a.ID={0}", rec["ID"].ToString());
                DataSet ds = new DataSet();
                string OAFlowID = "";
                string IsToOA = "";
                string docState = "";
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    OAFlowID = row["OAFlowID"].ToString();
                    IsToOA = row["IsToOA"].ToString();
                    docState = row["Status"].ToString();
                }
                if (string.IsNullOrEmpty(OAFlowID))//未提交
                {
                    btnOAFlow.Enabled = false;
                }
                else
                {
                    btnOAFlow.Enabled = true;
                }
                if (IsToOA == "1")
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
                                if (docState == "2")
                                {
                                    ctrl1.Enabled = false;
                                }
                                break;
                            case "BtnSave":
                                if (docState == "2")
                                {
                                    ctrl1.Enabled = false;
                                }
                                break;
                            case "BtnRevocate":
                                ctrl1.Enabled = false;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 打开OA流程页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnOAFlow_Click(object sender, EventArgs e)
        {
            IUIRecord rec = uiPart.Model.Views["Ship"].FocusedRecord;
            string sql = string.Format(@"SELECT  a.DescFlexField_PrivateDescSeg6 OAFlowID 
FROM dbo.SM_Ship a WHERE a.ID={0}", rec["ID"].ToString());
            DataSet ds = new DataSet();
            string OAFlowID = "";
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OAFlowID = row["OAFlowID"].ToString();
            }
            string script = PubFunction.GetOAFlowScript(PDContext.Current.UserCode, OAFlowID);//打开流程页面脚本
            AtlasHelper.RegisterAtlasStartupScript
             ((Control)this.uiPart.TopLevelContainer, this.uiPart.GetType(), "ReferenceReturn", script, false);
        }

    }
}
