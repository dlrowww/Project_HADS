using System;

namespace Search.API.Models
{
    public class TransportOfferDto
    {
        /// <summary>
        /// 后端生成的可读短 ID（8 位 Base36），客户端展示此字段。
        /// </summary>
        public string ShortId        { get; set; } = "";

        /// <summary>
        /// 原始全局唯一 GUID，内部主键，如不需要可在前端隐藏。
        /// </summary>
       // public Guid   Id             { get; set; }

        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime   { get; set; }

        public decimal Price          { get; set; }
        public string  Currency       { get; set; } = "GBP";
        public string  Provider       { get; set; } = "";

        public string FromCity        { get; set; } = "";
        public string ToCity          { get; set; } = "";

        public int     AvailableSeats { get; set; }
        public bool    IsPromoted     { get; set; }
        public string  Transport      { get; set; } = "bus";

        public int     DurationHours     { get; set; }
        public int     DurationMinutes   { get; set; }


    }
}
