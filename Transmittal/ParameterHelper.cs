using Autodesk.Revit.DB;
using System.Collections;

namespace Transmittal;
/// <summary>
/// Helper class used to work with parameters
/// </summary>
/// <remarks></remarks>
public class ParameterHelper
{
    private readonly Parameter _parameter;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="p"></param>
    /// <remarks></remarks>
    public ParameterHelper(Parameter p)
    {

        // Widen Scope
        _parameter = p;
    }

    /// <summary>
    /// The parameter reference
    /// </summary>
    /// <value></value>
    /// <returns>Parameter Object</returns>
    /// <remarks></remarks>
    public Parameter ParameterObject
    {
        get
        {
            return _parameter;
        }
    }

    /// <summary>
    /// Returns value of the parameter
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public string Value
    {
        get
        {
            try
            {
                string v = GetValue(false);
                if (!string.IsNullOrEmpty(v))
                {
                    return v;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return null;
            }
        }

        set
        {
            try
            {
                SetValue(value, false);
            }
            catch
            {
            }
        }
    }



    /// <summary>
    /// Returns value of the parameter
    /// as a string
    /// </summary>
    /// <value></value>
    /// <returns></returns>
    /// <remarks></remarks>
    public string ValueString
    {
        get
        {
            try
            {
                string v = GetValue(true);
                if (!string.IsNullOrEmpty(v))
                {
                    return v;
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return null;
            }
        }

        set
        {
            try
            {
                SetValue(value, true);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// Set a value to a parameter
    /// </summary>
    /// <param name="value">
    /// Value does not have to be a string
    /// </param>
    /// <param name="asString">As string?</param>
    /// <remarks></remarks>
    private void SetValue(object value, bool asString)
    {

        // Cannot edit readonly
        if (_parameter.IsReadOnly)
            return;
        try
        {
            // Storage Type
            switch (_parameter.StorageType)
            {
                case StorageType.Double:
                    {
                        if (asString == true)
                        {
                            _parameter.SetValueString(value as string);
                        }
                        else
                        {
                            _parameter.Set((ElementId)value);
                        }

                        break;
                    }

                case StorageType.ElementId:
                    {
                        // Dim m_eid As ElementId
                        // m_eid = DirectCast((value), ElementId)
                        // _parameter.Set(m_eid)
                        if (value.GetType().Equals(typeof(ElementId)))
                        {
                            _parameter.Set(value as ElementId);
                        }
                        else if (value.GetType().Equals(typeof(string)))
                        {
#if REVIT2024_OR_GREATER
                            _parameter.Set(new ElementId(Int64.Parse(value as string)));
#else
                            _parameter.Set(new ElementId(int.Parse(value as string)));
#endif
                        }
                        else
                        {
#if REVIT2024_OR_GREATER
                            _parameter.Set(new ElementId(Convert.ToInt64(value)));
#else
                            _parameter.Set(new ElementId(Convert.ToInt32(value)));
#endif
                        }

                        break;
                    }

                case StorageType.Integer:
                    {
                        // _parameter.SetValueString _
                        // (TryCast(value, String))

                        if (value.GetType().Equals(typeof(string)))
                        {
                            _parameter.Set(int.Parse(value as string));
                        }
                        else
                        {
                            _parameter.Set(Convert.ToInt32(value));
                        }

                        break;
                    }

                case StorageType.None:
                    {
                        _parameter.SetValueString(value as string);
                        break;
                    }

                case StorageType.String:
                    {
                        _parameter.Set(value as string);
                        break;
                    }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Get the value of a parameter
    /// </summary>
    /// <param name="asString">Return as String?</param>
    /// <returns>String representing the value</returns>
    /// <remarks></remarks>
    private string GetValue(bool asString)
    {

        // Return the Value
        switch (_parameter.StorageType)
        {
            case StorageType.Double:
                {
                    if (asString == true)
                    {
                        return _parameter.AsValueString();
                    }
                    else
                    {
                        return _parameter.AsDouble().ToString();
                    }
                }

            case StorageType.ElementId:
                {
                    if (asString == true)
                    {
                        // Get the Element's Name
#if REVIT2024_OR_GREATER
                        var m_eid = new ElementId(_parameter.AsElementId().Value);
#else
                        var m_eid = new ElementId(_parameter.AsElementId().IntegerValue);
#endif
                        Element m_obj;
                        m_obj = _parameter.Element.Document.GetElement(m_eid);
                        return m_obj.Name;
                    }
                    else
                    {
                        return _parameter.AsElementId().ToString();
                    }
                }

            case StorageType.Integer:
                {
                    return _parameter.AsInteger().ToString();
                }

            case StorageType.None:
                {
                    return _parameter.AsValueString();
                }

            case StorageType.String:
                {
                    return _parameter.AsString();
                }

            default:
                {
                    return "";
                }
        }
    }

    private bool IsSharedParameter()
    {
        try
        {
            _parameter.GUID.ToString();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

#if PREFORGETYPEID
    public virtual bool AddParameter(Document doc, Element elem, string bindType, ParameterType pDataType, string paramName, string defPath, string groupName, bool bHandelTransaction = true, bool bSuppressError = false)
    {
        bool flag = false;
        Transaction transaction = null;
        if (bHandelTransaction)
        {
            transaction = new Transaction(doc, "Add a parameter: " + paramName);
        }

        try
        {
            if (bHandelTransaction)
            {
                transaction.Start();
            }

            elem = GetTypeParameters(elem, bindType);
            CategorySet catSet = elem.Document.Application.Create.NewCategorySet();
            Category category = elem.Category;
            catSet.Insert(category);
            string definitionFilepath = GetDefinitionFilepath(elem.Document.Application);
            DefinitionFile defFile = SetDefinitionFilepath(elem.Document.Application, defPath);
            DefinitionGroup definitionGroup = defFile.Groups.get_Item(groupName);

            var op = new ExternalDefinitionCreationOptions(paramName, pDataType);

            var def = GetDefinition(defFile, paramName) ?? definitionGroup.Definitions.Create(op);
            if (BindParameter(elem, bindType, catSet, def))
            {
                if (bHandelTransaction)
                {
                    transaction.Commit();
                }

                flag = true;
            }
            else if (bHandelTransaction)
            {
                transaction.RollBack();
            }

            SetDefinitionFilepath(elem.Document.Application, definitionFilepath);
        }
        catch (Exception)
        {
            if (bHandelTransaction)
            {
                transaction.RollBack();
            }
        }

        return flag;
    }
#else
    public virtual bool AddParameter(Document doc, Element elem, string bindType, ForgeTypeId pDataType, string paramName, string defPath, string groupName, bool bHandelTransaction = true, bool bSuppressError = false)
    {
        bool flag = false;
        Transaction transaction = null;
        if (bHandelTransaction)
        {
            transaction = new Transaction(doc, "Add a parameter: " + paramName);
        }

        try
        {
            if (bHandelTransaction)
            {
                transaction.Start();
            }

            elem = GetTypeParameters(elem, bindType);
            CategorySet catSet = elem.Document.Application.Create.NewCategorySet();
            Category category = elem.Category;
            catSet.Insert(category);
            string definitionFilepath = GetDefinitionFilepath(elem.Document.Application);
            DefinitionFile defFile = SetDefinitionFilepath(elem.Document.Application, defPath);
            DefinitionGroup definitionGroup = defFile.Groups.get_Item(groupName);

            var op = new ExternalDefinitionCreationOptions(paramName, pDataType);

            var def = GetDefinition(defFile, paramName) ?? definitionGroup.Definitions.Create(op);
            if (BindParameter(elem, bindType, catSet, def))
            {
                if (bHandelTransaction)
                {
                    transaction.Commit();
                }

                flag = true;
            }
            else if (bHandelTransaction)
            {
                transaction.RollBack();
            }

            SetDefinitionFilepath(elem.Document.Application, definitionFilepath);
        }
        catch (Exception)
        {
            if (bHandelTransaction)
            {
                transaction.RollBack();
            }
        }

        return flag;
    }
#endif

    private Element GetTypeParameters(Element elem, string bindType)
    {
        if (!(bindType == "Type"))
        {
            return elem;
        }

        var typeId = elem.GetTypeId();
        return elem.Document.GetElement(typeId) as ElementType;
    }

    private string GetDefinitionFilepath(Autodesk.Revit.ApplicationServices.Application app)
    {
        return app.SharedParametersFilename;
    }

    private DefinitionFile SetDefinitionFilepath(Autodesk.Revit.ApplicationServices.Application app, string filePath)
    {
        app.SharedParametersFilename = filePath;
        return app.OpenSharedParameterFile();
    }

    private Definition GetDefinition(DefinitionFile defFile, string paramName)
    {
        Definition definition = null;
        IEnumerator enumerator = defFile.Groups.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                definition = ((DefinitionGroup)enumerator.Current).Definitions.get_Item(paramName);
                if (definition is object)
                {
                    return definition;
                }
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        return definition;
    }

    private bool BindParameter(Element elem, string bindType, CategorySet catSet, Definition def)
    {
        try
        {
            var parameterBindings = elem.Document.ParameterBindings;
            var binding1 = parameterBindings.get_Item(def);
            if (binding1 is object)
            {
                Autodesk.Revit.DB.Binding binding2;
                if (bindType == "Type")
                {
                    binding2 = binding1 as TypeBinding;
                    var enumerator = catSet.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Category category = (Category)enumerator.Current;
                            ((ElementBinding)binding2).Categories.Insert(category);
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
                else
                {
                    binding2 = binding1 as InstanceBinding;
                    var enumerator = catSet.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Category category = (Category)enumerator.Current;
                            ((ElementBinding)binding2).Categories.Insert(category);
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
#if REVIT2024_OR_GREATER
                parameterBindings.ReInsert(def, binding2, GroupTypeId.IdentityData);
#else
                parameterBindings.ReInsert(def, binding2, BuiltInParameterGroup.PG_IDENTITY_DATA);
#endif
            }
            else
            {
                var binding2 = !(bindType == "Type") ? elem.Document.Application.Create.NewInstanceBinding(catSet) : (Autodesk.Revit.DB.Binding)elem.Document.Application.Create.NewTypeBinding(catSet);
#if REVIT2024_OR_GREATER                
                parameterBindings.Insert(def, binding2, GroupTypeId.IdentityData);
#else
                parameterBindings.Insert(def, binding2, BuiltInParameterGroup.PG_IDENTITY_DATA);
#endif
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}

