// Decompiled with JetBrains decompiler
// Type: Sitecore.Shell.Applications.Layouts.DeviceEditor.DeviceEditorForm
// Assembly: Sitecore.Client, Version=11.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 4FFBBECE-5901-4E27-9E4A-52BD23424A42
// Assembly location: C:\inetpub\wwwroot\SIM\sc900\Website\bin\Sitecore.Client.dll

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Pipelines.RenderDeviceEditorRendering;
using Sitecore.Resources;
using Sitecore.Rules;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.Dialogs;
using Sitecore.Shell.Applications.Dialogs.ItemLister;
using Sitecore.Shell.Applications.Dialogs.Personalize;
using Sitecore.Shell.Applications.Dialogs.Testing;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;
using System;
using System.Collections;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;

namespace Sitecore.Support.Shell.Applications.Layouts.DeviceEditor
{
  /// <summary>Represents the Device Editor form.</summary>
  [UsedImplicitly]
  public class DeviceEditorForm : DialogForm
  {
    /// <summary>The command name.</summary>
    private const string CommandName = "device:settestdetails";

    /// <summary>Gets or sets the controls.</summary>
    /// <value>The controls.</value>
    public ArrayList Controls
    {
      get
      {
        return (ArrayList)Context.ClientPage.ServerProperties[nameof(Controls)];
      }
      set
      {
        Assert.ArgumentNotNull((object)value, nameof(value));
        Context.ClientPage.ServerProperties[nameof(Controls)] = (object)value;
      }
    }

    /// <summary>Gets or sets the device ID.</summary>
    /// <value>The device ID.</value>
    public string DeviceID
    {
      get
      {
        return StringUtil.GetString(Context.ClientPage.ServerProperties[nameof(DeviceID)]);
      }
      set
      {
        Assert.ArgumentNotNullOrEmpty(value, nameof(value));
        Context.ClientPage.ServerProperties[nameof(DeviceID)] = (object)value;
      }
    }

    /// <summary>Gets or sets the index of the selected.</summary>
    /// <value>The index of the selected.</value>
    public int SelectedIndex
    {
      get
      {
        return MainUtil.GetInt(Context.ClientPage.ServerProperties[nameof(SelectedIndex)], -1);
      }
      set
      {
        Context.ClientPage.ServerProperties[nameof(SelectedIndex)] = (object)value;
      }
    }

    /// <summary>Gets or sets the unique id.</summary>
    /// <value>The unique id.</value>
    public string UniqueId
    {
      get
      {
        return StringUtil.GetString(Context.ClientPage.ServerProperties["PlaceholderUniqueID"]);
      }
      set
      {
        Assert.ArgumentNotNullOrEmpty(value, nameof(value));
        Context.ClientPage.ServerProperties["PlaceholderUniqueID"] = (object)value;
      }
    }

    /// <summary>Gets or sets the layout.</summary>
    /// <value>The layout.</value>
    protected TreePicker Layout { get; set; }

    /// <summary>Gets or sets the placeholders.</summary>
    /// <value>The placeholders.</value>
    protected Scrollbox Placeholders { get; set; }

    /// <summary>Gets or sets the renderings.</summary>
    /// <value>The renderings.</value>
    protected Scrollbox Renderings { get; set; }

    /// <summary>Gets or sets the test.</summary>
    /// <value>The test button.</value>
    protected Button Test { get; set; }

    /// <summary>Gets or sets the personalize button control.</summary>
    /// <value>The personalize button control.</value>
    protected Button Personalize { get; set; }

    /// <summary>Gets or sets the edit.</summary>
    /// <value>The edit button.</value>
    protected Button btnEdit { get; set; }

    /// <summary>Gets or sets the change.</summary>
    /// <value>The change button.</value>
    protected Button btnChange { get; set; }

    /// <summary>Gets or sets the remove.</summary>
    /// <value>The Remove button.</value>
    protected Button btnRemove { get; set; }

    /// <summary>Gets or sets the move up.</summary>
    /// <value>The Move Up button.</value>
    protected Button MoveUp { get; set; }

    /// <summary>Gets or sets the move down.</summary>
    /// <value>The Move Down button.</value>
    protected Button MoveDown { get; set; }

