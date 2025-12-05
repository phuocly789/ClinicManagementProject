using System.Net.NetworkInformation;

public interface IRoomService
{
    Task<List<RoomDTO>> GetAllRoomsAsync();
}

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<RoomService> _logger;
    private readonly IUnitOfWork _uow;

    public RoomService(IRoomRepository roomRepository, ILogger<RoomService> logger, IUnitOfWork uow)
    {
        _roomRepository = roomRepository;
        _logger = logger;
        _uow = uow;
    }

    public async Task<List<RoomDTO>> GetAllRoomsAsync()
    {
        try
        {
            var rooms = await _roomRepository.GetAllAsync();

            var roomList = rooms
                .Select(r => new RoomDTO { RoomId = r.RoomId, RoomName = r.RoomName })
                .ToList();

            return roomList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching room list");
            return null;
        }
    }
}
