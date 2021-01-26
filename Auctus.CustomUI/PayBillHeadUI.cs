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
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.CustomUI
{
    public class PayBillHeadUI : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart uiPart;
        private IUFDataGrid datagrid;
        private IUFFldReference ctrlSelect;
        private string ctrlSelectClientID = string.Empty;
        public override void AfterEventProcess(IPart Part, string eventName, object sender, EventArgs args)
        {
            base.AfterEventProcess(Part, eventName, sender, args);
            uiPart = Part;
            UFWebButton4ToolbarAdapter adapter = sender as UFWebButton4ToolbarAdapter;
            IUIRecord porec = uiPart.Model.Views["PayBillHead"].FocusedRecord;
            //弃审后，更新标记
            if ((adapter != null) && (adapter.Action == "UnApproveClick"))
            {
                ILogger logger = LoggerManager.GetLogger(typeof(CacheManager));
                logger.Error(string.Format("单号为：{0}的付款单弃审了!", porec["DocNo"].ToString()));
                //string UpSQL = string.Format(@"UPDATE InvDoc_MiscShip set DescFlexField_PrivateDescSeg3='' WHERE ID = {0}", porec["ID"].ToString());
                //DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), UpSQL, null);
            }
        }

        public override void AfterInit(IPart Part, EventArgs e)
        {
            base.AfterInit(Part, e);
            uiPart = Part;
            IUFButton btnForPlugIn = new UFWebButtonAdapter();
            btnForPlugIn.Text = "OA请款流程";
            btnForPlugIn.ToolTip = "OA请款流程";
            btnForPlugIn.ID = "btnForPlugIn";
            btnForPlugIn.AutoPostBack = true;
            btnForPlugIn.Visible = true;

            IUFCard card = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");
            card.Controls.Add(btnForPlugIn);
            CommonFunction.Layout(card, btnForPlugIn, 16, 0);
            btnForPlugIn.Click += btnPlugIn_Click;

        }
        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            IUFControl ctrl = CommonFunction.FindControl(Part, "Card0", "btnForPlugIn");
            if (ctrl != null)
            {
                IUIRecord rec = uiPart.Model.Views["PayBillHead"].FocusedRecord;
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
                    string sql = string.Format(@"SELECT DescFlexField_PrivateDescSeg1 AS OAFlowID
                        FROM dbo.AP_PayBillHead WHERE ID={0}", rec["ID"].ToString());
                    DataSet ds = new DataSet();
                    string OAFlowID = "";
                    DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        OAFlowID = row["OAFlowID"].ToString();
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
        public void btnPlugIn_Click(object sender, EventArgs e)
        {
            IUIRecord headRec = uiPart.Model.Views["PayBillHead"].FocusedRecord;
            if (headRec == null)
                return;
            //查询OA的流程ID
            string sql = string.Format(@"SELECT DescFlexField_PrivateDescSeg1 AS OAFlowID
                        FROM dbo.AP_PayBillHead WHERE ID={0}", headRec["ID"].ToString());
            DataSet ds = new DataSet();
            string OAFlowID = "";
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OAFlowID = row["OAFlowID"].ToString();
            }
            string UserCode = PDContext.Current.UserCode;
            string UserCodeBase64 = CommonFunction.Base64Encode(Encoding.UTF8, UserCode);
            string OAIP = CommonFunction.GetOAIP();
            string url = "http://" + OAIP + string.Format("/Auctus/jsp/WorkflowSSO.jsp?usercode={0}&requestid={1}", HttpUtility.UrlEncode(UserCodeBase64, Encoding.UTF8), OAFlowID);
            string script1 = "<script language=\"javascript\">";
            script1 += string.Format(" window.open(\"{0}\");", url);
            script1 += "</script>";
            AtlasHelper.RegisterAtlasStartupScript
             ((Control)this.uiPart.TopLevelContainer, this.uiPart.GetType(), "ReferenceReturn", script1, false);

        }
    }
}