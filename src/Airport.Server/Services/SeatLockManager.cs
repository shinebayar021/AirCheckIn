//Tur sangiin zahialgiin heregjuulelt
public class SeatLockManager
{
    private static readonly Dictionary<string, DateTime> _locks = new();

    public static bool TryLockSeat(string seatKey)
    {
        lock (_locks)
        {
            if (_locks.TryGetValue(seatKey, out var expiry))
            {
                if (expiry > DateTime.UtcNow)
                    return false; // lock идэвхтэй
                else
                    _locks.Remove(seatKey); // хугацаа дууссан, устгах
            }
            _locks[seatKey] = DateTime.UtcNow.AddSeconds(30);
            return true;
        }
    }

    public static void UnlockSeat(string seatKey)
    {
        lock (_locks)
        {
            if (_locks.ContainsKey(seatKey))
                _locks.Remove(seatKey);
        }
    }
}
