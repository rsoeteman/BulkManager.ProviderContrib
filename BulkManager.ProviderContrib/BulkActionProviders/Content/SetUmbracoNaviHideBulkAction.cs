using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using BulkManager.Extensions.Extensions;
using BulkManager.Extensions.Providers.Attributes;
using BulkManager.Extensions.Providers.BulkAction;
using Umbraco.Web;

namespace BulkManager.ProviderContrib.BulkActionProviders.Content
{
    /// <summary>
    /// Bulkaction control to allow setting of UmbracoNavide.
    /// </summary>
    [BulkActionProvider(Id = "Content.SetUmbracoNaviHide", BulkManagerProviderId = "BulkManagerContentProvider", Icon = "icon-bulleted-list", SortOrder = 220)]
    public class SetUmbracoNaviHideBulkAction : BulkActionProvider
    {
        private readonly RadioButtonList _radioButtonList = new RadioButtonList();
        private readonly CheckBox _autoPublishCheckBox = new CheckBox();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _radioButtonList.Items.Add(new ListItem("Set umbracoNavihide to false", "false"));
            _radioButtonList.Items.Add(new ListItem("Set umbracoNavihide to true", "true"));
            _radioButtonList.DataBind();
            _radioButtonList.SelectedValue = "false";

            _autoPublishCheckBox.Checked = true;
            _autoPublishCheckBox.Text = "Publish document after update";

            Controls.Add(_radioButtonList);
            Controls.Add(new LiteralControl("<br/>"));
            Controls.Add(_autoPublishCheckBox);
            Controls.Add(new LiteralControl("<br/>"));
        }

        /// <summary>
        /// Iterate over selected document id's and set umbracoNaviHide property
        /// </summary>
        public override void Execute(List<string> selected, BulkActionItem item, int userId)
        {
            //Get selected umbracoNaviHide option (1 (true)/ 0 false)
            var umbracoNavidHide = _radioButtonList.SelectedValue.ToBool() ? 1 : 0;

            //Get Autopublish 
            var autoPublish = _autoPublishCheckBox.Checked;
          
            var i = 0;
            foreach (var id in selected)
            {
                i++;
                lock (_lock)
                {
                    try
                    {
                        //Set status message
                        item.StatusMessage = FormatProgressMessage("Updating", i, selected.Count);

                        //Get document from Umbraco
                        var content = UmbracoContext.Current.Application.Services.ContentService.GetById(id.ToInt());
                        
                        //Check for property existence
                        if (content.HasProperty("umbracoNaviHide"))
                        {

                            //Get umbracoNaviHide property
                            var prop = content.Properties["umbracoNaviHide"];
                            //Set value
                            prop.Value = umbracoNavidHide;

                            //Save (and publish when selected)
                            if (autoPublish)
                            {
                                UmbracoContext.Current.Application.Services.ContentService.SaveAndPublishWithStatus(content);
                            }
                            else
                            {
                                UmbracoContext.Current.Application.Services.ContentService.Save(content);
                            }

                        }
                        
                    }
                    catch (InvalidOperationException ex)
                    {
                        //Hack for peta poco transaction issue.   
                    }
                }
            }
        }

        public override string Name
        {
            get { return "Set umbracoNaviHide property"; }
        }

        public override string DialogIntro
        {
            get { return "Set hide in navigation (umbracoNaviHide) property to false, or true and click update to update all selected documents"; }
        }

        public override string ButtonText
        {
            get { return "Update"; }
        }

        public override string FinishedMessage
        {
            get { return "Selected documents are updated"; }
        }
    }
}
