namespace Shared.DTOs
{
    /// <summary>
    /// Base class for update DTOs
    /// </summary>
    /// <typeparam name="TKey">Type of the entity identifier</typeparam>
    public abstract class BaseUpdateDto<TKey>
    {
        public TKey Id { get; set; } = default!;
    }
}

