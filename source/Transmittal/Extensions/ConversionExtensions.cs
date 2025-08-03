namespace Transmittal.Extensions;
public static class ConversionExtensions
{
     private const double _convertFootToMm = 12d * 25.4d;   
    
    public static double FootToMm(this double length)
    {
        return length * _convertFootToMm;
    }    
}
