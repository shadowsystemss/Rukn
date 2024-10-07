using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RucSu.DB.Models
{
    public class Profile : RucSu.Models.Profile
    {
        public string? BranchName { get; private set; }
        public string? EmployeeName { get; private set; }
        public string? YearName { get; private set; }
        public string? GroupName { get; private set; }

        public void SetBranch(KeyValuePair<string, string> pair)
        {
            BranchName = pair.Key;
            Branch = pair.Value;
        }
        public void SetEmployee(KeyValuePair<string, string> pair)
        {
            EmployeeName = pair.Key;
            Employee = pair.Value;
        }
        public void SetYear(KeyValuePair<string, string> pair)
        {
            YearName = pair.Key;
            Year = pair.Value;
        }
        public void SetGroup(KeyValuePair<string, string> pair)
        {
            GroupName = pair.Key;
            Group = pair.Value;
        }
    }
}
