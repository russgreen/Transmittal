using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Transmittal.Library.ViewModels;

using Transmittal.Models;
using Transmittal.Requesters;

namespace Transmittal.ViewModels;

internal partial class ParameterSelectorViewModel : BaseViewModel
{
    private readonly IParameterGuidRequester _callingViewModel;

    [ObservableProperty]
    private ObservableCollection<ParameterDataModel> _parameters;
    [ObservableProperty]
    private ParameterDataModel _selectedParameter;
    [ObservableProperty]
    private string _targetVariable;

    public ParameterSelectorViewModel(IParameterGuidRequester caller, string targetVariable)
    {
        _callingViewModel = caller;
        _targetVariable = targetVariable;
    }

    private List<ParameterDataModel> LoadSharedParameters(BuiltInCategory category)
    {
        List<ParameterDataModel> parameterDataModels = new List<ParameterDataModel>();
        List<Element> sharedParams = new List<Element>();

        FilteredElementCollector collector
            = new FilteredElementCollector(App.RevitDocument)
            .WhereElementIsNotElementType();

        // Filter elements for shared parameters only
        collector.OfClass(typeof(SharedParameterElement));
            //.OfCategory(category);

        foreach (Element e in collector)
        {
            SharedParameterElement param = e as SharedParameterElement;
            Definition def = param.GetDefinition();

            ParameterDataModel parameterDataModel = new()
            {
                ID = e.Id,
                Name = def.Name,
                Guid = param.GuidValue.ToString()
            };

            parameterDataModels.Add(parameterDataModel);
        }

        return parameterDataModels;
    }

    private List<ParameterDataModel> LoadSharedParameters2(BuiltInCategory category)
    {
        List<ParameterDataModel> parameterDataModels = new List<ParameterDataModel>();

        FilteredElementCollector collector = new FilteredElementCollector(App.RevitDocument)
            .WhereElementIsNotElementType();

        // Filter elements for shared parameters only
        collector.OfClass(typeof(SharedParameterElement));

        var map = App.RevitDocument.ParameterBindings;

        foreach (Element e in collector)
        {
            SharedParameterElement param = e as SharedParameterElement;
            Definition def = param.GetDefinition();

            ParameterDataModel parameterDataModel = new()
            {
                ID = e.Id,
                Name = def.Name,
                Guid = param.GuidValue.ToString(),
                Binding = map.get_Item(def) as ElementBinding,
            };

            if (parameterDataModel.Binding != null && parameterDataModel.Binding.Categories.Contains(Category.GetCategory(App.RevitDocument, category)))
            {
                parameterDataModels.Add(parameterDataModel);
            }
        }

        return parameterDataModels;
    }


    [RelayCommand]
    private void PopulateParameterList(BuiltInCategory category)
    {
        Parameters = new ObservableCollection<ParameterDataModel>(LoadSharedParameters2(category));
    }

    [RelayCommand]
    private void SendParameter()
    {
        _callingViewModel.ParameterComplete(TargetVariable, SelectedParameter.Guid);
        this.OnClosingRequest();
    }


}
