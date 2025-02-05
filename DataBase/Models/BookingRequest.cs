namespace DataBase.Models
{   
    public class BookingRequest
    {
        public Customer Customer { get; set; }
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }

}
