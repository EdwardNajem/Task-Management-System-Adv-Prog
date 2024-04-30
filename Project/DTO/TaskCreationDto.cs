namespace Project.DTO
{
    public class TaskCreationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
    }
}
