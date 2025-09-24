using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using DisasterManagementSystem_Services.Models;
using AppLocation = DisasterManagementSystem_Data.Models.Location;
using DisasterManagementSystem_Services.Models.LocationDtos;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;

public class LocationService : IlocationService
{
    private readonly IlocationRepository _repository;
    private readonly INominatimGeocodingService _geocodingService;
    private readonly GeoJsonWriter _geoJsonWriter;

    public LocationService(IlocationRepository repository, INominatimGeocodingService geocodingService)
    {
        _repository = repository;
        _geocodingService = geocodingService;
        _geoJsonWriter = new GeoJsonWriter();
    }

    private bool IsFinite(double value) => !(double.IsNaN(value) || double.IsInfinity(value));

    public async Task<Result<LocationDto>> GetByIdAsync(int id)
    {
        var loc = await _repository.GetByIdAsync(id);
        if (loc == null)
            return Result<LocationDto>.NotFoundError("Location not found.");

        var centroid = loc.Geography?.Centroid;
        double? lat = null, lon = null;

        if (centroid != null && IsFinite(centroid.X) && IsFinite(centroid.Y))
        {
            lat = centroid.Y;
            lon = centroid.X;
        }
        else
        {
            lat = null;
            lon = null;
        }

        var dto = new LocationDto
        {
            Id = loc.Id,
            Name = loc.Name,
            GeoJson = loc.Geography != null ? _geoJsonWriter.Write(FixPolygonOrientation(loc.Geography)) : null,
            Address = loc.Address,
            Country = loc.Country,
            Region = loc.Region,
            Latitude = SanitizeNullableDouble(lat),
            Longitude = SanitizeNullableDouble(lon)
        };

        return Result<LocationDto>.Success(dto);
    }

    public async Task<Result<IEnumerable<LocationDto>>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();

        var result = all.Select(loc =>
        {
            var centroid = loc.Geography?.Centroid;
            double? lat = null, lon = null;

            if (centroid != null && IsFinite(centroid.X) && IsFinite(centroid.Y))
            {
                lat = centroid.Y;
                lon = centroid.X;
            }

            return new LocationDto
            {
                Id = loc.Id,
                Name = loc.Name,
                GeoJson = loc.Geography != null ? _geoJsonWriter.Write(loc.Geography) : null,
                Address = loc.Address,
                Country = loc.Country,
                Region = loc.Region,
                Latitude = SanitizeNullableDouble(lat),
                Longitude = SanitizeNullableDouble(lon)
            };
        });

