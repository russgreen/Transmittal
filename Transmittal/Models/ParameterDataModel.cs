using Autodesk.Revit.DB;

namespace Transmittal.Models;
internal class ParameterDataModel
{
    public ElementId ID { get; set; }
    public string Name { get; set; }
    public string Guid { get; set; }
    public ElementBinding Binding { get; set; }
    public Definition Definition { get; set; }
}
