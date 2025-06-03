//Tur sangiin zahialgiin heregjuulelt
using System.Collections.Concurrent;

public class SeatLockManager
{
    private static readonly Dictionary<string, DateTime> _locks = new();
    /// <summary>
    /// semaphoreslim ni synchronchlolt,  ConcurrentDictionary<string, SemaphoreSlim> ni suudal burt zoriulsan lock hadgaldag map
    /// </summary>
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _seatLocks = new();
    //public static bool TryLockSeat(string seatKey)
    //{
    //    lock (_locks)
    //    {
    //        if (_locks.TryGetValue(seatKey, out var expiry))
    //        {
    //            if (expiry > DateTime.UtcNow)
    //                return false; // lock идэвхтэй
    //            else
    //                _locks.Remove(seatKey); // хугацаа дууссан, устгах
    //        }
    //        _locks[seatKey] = DateTime.UtcNow.AddSeconds(30);
    //        return true;
    //    }
    //}
    
    // avsan seatkeyeer suudaliig uuriin locktoi bolgoh
    public static async Task<bool> WaitToLockSeatAsync(string seatKey, int timeoutMillis = 5000)
    {
        // (1,1) ni neg udaa neg thread oroh erhtei baih
        var semaphore = _seatLocks.GetOrAdd(seatKey, key => new SemaphoreSlim(1, 1));
        return await semaphore.WaitAsync(timeoutMillis);
    }

    public static void UnlockSeat(string seatKey)
    {
        if (_seatLocks.TryGetValue(seatKey, out var semaphore))
        {
            semaphore.Release();
        }
    }
}
