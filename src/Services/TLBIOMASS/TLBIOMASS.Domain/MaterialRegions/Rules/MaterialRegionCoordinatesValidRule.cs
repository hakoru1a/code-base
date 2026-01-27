using Contracts.Domain.Interface;

namespace TLBIOMASS.Domain.MaterialRegions.Rules;

public class MaterialRegionCoordinatesValidRule : IBusinessRule
{
    private readonly double _latitude;
    private readonly double _longitude;

    public MaterialRegionCoordinatesValidRule(double latitude, double longitude)
    {
        _latitude = latitude;
        _longitude = longitude;
    }

    public bool IsBroken()
    {
        return _latitude < -90 || _latitude > 90 || _longitude < -180 || _longitude > 180;
    }

    public string Message => "Invalid coordinates. Latitude must be between -90 and 90, Longitude between -180 and 180.";
    public string Code => "MaterialRegion.CoordinatesInvalid";
}
