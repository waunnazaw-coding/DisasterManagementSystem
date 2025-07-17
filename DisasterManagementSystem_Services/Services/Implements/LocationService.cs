using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using DisasterManagementSystem_Services.Models;
using AppLocation = DisasterManagementSystem_Data.Models.Location;
using DisasterManagementSystem_Services.Models.LocationDtos;
using NetTopologySuite.IO;

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
        Geometry geom;
        try
        {
            var reader = new GeoJsonReader();
            geom = reader.Read<Geometry>(dto.GeoJson);
        }
        catch (Exception ex)
        {
            return Result<LocationDto>.ValidationError($"Invalid GeoJSON: {ex.Message}");
        }

        geom = FixPolygonOrientation(geom);

        var centroid = geom.Centroid;
        if (!IsFinite(centroid.X) || !IsFinite(centroid.Y))
            return Result<LocationDto>.ValidationError("Geometry centroid has invalid coordinate values.");

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
            GeoJson = location.Geography != null ? _geoJsonWriter.Write(FixPolygonOrientation(location.Geography)) : null,
            Address = location.Address,
            Country = location.Country,
            Region = location.Region,
            Latitude = SanitizeNullableDouble(location.Geography?.Centroid?.Y),
            Longitude = SanitizeNullableDouble(location.Geography?.Centroid?.X)
        };

        return Result<LocationDto>.Success(createdDto, "Location added successfully.");
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
                geom = reader.Read<Geometry>(dto.GeoJson);
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
