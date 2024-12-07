namespace KVATUM_STREAMING_SERVICE.Core.IService
{
    public interface IRoomConnectionService
    {
        IEnumerable<Guid> JoinToRoomAndGetMembers(Guid roomId, Guid accountId);
        void LeaveFromRoom(Guid roomId, Guid accountId);
        HashSet<Guid> GetRoomMembers(Guid roomId);
    }
}