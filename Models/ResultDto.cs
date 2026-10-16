namespace RaceDay.Models
{
    public class ResultDto
    {
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public string ResultStatus { get; set; }
    }
}