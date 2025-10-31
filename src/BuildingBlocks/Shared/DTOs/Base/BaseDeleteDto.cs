namespace Shared.DTOs
{
    /// <summary>
    /// Base class for delete DTOs
    /// </summary>
    /// <typeparam name="TKey">Type of the entity identifier</typeparam>
    public abstract class BaseDeleteDto<TKey>
    {
        public TKey Id { get; set; } = default!;
    }
}