    /// <summary>Gets or sets the Edit placeholder button.</summary>
    /// <value>The Edit placeholder button.</value>
    protected Button phEdit { get; set; }

    /// <summary>Gets or sets the phRemove button.</summary>
    /// <value>Remove place holder button.</value>
    protected Button phRemove { get; set; }

    /// <summary>Adds the specified arguments.</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:add", true)]
    [UsedImplicitly]
    protected void Add(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (args.IsPostBack)
      {
        if (!args.HasResult)
          return;
        string[] strArray = args.Result.Split(',');
        string str1 = strArray[0];
        string str2 = strArray[1].Replace("-c-", ",");
        int num = strArray[2] == "1" ? 1 : 0;
        LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
        DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
        RenderingDefinition renderingDefinition = new RenderingDefinition()
        {
          ItemID = str1,
          Placeholder = str2
        };
        device.AddRendering(renderingDefinition);
        DeviceEditorForm.SetDefinition(layoutDefinition);
        this.Refresh();
        if (num != 0)
        {
          ArrayList renderings = device.Renderings;
          if (renderings != null)
          {
            this.SelectedIndex = renderings.Count - 1;
            Context.ClientPage.SendMessage((object)this, "device:edit");
          }
        }
        Registry.SetString("/Current_User/SelectRendering/Selected", str1);
      }
      else
      {
        SelectRenderingOptions renderingOptions = new SelectRenderingOptions()
        {
          ShowOpenProperties = true,
          ShowPlaceholderName = true,
          PlaceholderName = string.Empty
        };
        string path = Registry.GetString("/Current_User/SelectRendering/Selected");
        if (!string.IsNullOrEmpty(path))
          renderingOptions.SelectedItem = Client.ContentDatabase.GetItem(path);
        SheerResponse.ShowModalDialog(renderingOptions.ToUrlString(Client.ContentDatabase).ToString(), true);
        args.WaitForPostBack();
      }
    }

