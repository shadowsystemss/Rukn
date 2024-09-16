namespace RucSu.DB.Models
{
    public class Profile
    {
        public bool EmployeeMode { get; set; }
        public string? Branch { get; set; }
        public string? Employee { get; set; }
        public string? Year { get; set; }
        public string? Group { get; set; }

        public string Parameters => $"branch={Branch}"
            + (EmployeeMode ? $"&employee={Employee}" : $"&year={Year}&group={Group}");
    }
}
