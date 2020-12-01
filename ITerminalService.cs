using SocialDistancing.Services.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialDistancing.Services.Contracts
{
    public interface ITerminalService
    {
        Task<Terminal> CreateAsync(Terminal terminal);

        Task<bool> UpdateAsync(string samNumber, Terminal terminal);

        Task<bool> PatchAsync(string samNumber, TerminalPatch terminal);

        Task<bool> DeleteAsync(string id);

        Task<bool> DeleteAsync(Terminal terminalIn);

        Task<List<Terminal>> GetAsync();

        Task<Terminal> GetAsync(string samNumber);
    }
}