    /// <summary>Adds the specified arguments.</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:addplaceholder", true)]
    [UsedImplicitly]
    protected void AddPlaceholder(ClientPipelineArgs args)
    {
      if (args.IsPostBack)
      {
        if (string.IsNullOrEmpty(args.Result) || !(args.Result != "undefined"))
          return;
        LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
        DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
        string placeholderKey;
        Item dialogResult = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out placeholderKey);
        if (dialogResult == null || string.IsNullOrEmpty(placeholderKey))
          return;
        PlaceholderDefinition placeholderDefinition = new PlaceholderDefinition()
        {
          UniqueId = ID.NewID.ToString(),
          MetaDataItemId = dialogResult.Paths.FullPath,
          Key = placeholderKey
        };
        device.AddPlaceholder(placeholderDefinition);
        DeviceEditorForm.SetDefinition(layoutDefinition);
        this.Refresh();
      }
      else
      {
        SheerResponse.ShowModalDialog(new SelectPlaceholderSettingsOptions()
        {
          IsPlaceholderKeyEditable = true
        }.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    /// <summary>Adds the specified arguments.</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:change", true)]
    [UsedImplicitly]
    protected void Change(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (this.SelectedIndex < 0)
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
      if (renderings == null)
        return;
      RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
      if (renderingDefinition == null || string.IsNullOrEmpty(renderingDefinition.ItemID))
        return;
      if (args.IsPostBack)
      {
        if (!args.HasResult)
          return;
        string[] strArray = args.Result.Split(',');
        renderingDefinition.ItemID = strArray[0];
        int num = strArray[2] == "1" ? 1 : 0;
        DeviceEditorForm.SetDefinition(layoutDefinition);
        this.Refresh();
        if (num == 0)
          return;
        Context.ClientPage.SendMessage((object)this, "device:edit");
      }
      else
      {
        SelectRenderingOptions renderingOptions = new SelectRenderingOptions();
        renderingOptions.ShowOpenProperties = true;
        renderingOptions.ShowPlaceholderName = false;
        renderingOptions.PlaceholderName = string.Empty;
        renderingOptions.SelectedItem = Client.ContentDatabase.GetItem(renderingDefinition.ItemID);
        SheerResponse.ShowModalDialog(renderingOptions.ToUrlString(Client.ContentDatabase).ToString(), true);
        args.WaitForPostBack();
      }
    }

    /// <summary>Edits the specified arguments.</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:edit", true)]
    [UsedImplicitly]
    protected void Edit(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (!new RenderingParameters()
      {
        Args = args,
        DeviceId = this.DeviceID,
        SelectedIndex = this.SelectedIndex,
        Item = UIUtil.GetItemFromQueryString(Client.ContentDatabase)
      }.Show())
        return;
      this.Refresh();
    }

    /// <summary>Edits the placeholder.</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:editplaceholder", true)]
    [UsedImplicitly]
    protected void EditPlaceholder(ClientPipelineArgs args)
    {
      if (string.IsNullOrEmpty(this.UniqueId))
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      PlaceholderDefinition placeholder = layoutDefinition.GetDevice(this.DeviceID).GetPlaceholder(this.UniqueId);
      if (placeholder == null)
        return;
      if (args.IsPostBack)
      {
        if (string.IsNullOrEmpty(args.Result) || !(args.Result != "undefined"))
          return;
        string placeholderKey;
        Item dialogResult = SelectPlaceholderSettingsOptions.ParseDialogResult(args.Result, Client.ContentDatabase, out placeholderKey);
        if (dialogResult == null)
          return;
        placeholder.MetaDataItemId = dialogResult.Paths.FullPath;
        placeholder.Key = placeholderKey;
        DeviceEditorForm.SetDefinition(layoutDefinition);
        this.Refresh();
      }
      else
      {
        Item obj = string.IsNullOrEmpty(placeholder.MetaDataItemId) ? (Item)null : Client.ContentDatabase.GetItem(placeholder.MetaDataItemId);
        SelectPlaceholderSettingsOptions placeholderSettingsOptions = new SelectPlaceholderSettingsOptions();
        placeholderSettingsOptions.TemplateForCreating = (Template)null;
        placeholderSettingsOptions.PlaceholderKey = placeholder.Key;
        placeholderSettingsOptions.CurrentSettingsItem = obj;
        placeholderSettingsOptions.SelectedItem = obj;
        placeholderSettingsOptions.IsPlaceholderKeyEditable = true;
        SheerResponse.ShowModalDialog(placeholderSettingsOptions.ToUrlString().ToString(), "460px", "460px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    /// <summary>The set test</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:test", true)]
    [UsedImplicitly]
    protected void SetTest(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (this.SelectedIndex < 0)
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
      ArrayList renderings = device.Renderings;
      if (renderings == null)
        return;
      RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
      if (renderingDefinition == null)
        return;
      if (args.IsPostBack)
      {
        if (!args.HasResult)
          return;
        if (args.Result == "#reset#")
        {
          renderingDefinition.MultiVariateTest = string.Empty;
          DeviceEditorForm.SetDefinition(layoutDefinition);
          this.Refresh();
        }
        else
        {
          ID dialogResult = SetTestDetailsOptions.ParseDialogResult(args.Result);
          if (ID.IsNullOrEmpty(dialogResult))
          {
            SheerResponse.Alert("Item not found.", Array.Empty<string>());
          }
          else
          {
            renderingDefinition.MultiVariateTest = dialogResult.ToString();
            DeviceEditorForm.SetDefinition(layoutDefinition);
            this.Refresh();
          }
        }
      }
      else
      {
        Command command = CommandManager.GetCommand("device:settestdetails");
        Assert.IsNotNull((object)command, "deviceTestCommand");
        command.Execute(new CommandContext()
        {
          Parameters = {
            ["deviceDefinitionId"] = device.ID,
            ["renderingDefinitionUniqueId"] = renderingDefinition.UniqueId
          }
        });
        args.WaitForPostBack();
      }
    }

    /// <summary>Raises the load event.</summary>
    /// <param name="e">
    /// The <see cref="T:System.EventArgs" /> instance containing the event data.
    /// </param>
    /// <remarks>
    /// This method notifies the server control that it should perform actions common to each HTTP
    /// request for the page it is associated with, such as setting up a database query. At this
    /// stage in the page life cycle, server controls in the hierarchy are created and initialized,
    /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
    /// property to determine whether the page is being loaded in response to a client post back,
    /// or if it is being loaded and accessed for the first time.
    /// </remarks>
    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull((object)e, nameof(e));
      base.OnLoad(e);
      if (Context.ClientPage.IsEvent)
        return;
      this.DeviceID = WebUtil.GetQueryString("de");
      DeviceDefinition device = DeviceEditorForm.GetLayoutDefinition().GetDevice(this.DeviceID);
      if (device.Layout != null)
        this.Layout.Value = device.Layout;
      this.Personalize.Visible = Policy.IsAllowed("Page Editor/Extended features/Personalization");
      Command command = CommandManager.GetCommand("device:settestdetails");
      this.Test.Visible = command != null && command.QueryState(CommandContext.Empty) != CommandState.Hidden;
      this.Refresh();
      this.SelectedIndex = -1;
    }

    /// <summary>Handles a click on the OK button.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The arguments.</param>
    /// <remarks>
    /// When the user clicks OK, the dialog is closed by calling
    /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.
    /// </remarks>
    protected override void OnOK(object sender, EventArgs args)
    {
      Assert.ArgumentNotNull(sender, nameof(sender));
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (this.Layout.Value.Length > 0)
      {
        Item obj = Client.ContentDatabase.GetItem(this.Layout.Value);
        if (obj == null)
        {
          Context.ClientPage.ClientResponse.Alert("Layout not found.");
          return;
        }
        if (obj.TemplateID == TemplateIDs.Folder || obj.TemplateID == TemplateIDs.Node)
        {
          Context.ClientPage.ClientResponse.Alert(Translate.Text("\"{0}\" is not a layout.", (object)obj.GetUIDisplayName()));
          return;
        }
      }
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
      ArrayList renderings = device.Renderings;
      if (renderings != null && renderings.Count > 0 && this.Layout.Value.Length == 0)
      {
        Context.ClientPage.ClientResponse.Alert("You must specify a layout when you specify renderings.");
      }
      else
      {
        device.Layout = this.Layout.Value;
        DeviceEditorForm.SetDefinition(layoutDefinition);
        Context.ClientPage.ClientResponse.SetDialogValue("yes");
        base.OnOK(sender, args);
      }
    }

    /// <summary>Called when the rendering has click.</summary>
    /// <param name="uniqueId">The unique Id.</param>
    [UsedImplicitly]
    protected void OnPlaceholderClick(string uniqueId)
    {
      Assert.ArgumentNotNullOrEmpty(uniqueId, nameof(uniqueId));
      if (!string.IsNullOrEmpty(this.UniqueId))
        SheerResponse.SetStyle("ph_" + (object)ID.Parse(this.UniqueId).ToShortID(), "background", string.Empty);
      this.UniqueId = uniqueId;
      if (!string.IsNullOrEmpty(uniqueId))
        SheerResponse.SetStyle("ph_" + (object)ID.Parse(uniqueId).ToShortID(), "background", "#D0EBF6");
      this.UpdatePlaceholdersCommandsState();
    }

    /// <summary>Called when the rendering has click.</summary>
    /// <param name="index">The index.</param>
    [UsedImplicitly]
    protected void OnRenderingClick(string index)
    {
      Assert.ArgumentNotNull((object)index, nameof(index));
      if (this.SelectedIndex >= 0)
        SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", string.Empty);
      this.SelectedIndex = MainUtil.GetInt(index, -1);
      if (this.SelectedIndex >= 0)
        SheerResponse.SetStyle(StringUtil.GetString(this.Controls[this.SelectedIndex]), "background", "#D0EBF6");
      this.UpdateRenderingsCommandsState();
    }

    /// <summary>Personalizes the selected control.</summary>
    /// <param name="args">The arguments.</param>
    [HandleMessage("device:personalize", true)]
    [UsedImplicitly]
    protected void PersonalizeControl(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (this.SelectedIndex < 0)
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
      if (renderings == null)
        return;
      RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
      if (renderingDefinition == null || string.IsNullOrEmpty(renderingDefinition.ItemID) || string.IsNullOrEmpty(renderingDefinition.UniqueId))
        return;
      if (args.IsPostBack)
      {
        if (!args.HasResult)
          return;
        XElement xelement = XElement.Parse(args.Result);
        renderingDefinition.Rules = xelement;
        DeviceEditorForm.SetDefinition(layoutDefinition);
        this.Refresh();
      }
      else
      {
        Item itemFromQueryString = UIUtil.GetItemFromQueryString(Client.ContentDatabase);
        string str = itemFromQueryString != null ? itemFromQueryString.Uri.ToString() : string.Empty;
        SheerResponse.ShowModalDialog(new PersonalizeOptions()
        {
          SessionHandle = DeviceEditorForm.GetSessionHandle(),
          DeviceId = this.DeviceID,
          RenderingUniqueId = renderingDefinition.UniqueId,
          ContextItemUri = str
        }.ToUrlString().ToString(), "980px", "712px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    /// <summary>Removes the specified message.</summary>
    /// <param name="message">The message.</param>
    [HandleMessage("device:remove")]
    [UsedImplicitly]
    protected void Remove(Message message)
    {
      Assert.ArgumentNotNull((object)message, nameof(message));
      int selectedIndex = this.SelectedIndex;
      if (selectedIndex < 0)
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
      if (renderings == null || selectedIndex < 0 || selectedIndex >= renderings.Count)
        return;
      renderings.RemoveAt(selectedIndex);
      if (selectedIndex >= 0)
        --this.SelectedIndex;
      DeviceEditorForm.SetDefinition(layoutDefinition);
      this.Refresh();
    }

    /// <summary>Removes the placeholder.</summary>
    /// <param name="message">The message.</param>
    [HandleMessage("device:removeplaceholder")]
    [UsedImplicitly]
    protected void RemovePlaceholder(Message message)
    {
      Assert.ArgumentNotNull((object)message, nameof(message));
      if (string.IsNullOrEmpty(this.UniqueId))
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      DeviceDefinition device = layoutDefinition.GetDevice(this.DeviceID);
      PlaceholderDefinition placeholder = device.GetPlaceholder(this.UniqueId);
      if (placeholder == null)
        return;
      ArrayList placeholders = device.Placeholders;
      if (placeholders != null)
        placeholders.Remove((object)placeholder);
      DeviceEditorForm.SetDefinition(layoutDefinition);
      this.Refresh();
    }

    /// <summary>Sorts the down.</summary>
    /// <param name="message">The message.</param>
    [HandleMessage("device:sortdown")]
    [UsedImplicitly]
    protected void SortDown(Message message)
    {
      Assert.ArgumentNotNull((object)message, nameof(message));
      if (this.SelectedIndex < 0)
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
      if (renderings == null || this.SelectedIndex >= renderings.Count - 1)
        return;
      RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
      if (renderingDefinition == null)
        return;
      renderings.Remove((object)renderingDefinition);
      renderings.Insert(this.SelectedIndex + 1, (object)renderingDefinition);
      ++this.SelectedIndex;
      DeviceEditorForm.SetDefinition(layoutDefinition);
      this.Refresh();
    }

    /// <summary>Sorts the up.</summary>
    /// <param name="message">The message.</param>
    [HandleMessage("device:sortup")]
    [UsedImplicitly]
    protected void SortUp(Message message)
    {
      Assert.ArgumentNotNull((object)message, nameof(message));
      if (this.SelectedIndex <= 0)
        return;
      LayoutDefinition layoutDefinition = DeviceEditorForm.GetLayoutDefinition();
      ArrayList renderings = layoutDefinition.GetDevice(this.DeviceID).Renderings;
      if (renderings == null)
        return;
      RenderingDefinition renderingDefinition = renderings[this.SelectedIndex] as RenderingDefinition;
      if (renderingDefinition == null)
        return;
      renderings.Remove((object)renderingDefinition);
      renderings.Insert(this.SelectedIndex - 1, (object)renderingDefinition);
      --this.SelectedIndex;
      DeviceEditorForm.SetDefinition(layoutDefinition);
      this.Refresh();
    }

    /// <summary>Gets the layout definition.</summary>
    /// <returns>The layout definition.</returns>
    /// <contract><ensures condition="not null" /></contract>
    private static LayoutDefinition GetLayoutDefinition()
    {
      string sessionString = WebUtil.GetSessionString(DeviceEditorForm.GetSessionHandle());
      Assert.IsNotNull((object)sessionString, "layout definition");
      return LayoutDefinition.Parse(sessionString);
    }

    /// <summary>Gets the session handle.</summary>
    /// <returns>The session handle string.</returns>
    private static string GetSessionHandle()
    {
      return "SC_DEVICEEDITOR";
    }

    /// <summary>Sets the definition.</summary>
    /// <param name="layout">The layout.</param>
    private static void SetDefinition(LayoutDefinition layout)
    {
      Assert.ArgumentNotNull((object)layout, nameof(layout));
      WebUtil.SetSessionValue(DeviceEditorForm.GetSessionHandle(), (object)layout.ToXml());
    }

    /// <summary>Refreshes this instance.</summary>
    private void Refresh()
    {
      this.Renderings.Controls.Clear();
      this.Placeholders.Controls.Clear();
      this.Controls = new ArrayList();
      DeviceDefinition device = DeviceEditorForm.GetLayoutDefinition().GetDevice(this.DeviceID);
      if (device.Renderings == null)
      {
        SheerResponse.SetOuterHtml("Renderings", (System.Web.UI.Control)this.Renderings);
        SheerResponse.SetOuterHtml("Placeholders", (System.Web.UI.Control)this.Placeholders);
        SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
      }
      else
      {
        int selectedIndex = this.SelectedIndex;
        this.RenderRenderings(device, selectedIndex, 0);
        this.RenderPlaceholders(device);
        this.UpdateRenderingsCommandsState();
        this.UpdatePlaceholdersCommandsState();
        SheerResponse.SetOuterHtml("Renderings", (System.Web.UI.Control)this.Renderings);
        SheerResponse.SetOuterHtml("Placeholders", (System.Web.UI.Control)this.Placeholders);
        SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
      }
    }

    /// <summary>Renders the placeholders.</summary>
    /// <param name="deviceDefinition">The device definition.</param>
    private void RenderPlaceholders(DeviceDefinition deviceDefinition)
    {
      Assert.ArgumentNotNull((object)deviceDefinition, nameof(deviceDefinition));
      ArrayList placeholders = deviceDefinition.Placeholders;
      if (placeholders == null)
        return;
      foreach (PlaceholderDefinition placeholderDefinition in placeholders)
      {
        Item obj = (Item)null;
        string metaDataItemId = placeholderDefinition.MetaDataItemId;
        if (!string.IsNullOrEmpty(metaDataItemId))
          obj = Client.ContentDatabase.GetItem(metaDataItemId);
        XmlControl webControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
        Assert.IsNotNull((object)webControl, typeof(XmlControl));
        this.Placeholders.Controls.Add((System.Web.UI.Control)webControl);
        ID id = ID.Parse(placeholderDefinition.UniqueId);
        if (placeholderDefinition.UniqueId == this.UniqueId)
          webControl["Background"] = (object)"#D0EBF6";
        string str = "ph_" + (object)id.ToShortID();
        webControl["ID"] = (object)str;
        webControl["Header"] = (object)placeholderDefinition.Key;
        webControl["Click"] = (object)("OnPlaceholderClick(\"" + placeholderDefinition.UniqueId + "\")");
        webControl["DblClick"] = (object)"device:editplaceholder";
        webControl["Icon"] = obj == null ? (object)"Imaging/24x24/layer_blend.png" : (object)obj.Appearance.Icon;
      }
    }

    /// <summary>Renders the specified device definition.</summary>
    /// <param name="deviceDefinition">The device definition.</param>
    /// <param name="selectedIndex">Index of the selected.</param>
    /// <param name="index">The index.</param>
    private void RenderRenderings(DeviceDefinition deviceDefinition, int selectedIndex, int index)
    {
      Assert.ArgumentNotNull((object)deviceDefinition, nameof(deviceDefinition));
      ArrayList renderings = deviceDefinition.Renderings;
      if (renderings == null)
        return;
      foreach (RenderingDefinition rendering in renderings)
      {
        if (rendering.ItemID != null)
        {
          Item obj = Client.ContentDatabase.GetItem(rendering.ItemID);
          XmlControl webControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
          Assert.IsNotNull((object)webControl, typeof(XmlControl));
          HtmlGenericControl htmlGenericControl1 = new HtmlGenericControl("div");
          htmlGenericControl1.Style.Add("padding", "0");
          htmlGenericControl1.Style.Add("margin", "0");
          htmlGenericControl1.Style.Add("border", "0");
          htmlGenericControl1.Style.Add("position", "relative");
          htmlGenericControl1.Controls.Add((System.Web.UI.Control)webControl);
          string uniqueId = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("R");
          this.Renderings.Controls.Add((System.Web.UI.Control)htmlGenericControl1);
          htmlGenericControl1.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("C");
          webControl["Click"] = (object)("OnRenderingClick(\"" + (object)index + "\")");
          webControl["DblClick"] = (object)"device:edit";
          if (index == selectedIndex)
            webControl["Background"] = (object)"#D0EBF6";
          this.Controls.Add((object)uniqueId);
          if (obj != null)
          {
            webControl["ID"] = (object)uniqueId;
            webControl["Icon"] = (object)obj.Appearance.Icon;
            webControl["Header"] = (object)obj.GetUIDisplayName();
            webControl["Placeholder"] = (object)WebUtil.SafeEncode(rendering.Placeholder);
          }
          else
          {
            webControl["ID"] = (object)uniqueId;
            webControl["Icon"] = (object)"Applications/24x24/forbidden.png";
            webControl["Header"] = (object)"Unknown rendering";
            webControl["Placeholder"] = (object)string.Empty;
          }
          if (rendering.Rules != null && !rendering.Rules.IsEmpty)
          {
            int num = rendering.Rules.Elements((XName)"rule").Count<XElement>();
            if (num > 1)
            {
              HtmlGenericControl htmlGenericControl2 = new HtmlGenericControl("span");
              if (num > 9)
                htmlGenericControl2.Attributes["class"] = "scConditionContainer scLongConditionContainer";
              else
                htmlGenericControl2.Attributes["class"] = "scConditionContainer";
              htmlGenericControl2.InnerText = num.ToString();
              htmlGenericControl1.Controls.Add((System.Web.UI.Control)htmlGenericControl2);
            }
          }
          RenderDeviceEditorRenderingPipeline.Run(rendering, webControl, (System.Web.UI.Control)htmlGenericControl1);
          ++index;
        }
      }
    }

    /// <summary>Updates the state of the commands.</summary>
    private void UpdateRenderingsCommandsState()
    {
      if (this.SelectedIndex < 0)
      {
        this.ChangeButtonsState(true);
      }
      else
      {
        ArrayList renderings = DeviceEditorForm.GetLayoutDefinition().GetDevice(this.DeviceID).Renderings;
        if (renderings == null)
        {
          this.ChangeButtonsState(true);
        }
        else
        {
          RenderingDefinition definition = renderings[this.SelectedIndex] as RenderingDefinition;
          if (definition == null)
          {
            this.ChangeButtonsState(true);
          }
          else
          {
            this.ChangeButtonsState(false);
            this.Personalize.Disabled = !string.IsNullOrEmpty(definition.MultiVariateTest);
            this.Test.Disabled = DeviceEditorForm.HasRenderingRules(definition);
          }
        }
      }
    }

    private void UpdatePlaceholdersCommandsState()
    {
      this.phEdit.Disabled = string.IsNullOrEmpty(this.UniqueId);
      this.phRemove.Disabled = string.IsNullOrEmpty(this.UniqueId);
    }

    /// <summary>Changes the disable of the buttons.</summary>
    /// <param name="disable">if set to <c>true</c> buttons are disabled.</param>
    private void ChangeButtonsState(bool disable)
    {
      this.Personalize.Disabled = disable;
      this.btnEdit.Disabled = disable;
      this.btnChange.Disabled = disable;
      this.btnRemove.Disabled = disable;
      this.MoveUp.Disabled = disable;
      this.MoveDown.Disabled = disable;
      this.Test.Disabled = disable;
    }

    /// <summary>
    /// Determines whether [has rendering rules] [the specified definition].
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <returns><c>true</c> if the definition has a defined rule with action; otherwise, <c>false</c>.</returns>
    private static bool HasRenderingRules(RenderingDefinition definition)
    {
      if (definition.Rules == null)
        return false;
      foreach (XContainer xcontainer in new RulesDefinition(definition.Rules.ToString()).GetRules().Where<XElement>((Func<XElement, bool>)(rule => rule.Attribute((XName)"uid").Value != ItemIDs.Null.ToString())))
      {
        XElement xelement = xcontainer.Descendants((XName)"actions").FirstOrDefault<XElement>();
        if (xelement != null && xelement.Descendants().Any<XElement>())
          return true;
      }
      return false;
    }
  }
}
