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
    public LocationService(IlocationRepository repository, INominatimGeocodingService geocodingService)
    {
        _repository = repository;
        _geocodingService = geocodingService;
    }

    public async Task<Result<AppLocation>> GetByIdAsync(int id)
    {
        var loc = await _repository.GetByIdAsync(id);
        return loc == null
            ? Result<AppLocation>.NotFoundError("Location not found.")
            : Result<AppLocation>.Success(loc);
    }

    public async Task<Result<IEnumerable<AppLocation>>> GetAllAsync()
    {
        var all = await _repository.GetAllAsync();
        return Result<IEnumerable<AppLocation>>.Success(all);
    }


    public async Task<Result<AppLocation>> AddAsync(LocationCreateDto dto)
    {
        Geometry geom;
        try
        {
            var reader = new GeoJsonReader();
            geom = reader.Read<Geometry>(dto.GeoJson);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid GeoJSON: ", ex.Message);
        }

        geom = FixPolygonOrientation(geom);
        var centroid = geom.Centroid;
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

        return Result<AppLocation>.Success(location, "Location added successfully.");
    }

    public async Task<Result<AppLocation>> UpdateAsync(LocationUpdateDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null)
            return Result<AppLocation>.NotFoundError("Location not found.");

        // Fix GeoJson if it's provided
        if (!string.IsNullOrWhiteSpace(dto.GeoJson))
        {
            var reader = new GeoJsonReader();
            try
            {
                var geom = FixPolygonOrientation(reader.Read<Geometry>(dto.GeoJson));
                existing.Geography = geom;

                // Optional: reverse geocode if location changed
                var center = geom.Centroid;
                var geoInfo = await _geocodingService.ReverseGeocodeAsync(center.Y, center.X);
                existing.Address = geoInfo?.Address;
                existing.Region = geoInfo?.Region;
                existing.Country = geoInfo?.Country;
            }
            catch
            {
                return Result<AppLocation>.ValidationError("Invalid GeoJSON.");
            }
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

        // For other geometry types, return as is
        return geometry;
    }



}
