using System;
using System.Collections.Generic;
using UFIDA.U9.MFG.MO.MOToPOModel;
//using UFIDA.U9.MFG.MO.MOToPOModel;
//using UFSoft.UBF.UI.ActionProcess;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;
//using UFSoft.UBF.Util.DataAccess;

namespace Auctus.MOToPO.UIPlugin
{
    public class CalWPOQuotaUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart iPart;
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
            IUFButton btnCalWPOQuota = new UFWebButtonAdapter();
            btnCalWPOQuota.Text = "比例分配";
            btnCalWPOQuota.ID = "btnCalWPOQuota";
            btnCalWPOQuota.AutoPostBack = true;
            btnCalWPOQuota.ToolTip = "比列分配";
            btnCalWPOQuota.Visible = true;

            // 将按钮加入到按钮栏
            IUFCard iCard = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");
            iCard.Controls.Add(btnCalWPOQuota);
            CommonFunction.Layout(iCard, btnCalWPOQuota, 4, 0);
            btnCalWPOQuota.Click += new EventHandler(BtnCalWPOQuota_Click);
        }

        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            MOToPOUIFormWebPart m = (MOToPOUIFormWebPart)this.iPart;
            m.Action.CommonAction.Load(new IUIView[] { m.Model.MO });
            IEnumerator<IUIRecord> arr = m.Model.MO.Records.GetEnumerator();
            MORecord mm = m.Model.MO.NewUIRecord();
            m.Model.MO.Records[0].CopyTo(mm);
            int i = 0;
            using (IEnumerator<IUIRecord> enumerator2 = m.Model.MO.Records.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    MORecord mORecord = (MORecord)enumerator2.Current;
                    mORecord.IsMatchBOM = true;
                    if (i == 1)
                    {
                        mORecord.TranferQty = 15000;
                    }
                    else
                    {
                        mORecord.TranferQty = 10000;
                    }
                    i++;
                }
            }

        }

        private void BtnCalWPOQuota_Click(object sender, EventArgs e) 
        {
            //this.iPart.DataCollect();
            //this.iPart.IsDataBinding = true;
            //this.iPart.IsConsuming = false;
            //IList<IUIRecord> selectRecordFromCache = UIRuntimeHelper.Instance.GetSelectRecordFromCache(((PRToPOUIModelAction)this.iPart.Action).CurrentModel.PR_PRLineList);
            //this.TryCalc_Extend(sender, new UIActionEventArgs());
            MOToPOUIFormWebPart m = (MOToPOUIFormWebPart)this.iPart;
            m.Action.CommonAction.Load(new IUIView[] { m.Model.MO });
            IEnumerator<IUIRecord> arr = m.Model.MO.Records.GetEnumerator();            
            MORecord mm = m.Model.MO.NewUIRecord();
            m.Model.MO.Records[0].CopyTo(mm);
            int i = 0;
            using (IEnumerator<IUIRecord> enumerator2 = m.Model.MO.Records.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {                    
                    MORecord mORecord = (MORecord)enumerator2.Current;
                    mORecord.IsMatchBOM = true;                    
                    if (i == 1)
                    {
                        mORecord.TranferQty = 15000;
                    }
                    else
                    {
                        mORecord.TranferQty = 10000;
                    }
                    i++;
                }
            }
         }
        //            if (base.CurrentState["MOList"] != null)
        //            {
        //                IList<long> list = base.CurrentState["MOList"] as List<long>;
        //                if (list != null && list.Count > 0)
        //                {
        //                    string text = "";
        //                    foreach (long current in list)
        //                    {
        //                        if (text.Length == 0)
        //                        {
        //                            text = current.ToString();
        //                        }
        //                        else
        //                        {
        //                            text = text + "," + current.ToString();
        //                        }
        //                    }
        //                    this.Model.MO.CurrentFilter.OPath = "ID in (" + text + ")";
        //                    this.Action.CommonAction.Load(new IUIView[]
        //                    {
        //                        this.Model.MO
        //                    });
        //                }
        //                using (IEnumerator<IUIRecord> enumerator2 = this.Model.MO.Records.GetEnumerator())
        //                {
        //                    while (enumerator2.MoveNext())
        //                    {
        //                        MORecord mORecord = (MORecord)enumerator2.Current;
        //                        mORecord.IsMatchBOM = true;
        //                        mORecord.TranferQty = 0m;
        //                    }
        //                }
        //                this.SetSeparateConditionViewModelValue();
        //                this.SetGroupConditionViewModelValue();
        //}
    }
}
