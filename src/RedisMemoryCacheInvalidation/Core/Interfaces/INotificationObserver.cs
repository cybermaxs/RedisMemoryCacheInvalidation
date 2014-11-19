
namespace RedisMemoryCacheInvalidation.Core.Interfaces
{
    /// <summary>
    /// Provides a mechanism for receiving push-based notifications.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INotificationObserver<T>
    {
        void Notify(T value);
    }
}
