namespace Transmittal.Models;
public class DWGLayerMappingModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LayerMapping { get; set; }
    public bool IsCustom { get; set; }
    public bool IsActionItem { get; set; }
}
