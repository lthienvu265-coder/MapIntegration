namespace IndoorWayfinder.Api.Models.Requests
{
    public class ClearMapIn
    {
        public int MapId { get; set; }
        public bool DeleteMap { get; set; } = false;
        public bool DeleteUpload { get; set; } = false;
    }
}
