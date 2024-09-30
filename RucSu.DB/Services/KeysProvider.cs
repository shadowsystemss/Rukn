using RucSu.DB.DataBases;

namespace RucSu.DB.Services
{
    public class KeysProvider(KeysDB db, KeysManager manager)
    {
        public async Task<KeyValuePair<string, string>?> GetBranchAsync(string branch)
        {
            KeyValuePair<string, string>? pair = db.FindBranch(branch);
            if (pair is null)
            {
                Dictionary<string, string>? pairs = await manager.UpdateBranchesAsync();
                pair = pairs?.First(x => x.Key == branch || x.Value == branch);
            }
            return pair;
        }

        public async Task<KeyValuePair<string, string>?> GetEmployeeAsync(string branch, string employee)
        {
            KeyValuePair<string, string>? branchPair = await GetBranchAsync(branch);
            if (branchPair is null) return null;

            KeyValuePair<string, string>? employeePair = db.FindEmployee(branchPair.Value.Key, employee);
            if (employeePair is null)
            {
                Dictionary<string, string>? pairs = await manager.UpdateEmployeesAsync(branchPair.Value.Key);
                employeePair = pairs?.First(x => x.Key == employee || x.Value == employee);
            }
            return employeePair;
        }

        public async Task<KeyValuePair<string, string>?> GetYearAsync(string branch, string year)
        {
            KeyValuePair<string, string>? branchPair = await GetBranchAsync(branch);
            if (branchPair is null) return null;

            KeyValuePair<string, string>? yearPair = db.FindYear(branchPair.Value.Key, year);
            if (yearPair is null)
            {
                Dictionary<string, string>? pairs = await manager.UpdateYearsAsync(branchPair.Value.Key);
                yearPair = pairs?.First(x => x.Key == year || x.Value == year);
            }
            return yearPair;
        }

        public async Task<KeyValuePair<string, string>?> GetGroupAsync(string branch, string year, string group)
        {
            KeyValuePair<string, string>? branchPair = await GetBranchAsync(branch);
            if (branchPair is null) return null;

            KeyValuePair<string, string>? yearPair = await GetYearAsync(branch, year);
            if (branchPair is null) return null;

            KeyValuePair<string, string>? groupPair = db.FindGroup(branchPair.Value.Key, yearPair.Value.Key, group);
            if (groupPair is null)
            {
                Dictionary<string, string>? pairs = await manager.UpdateGroupsAsync(branchPair.Value.Key, yearPair.Value.Key);
                groupPair = pairs?.First(x => x.Key == group || x.Value == group);
            }
            return groupPair;
        }
    }
}
