using Shared.Interfaces.Event;

namespace TLBIOMASS.Domain.Customers.Events;

public record CustomerUpdatedEvent : BaseEvent
{
    public int CustomerId { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
}
