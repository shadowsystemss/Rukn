using RucSu.DB.DataBases;
using RucSu.Services;

namespace RucSu.DB.Services
{
    public class KeysManager(HttpClient client, KeysDB db)
    {
        public async Task<Dictionary<string, string>?> GetBranchesAsync(CancellationToken cancel = default)
            => db.GetBranches() ?? await UpdateBranchesAsync(cancel);

        public async Task<Dictionary<string, string>?> UpdateBranchesAsync(CancellationToken cancel = default)
        {
            try
            {
                var network = await ParserWrapper.GetSelects(client, parameters: null, cancel: cancel);
                if (network is not null
                    && network.TryGetValue("branch", out Dictionary<string, string>? data)
                    && data is not null)
                {
                    db.AddBranches(data);
                    return data;
                }
            }
            catch
            {

            }
            return null;
        }

        public async Task<Dictionary<string, string>?> UpdateEmployeesAsync(string branch, CancellationToken cancel = default)
        {
            KeyValuePair<string, string>? branchPair = db.FindBranch(branch);
            if (branchPair is null) return null;

            var network = await ParserWrapper.GetSelects(client, branch: branchPair.Value.Value, employeeMode: true, cancel: cancel);
            if (network is not null
                && network.TryGetValue("employee", out Dictionary<string, string>? data)
                && data is not null)
            {
                db.AddEmployees(branchPair.Value.Key, data);
                return data;
            }
            return null;
        }

        public async Task<Dictionary<string, string>?> UpdateYearsAsync(string branch, CancellationToken cancel = default)
        {
            KeyValuePair<string, string>? branchPair = db.FindBranch(branch);
            if (branchPair is null) return null;

            var network = await ParserWrapper.GetSelects(client,
                                                         branch: branchPair.Value.Value,
                                                         cancel: cancel);
            if (network is not null
                && network.TryGetValue("year", out Dictionary<string, string>? data)
                && data is not null)
            {
                db.AddYears(branchPair.Value.Key, data);
                return data;
            }
            return null;
        }

        public async Task<Dictionary<string, string>?> UpdateGroupsAsync(string branch,
                                                                         string year,
                                                                         CancellationToken cancel = default)
        {
            KeyValuePair<string, string>? branchPair = db.FindBranch(branch);
            if (branchPair is null) return null;

            KeyValuePair<string, string>? yearPair = db.FindYear(branchPair.Value.Key, year);
            if (yearPair is null) return null;

            var network = await ParserWrapper.GetSelects(client,
                                                         branch: branchPair.Value.Value,
                                                         year: yearPair.Value.Key,
                                                         cancel: cancel);
            if (network is not null
                && network.TryGetValue("year", out Dictionary<string, string>? data)
                && data is not null)
            {
                db.AddGroups(branchPair.Value.Key, yearPair.Value.Key, data);
                return data;
            }
            return null;
        }
    }
}
