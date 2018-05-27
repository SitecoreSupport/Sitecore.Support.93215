// Sitecore.Shell.Applications.Layouts.DeviceEditor.RenderingParameters
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.SecurityModel;
using Sitecore.Shell;
using Sitecore.Shell.Applications.WebEdit;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Sitecore.Support.Shell.Applications.Layouts.DeviceEditor
{
  /// <summary>
  /// Defines the rendering parameters options class.
  /// </summary>
  public class RenderingParameters
  {
    /// <summary>
    /// The current pipeline arguments.
    /// </summary>
    private ClientPipelineArgs args;

    /// <summary>
    /// The selected device ID.
    /// </summary>
    private string deviceId;

    /// <summary>
    /// The name of the handle.
    /// </summary>
    private string handleName;

    /// <summary>
    /// The current layout definition.
    /// </summary>
    private LayoutDefinition layoutDefinition;

    /// <summary>
    /// Gets or sets the args.
    /// </summary>
    /// <value>
    /// The args.
    /// </value>
    public ClientPipelineArgs Args
    {
      get
      {
        return this.args;
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        this.args = value;
      }
    }

    /// <summary>
    /// Gets or sets the  item.
    /// </summary>
    /// <value>The item.</value>
    public Item Item
    {
      private get;
      set;
    }

    /// <summary>
    /// Gets or sets the device ID.
    /// </summary>
    /// <value>
    /// The device ID.
    /// </value>
    public string DeviceId
    {
      get
      {
        return this.deviceId ?? string.Empty;
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        this.deviceId = value;
      }
    }

    /// <summary>
    /// Gets or sets the name of the handle.
    /// </summary>
    /// <value>
    /// The name of the handle.
    /// </value>
    public string HandleName
    {
      get
      {
        return this.handleName ?? "SC_DEVICEEDITOR";
      }
      set
      {
        Assert.ArgumentNotNull(value, "value");
        this.handleName = value;
      }
    }

    /// <summary>
    /// Gets or sets the index of the selected.
    /// </summary>
    /// <value>
    /// The index of the selected.
    /// </value>
    public int SelectedIndex
    {
      get;
      set;
    }

    /// <summary>
    /// Shows this instance.
    /// </summary>
    /// <returns>
    /// The boolean.
    /// </returns>
    public bool Show()
    {
      if (this.Args.IsPostBack)
      {
        if (this.Args.HasResult)
        {
          this.Save();
        }
        return true;
      }
      if (this.SelectedIndex < 0)
      {
        return true;
      }
      RenderingDefinition renderingDefinition = this.GetRenderingDefinition();
      if (renderingDefinition == null)
      {
        return true;
      }
      string text = null;
      if (!string.IsNullOrEmpty(renderingDefinition.ItemID))
      {
        Item item = Client.ContentDatabase.GetItem(renderingDefinition.ItemID, this.Item.Language);
        if (item != null)
        {
          LinkField linkField = item.Fields["Customize Page"];
          Assert.IsNotNull(linkField, "linkField");
          if (!string.IsNullOrEmpty(linkField.Url))
          {
            text = linkField.Url;
          }
        }
      }
      Dictionary<string, string> parameters = RenderingParameters.GetParameters(renderingDefinition);
      RenderingParametersFieldEditorOptions renderingParametersFieldEditorOptions = new RenderingParametersFieldEditorOptions(this.GetFields(renderingDefinition, parameters))
      {
        DialogTitle = "Control Properties",
        HandleName = this.HandleName,
        PreserveSections = true
      };
      this.SetCustomParameters(renderingDefinition, renderingParametersFieldEditorOptions);
      UrlString urlString;
      if (!string.IsNullOrEmpty(text))
      {
        urlString = new UrlString(text);
        renderingParametersFieldEditorOptions.ToUrlHandle().Add(urlString, this.HandleName);
      }
      else
      {
        urlString = renderingParametersFieldEditorOptions.ToUrlString();
      }
      SheerResponse.ShowModalDialog(new ModalDialogOptions(urlString.ToString())
      {
        Width = "720",
        Height = "480",
        Response = true,
        Header = renderingParametersFieldEditorOptions.DialogTitle
      });
      this.args.WaitForPostBack();
      return false;
    }

    /// <summary>
    /// Sets the custom parameters.
    /// </summary>
    /// <param name="renderingDefinition">The rendering definition.</param>
    /// <param name="options">The options.</param>
    private void SetCustomParameters(RenderingDefinition renderingDefinition, RenderingParametersFieldEditorOptions options)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Assert.ArgumentNotNull(options, "options");
      Item item = (renderingDefinition.ItemID != null) ? Client.ContentDatabase.GetItem(renderingDefinition.ItemID) : null;
      if (item != null)
      {
        options.Parameters["rendering"] = item.Uri.ToString();
      }
      if (this.Item != null)
      {
        options.Parameters["contentitem"] = this.Item.Uri.ToString();
      }
      if (WebEditUtil.IsRenderingPersonalized(renderingDefinition))
      {
        options.Parameters["warningtext"] = "There are personalization conditions defined for this control. Changing control properties may effect them.";
      }
      if (!string.IsNullOrEmpty(renderingDefinition.MultiVariateTest))
      {
        options.Parameters["warningtext"] = "There is a multivariate test set up for this control. Changing control properties may effect the test.";
      }
    }

    /// <summary>
    /// Gets the additional parameters.
    /// </summary>
    /// <param name="fieldDescriptors">
    /// The field descriptors.
    /// </param>
    /// <param name="standardValues">
    /// The standard values.
    /// </param>
    /// <param name="additionalParameters">
    /// The addtional parameters.
    /// </param>
    private static void GetAdditionalParameters(List<FieldDescriptor> fieldDescriptors, Item standardValues, Dictionary<string, string> additionalParameters)
    {
      Assert.ArgumentNotNull(fieldDescriptors, "fieldDescriptors");
      Assert.ArgumentNotNull(standardValues, "standardValues");
      Assert.ArgumentNotNull(additionalParameters, "additionalParameters");
      string fieldName = "Additional Parameters";
      if (standardValues.Fields[fieldName] == null && !additionalParameters.Any())
      {
        return;
      }
      UrlString urlString = new UrlString();
      foreach (string key in additionalParameters.Keys)
      {
        urlString[key] = HttpUtility.UrlDecode(additionalParameters[key]);
      }
      fieldDescriptors.Add(new FieldDescriptor(standardValues, fieldName)
      {
        Value = urlString.ToString()
      });
    }

    /// <summary>
    /// Gets the caching.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <returns>
    /// The caching.
    /// </returns>
    private static string GetCaching(RenderingDefinition renderingDefinition)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      return ((renderingDefinition.Cachable == "1") ? "1" : "0") + "|" + ((renderingDefinition.ClearOnIndexUpdate == "1") ? "1" : "0") + "|" + ((renderingDefinition.VaryByData == "1") ? "1" : "0") + "|" + ((renderingDefinition.VaryByDevice == "1") ? "1" : "0") + "|" + ((renderingDefinition.VaryByLogin == "1") ? "1" : "0") + "|" + ((renderingDefinition.VaryByParameters == "1") ? "1" : "0") + "|" + ((renderingDefinition.VaryByQueryString == "1") ? "1" : "0") + "|" + ((renderingDefinition.VaryByUser == "1") ? "1" : "0");
    }

    /// <summary>
    /// Gets the fields.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <param name="parameters">
    /// The parameters.
    /// </param>
    /// <returns>
    /// The fields.
    /// </returns>
    private List<FieldDescriptor> GetFields(RenderingDefinition renderingDefinition, Dictionary<string, string> parameters)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Assert.ArgumentNotNull(parameters, "parameters");
      List<FieldDescriptor> list = new List<FieldDescriptor>();
      Item standardValuesItem = default(Item);
      using (new SecurityDisabler())
      {
        standardValuesItem = this.GetStandardValuesItem(renderingDefinition);
      }
      if (standardValuesItem == null)
      {
        return list;
      }
      FieldCollection fields = standardValuesItem.Fields;
      fields.ReadAll();
      fields.Sort();
      Dictionary<string, string> dictionary = new Dictionary<string, string>(parameters);
      foreach (Field item2 in fields)
      {
        if (!(item2.Name == "Additional Parameters") && (!(item2.Name == "Personalization") || UserOptions.View.ShowPersonalizationSection) && (!(item2.Name == "Tests") || UserOptions.View.ShowTestLabSection) && RenderingItem.IsRenderingParameterField(item2))
        {
          string value = RenderingParameters.GetValue(item2.Name, renderingDefinition, parameters);
          FieldDescriptor item = new FieldDescriptor(standardValuesItem, item2.Name)
          {
            Value = (value ?? item2.Value),
            ContainsStandardValue = ((byte)((value == null) ? 1 : 0) != 0)
          };
          list.Add(item);
          dictionary.Remove(item2.Name);
        }
      }
      RenderingParameters.GetAdditionalParameters(list, standardValuesItem, dictionary);
      return list;
    }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <returns>
    /// The parameters.
    /// </returns>
    private static Dictionary<string, string> GetParameters(RenderingDefinition renderingDefinition)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      NameValueCollection nameValueCollection = WebUtil.ParseUrlParameters(renderingDefinition.Parameters ?? string.Empty);
      foreach (string key in nameValueCollection.Keys)
      {
        if (!string.IsNullOrEmpty(key))
        {
          dictionary[key] = nameValueCollection[key];
        }
      }
      return dictionary;
    }

    /// <summary>
    /// Gets the rendering item.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <returns>
    /// The rendering item.
    /// </returns>
    private Item GetRenderingItem(RenderingDefinition renderingDefinition)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      string itemID = renderingDefinition.ItemID;
      if (!string.IsNullOrEmpty(itemID))
      {
        return Client.ContentDatabase.GetItem(itemID, this.Item.Language);
      }
      return null;
    }

    /// <summary>
    /// Gets the standard values item.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <returns>
    /// The standard values item.
    /// </returns>
    private Item GetStandardValuesItem(RenderingDefinition renderingDefinition)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Item renderingItem = this.GetRenderingItem(renderingDefinition);
      if (renderingItem == null)
      {
        return null;
      }
      return RenderingItem.GetStandardValuesItemFromParametersTemplate(renderingItem);
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="fieldName">
    /// The name.
    /// </param>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <param name="parameters">
    /// The parameters.
    /// </param>
    /// <returns>
    /// The value.
    /// </returns>
    private static string GetValue(string fieldName, RenderingDefinition renderingDefinition, Dictionary<string, string> parameters)
    {
      Assert.ArgumentNotNull(fieldName, "fieldName");
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Assert.ArgumentNotNull(parameters, "parameters");
      switch (fieldName.ToLowerInvariant())
      {
        case "placeholder":
          return renderingDefinition.Placeholder ?? string.Empty;
        case "data source":
          return renderingDefinition.Datasource ?? string.Empty;
        case "caching":
          return RenderingParameters.GetCaching(renderingDefinition);
        case "personalization":
          return renderingDefinition.Conditions ?? string.Empty;
        case "tests":
          return renderingDefinition.MultiVariateTest ?? string.Empty;
        default:
          {
            string result = default(string);
            parameters.TryGetValue(fieldName, out result);
            return result;
          }
      }
    }

    /// <summary>
    /// Sets the caching.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    private static void SetCaching(RenderingDefinition renderingDefinition, string value)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Assert.ArgumentNotNull(value, "value");
      if (string.IsNullOrEmpty(value))
      {
        value = "0|0|0|0|0|0|0|0";
      }
      string[] array = value.Split('|');
      Assert.IsTrue(array.Length == 8, "Invalid caching value format");
      renderingDefinition.Cachable = ((array[0] == "1") ? "1" : ((renderingDefinition.Cachable != null) ? "0" : null));
      renderingDefinition.ClearOnIndexUpdate = ((array[1] == "1") ? "1" : ((renderingDefinition.ClearOnIndexUpdate != null) ? "0" : null));
      renderingDefinition.VaryByData = ((array[2] == "1") ? "1" : ((renderingDefinition.VaryByData != null) ? "0" : null));
      renderingDefinition.VaryByDevice = ((array[3] == "1") ? "1" : ((renderingDefinition.VaryByDevice != null) ? "0" : null));
      renderingDefinition.VaryByLogin = ((array[4] == "1") ? "1" : ((renderingDefinition.VaryByLogin != null) ? "0" : null));
      renderingDefinition.VaryByParameters = ((array[5] == "1") ? "1" : ((renderingDefinition.VaryByParameters != null) ? "0" : null));
      renderingDefinition.VaryByQueryString = ((array[6] == "1") ? "1" : ((renderingDefinition.VaryByQueryString != null) ? "0" : null));
      renderingDefinition.VaryByUser = ((array[7] == "1") ? "1" : ((renderingDefinition.VaryByUser != null) ? "0" : null));
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="renderingDefinition">
    /// The rendering definition.
    /// </param>
    /// <param name="parameters">
    /// The parameters.
    /// </param>
    /// <param name="fieldName">
    /// Name of the field.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    private static void SetValue(RenderingDefinition renderingDefinition, UrlString parameters, string fieldName, string value, bool containsStandardValue)
    {
      Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");
      Assert.ArgumentNotNull(fieldName, "fieldName");
      Assert.ArgumentNotNull(value, "value");
      Assert.ArgumentNotNull(parameters, "parameters");
      switch (fieldName.ToLowerInvariant())
      {
        case "placeholder":
          renderingDefinition.Placeholder = value;
          break;
        case "data source":
          renderingDefinition.Datasource = value;
          break;
        case "caching":
          RenderingParameters.SetCaching(renderingDefinition, value);
          break;
        case "personalization":
          renderingDefinition.Conditions = value;
          break;
        case "tests":
          {
            if (string.IsNullOrEmpty(value))
            {
              renderingDefinition.MultiVariateTest = string.Empty;
            }
            Item item = Client.ContentDatabase.GetItem(value);
            if (item != null)
            {
              renderingDefinition.MultiVariateTest = item.ID.ToString();
            }
            else
            {
              renderingDefinition.MultiVariateTest = value;
            }
            break;
          }
        case "additional parameters":
          {
            UrlString url = new UrlString(value);
            parameters.Append(url);
            break;
          }
        default:
          if (!containsStandardValue)
          {
            parameters[fieldName] = value;
          }
          break;
      }
    }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    /// <returns>
    /// The definition.
    /// </returns>
    private LayoutDefinition GetLayoutDefinition()
    {
      if (this.layoutDefinition == null)
      {
        string sessionString = WebUtil.GetSessionString(this.HandleName);
        Assert.IsNotNull(sessionString, "sessionValue");
        this.layoutDefinition = LayoutDefinition.Parse(sessionString);
      }
      return this.layoutDefinition;
    }

    /// <summary>
    /// Gets the rendering definition.
    /// </summary>
    /// <returns>
    /// The rendering definition.
    /// </returns>
    private RenderingDefinition GetRenderingDefinition()
    {
      ArrayList renderings = this.GetLayoutDefinition().GetDevice(this.DeviceId).Renderings;
      if (renderings == null)
      {
        return null;
      }
      return renderings[MainUtil.GetInt(this.SelectedIndex, 0)] as RenderingDefinition;
    }

    private static string GetStandardValueOfField(FieldDescriptor descriptor)
    {
      Item item = Database.GetItem(descriptor.ItemUri);
      Field field = item.Fields[descriptor.FieldID];
      return field.Value;
    }
    /// <summary>
    /// Sets the values.
    /// </summary>
    private void Save()
    {
      RenderingDefinition renderingDefinition = this.GetRenderingDefinition();
      if (renderingDefinition != null)
      {
        Item standardValuesItem = default(Item);
        using (new SecurityDisabler())
        {
          standardValuesItem = this.GetStandardValuesItem(renderingDefinition);
        }
        if (standardValuesItem != null)
        {
          UrlString urlString = new UrlString();
          foreach (FieldDescriptor field in RenderingParametersFieldEditorOptions.Parse(this.args.Result).Fields)
          {
            string standardValueOfField = RenderingParameters.GetStandardValueOfField(field);
            bool containsStandardValue = field.ContainsStandardValue && field.Value.Equals(standardValueOfField);
            RenderingParameters.SetValue(renderingDefinition, urlString, standardValuesItem.Fields[field.FieldID].Name, field.Value, containsStandardValue);
          }
          renderingDefinition.Parameters = urlString.ToString();
          LayoutDefinition layoutDefinition = this.GetLayoutDefinition();
          this.SetLayoutDefinition(layoutDefinition);
        }
      }
    }

    /// <summary>
    /// Sets the definition.
    /// </summary>
    /// <param name="layout">
    /// The layout.
    /// </param>
    private void SetLayoutDefinition(LayoutDefinition layout)
    {
      Assert.ArgumentNotNull(layout, "layout");
      WebUtil.SetSessionValue(this.HandleName, layout.ToXml());
    }
  }
}