namespace RucSu.Models
{
    public class Profile
    {
        public bool EmployeeMode { get; set; }
        public string? Branch { get; set; }
        public string? Employee { get; set; }
        public string? Year { get; set; }
        public string? Group { get; set; }

        public bool CanUse => Branch is not null
            && (EmployeeMode ? Employee is not null : (Year is not null && Group is not null));

        public string Parameters => $"branch={Branch}"
            + (EmployeeMode ? $"&employee={Employee}" : $"&year={Year}&group={Group}");
    }
}
