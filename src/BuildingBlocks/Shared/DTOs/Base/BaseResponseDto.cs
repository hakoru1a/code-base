namespace Shared.DTOs
{
    /// <summary>
    /// Base class for response DTOs with common properties
    /// </summary>
    /// <typeparam name="TKey">Type of the entity identifier</typeparam>
    public abstract class BaseResponseDto<TKey>
    {
        public TKey Id { get; set; } = default!;
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
    }
}

