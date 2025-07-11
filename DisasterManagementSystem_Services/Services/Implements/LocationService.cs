using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using DisasterManagementSystem_Services.Models;
using AppLocation = DisasterManagementSystem_Data.Models.Location;
using DisasterManagementSystem_Services.Models.LocationDtos;

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
            throw new ArgumentException("Invalid GeoJSON format", ex);
        }

        if (geom != null)
        {
            geom = FixPolygonOrientation(geom);
        }

        var disasterArea = new DisasterArea
        {
            Name = dto.Name,
            Description = dto.Description,
            Area = geom,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(disasterArea);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateAsync(DisasterArea disasterArea)
    {
        disasterArea.Area = FixPolygonOrientation(disasterArea.Area);
        await _repository.UpdateAsync(disasterArea);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
        await _repository.SaveChangesAsync();
    }



    private Geometry FixPolygonOrientation(Geometry geometry)
    {
        if (geometry is Polygon poly)
        {
            var shell = poly.Shell;
            if (!Orientation.IsCCW(shell.Co`ordinateSequence))
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
