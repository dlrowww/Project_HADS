
namespace Booking.Domain.Enums
{
    public enum BookingStatus
    {
        Created,     // 用户发起预订，等待支付
        Paid,        // 支付成功
        Failed,      // 支付失败或超时
        Cancelled,    // 用户主动取消
        Confirmed,  // 预订已确认
    }
}