        return Result<IEnumerable<LocationDto>>.Success(result);
    }

    public async Task<Result<LocationDto>> AddAsync(LocationCreateDto dto)
    {
        Geometry? geom;
        try
        {
            var reader = new GeoJsonReader();

            // Try to read as a raw Geometry first
            geom = reader.Read<Geometry>(dto.GeoJson);

            // If null, try to read as FeatureCollection
            if (geom == null)
            {
                var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(dto.GeoJson);
                geom = featureCollection?.FirstOrDefault()?.Geometry;
            }

            if (geom == null)
                return Result<LocationDto>.ValidationError("Invalid GeoJSON: No valid geometry found.");
        }
        catch (Exception ex)
        {
            return Result<LocationDto>.ValidationError($"Invalid GeoJSON: {ex.Message}");
        }

        // Ensure polygon orientation is fixed
        geom = FixPolygonOrientation(geom);

        var centroid = geom.Centroid;
        if (!IsFinite(centroid.X) || !IsFinite(centroid.Y))
            return Result<LocationDto>.ValidationError("Geometry centroid has invalid coordinate values.");

        // Reverse geocode from centroid
        var geoInfo = await _geocodingService.ReverseGeocodeAsync(centroid.Y, centroid.X);

        var location = new AppLocation
        {
            Name = dto.Name,
            Geography = geom,
            Address = geoInfo?.Address,
            Country = geoInfo?.Country,
            Region = geoInfo?.Region
        };

        await _repository.AddAsync(location);
        await _repository.SaveChangesAsync();

        // Return a sanitized DTO instead of raw entity
        var createdDto = new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            GeoJson = location.Geography != null
                ? _geoJsonWriter.Write(FixPolygonOrientation(location.Geography))
                : null,
            Address = location.Address,
            Country = location.Country,
            Region = location.Region,
            Latitude = SanitizeNullableDouble(location.Geography?.Centroid?.Y),
            Longitude = SanitizeNullableDouble(location.Geography?.Centroid?.X)
        };

        return Result<LocationDto>.Success(createdDto, "Location added successfully.");
    }

    public async Task<Result<LocationDto>> PureAddAsync(LocationCreateDto dto)
    {
        Geometry? geom;
        try
        {
            var reader = new GeoJsonReader();

            // Try to parse as raw Geometry
            geom = reader.Read<Geometry>(dto.GeoJson);

            // If null, try as FeatureCollection
            if (geom == null)
            {
                var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(dto.GeoJson);
                geom = featureCollection?.FirstOrDefault()?.Geometry;
            }

            if (geom == null)
                return Result<LocationDto>.ValidationError("Invalid GeoJSON: No valid geometry found.");
        }
        catch (Exception ex)
        {
            return Result<LocationDto>.ValidationError($"Invalid GeoJSON: {ex.Message}");
        }

        // Ensure polygon orientation is consistent
        geom = FixPolygonOrientation(geom);

        var centroid = geom.Centroid;
        if (!IsFinite(centroid.X) || !IsFinite(centroid.Y))
            return Result<LocationDto>.ValidationError("Geometry centroid has invalid coordinate values.");

        // Use data directly from dto instead of reverse geocoding
        var location = new AppLocation
        {
            Name = dto.Name,
            Geography = geom,
            Address = dto.Address,   // ✅ Use provided Address
            Country = dto.Country,   // ✅ Use provided Country
            Region = dto.Region      // ✅ Use provided Region
        };

        await _repository.AddAsync(location);
        await _repository.SaveChangesAsync();

        var createdDto = new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            GeoJson = location.Geography != null
                ? _geoJsonWriter.Write(FixPolygonOrientation(location.Geography))
                : null,
            Address = location.Address,
            Country = location.Country,
            Region = location.Region,
        };

        return Result<LocationDto>.Success(createdDto, "Location added successfully (pure add).");
    }

    public async Task<Result<AppLocation>> UpdateAsync(LocationUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null)
            return Result<AppLocation>.NotFoundError("Location not found.");

        if (!string.IsNullOrWhiteSpace(dto.GeoJson))
        {
            Geometry geom;
            try
            {
                var reader = new GeoJsonReader();

                // Parse GeoJSON to JObject to inspect
                var geoJsonObject = JObject.Parse(dto.GeoJson);
                string geometryJson;

                if (geoJsonObject["type"]?.ToString() == "FeatureCollection")
                {
                    // Extract geometry of first feature in the collection
                    var firstFeature = geoJsonObject["features"]?.First;
                    if (firstFeature == null)
                        return Result<AppLocation>.ValidationError("FeatureCollection contains no features.");

                    geometryJson = firstFeature["geometry"]?.ToString();
                    if (string.IsNullOrWhiteSpace(geometryJson))
                        return Result<AppLocation>.ValidationError("Feature geometry is missing.");
                }
                else if (geoJsonObject["type"]?.ToString() == "Feature")
                {
                    // If single Feature, extract its geometry
                    geometryJson = geoJsonObject["geometry"]?.ToString();
                    if (string.IsNullOrWhiteSpace(geometryJson))
                        return Result<AppLocation>.ValidationError("Feature geometry is missing.");
                }
                else
                {
                    // Assume the input is a raw Geometry GeoJSON
                    geometryJson = dto.GeoJson;
                }

                // Parse the extracted geometry JSON
                geom = reader.Read<Geometry>(geometryJson);

                geom = FixPolygonOrientation(geom);
            }
            catch (Exception ex)
            {
                return Result<AppLocation>.ValidationError($"Invalid GeoJSON: {ex.Message}");
            }

            var centroid = geom.Centroid;
            if (!IsFinite(centroid.X) || !IsFinite(centroid.Y))
                return Result<AppLocation>.ValidationError("Geometry centroid has invalid coordinate values.");

            existing.Geography = geom;

            var geoInfo = await _geocodingService.ReverseGeocodeAsync(centroid.Y, centroid.X);
            existing.Address = geoInfo?.Address;
            existing.Region = geoInfo?.Region;
            existing.Country = geoInfo?.Country;
        }

        existing.Name = dto.Name;

        await _repository.UpdateAsync(existing);
        await _repository.SaveChangesAsync();

        return Result<AppLocation>.Success(existing, "Location updated.");
    }


    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var existingLocation = await _repository.GetByIdAsync(id);
        if (existingLocation == null)
        {
            return Result<bool>.NotFoundError("Location not found.");
        }
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();

        return Result<bool>.Success(true, "Location deleted successfully.");
    }

    private Geometry FixPolygonOrientation(Geometry geometry)
    {
        if (geometry is Polygon poly)
        {
            var shell = poly.Shell;
            if (!Orientation.IsCCW(shell.CoordinateSequence))
            {
                shell = (LinearRing)shell.Reverse();
            }

            var holes = poly.Holes
                .Select(hole => Orientation.IsCCW(hole.CoordinateSequence) ? (LinearRing)hole.Reverse() : hole)
                .ToArray();

            return new Polygon(shell, holes, geometry.Factory);
        }
        else if (geometry is MultiPolygon multiPoly)
        {
            var fixedPolygons = multiPoly.Geometries
                .Cast<Polygon>()
                .Select(p => (Polygon)FixPolygonOrientation(p))
                .ToArray();

            return new MultiPolygon(fixedPolygons, geometry.Factory);
        }

        return geometry;
    }

    private double? SanitizeNullableDouble(double? value)
    {
        if (!value.HasValue) return null;
        return double.IsNaN(value.Value) || double.IsInfinity(value.Value) ? null : value;
    }
}
