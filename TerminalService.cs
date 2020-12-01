using SocialDistancing.Services.Contracts;
using SocialDistancing.Services.Model;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Collections.Generic;

namespace SocialDistancing.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly IMongoCollection<Terminal> _terminals;

        public TerminalService(ITerminalDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _terminals = database.GetCollection<Terminal>(settings.TerminalCollectionName);
        }

        public async Task<Terminal> CreateAsync(Terminal terminal)
        {
            await _terminals.InsertOneAsync(terminal); 
            return terminal;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _terminals.DeleteOneAsync(terminal => terminal.Id == id);
            return result.IsAcknowledged;
        }

        public async Task<bool> DeleteAsync(Terminal terminalIn)
        {
            var result = await _terminals.DeleteOneAsync(terminal => terminal.Id == terminalIn.Id);     
            return result.IsAcknowledged;
        }

        public async Task<List<Terminal>> GetAsync()
        {
            return await _terminals.Find<Terminal>(FilterDefinition<Terminal>.Empty).ToListAsync();
        }

        public async Task<Terminal> GetAsync(string samNumber)
        {
            return await _terminals.Find<Terminal>(terminal => terminal.SamNumber == samNumber).SingleOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string samNumber, Terminal terminal)
        {
            var result = await _terminals.ReplaceOneAsync(t => t.SamNumber == samNumber, terminal);
            return result.IsAcknowledged;
        }

        public async Task<bool> PatchAsync(string samNumber, TerminalPatch patch)
        {
            var update = Builders<Terminal>.Update;
            var updates = new List<UpdateDefinition<Terminal>>();

            if (patch.LeftNeighborSamNumber != null)
                updates.Add(update.Set(terminal => terminal.LeftNeighborSamNumber, patch.LeftNeighborSamNumber));
            if (patch.RightNeighborSamNumber != null)
                updates.Add(update.Set(terminal => terminal.RightNeighborSamNumber, patch.RightNeighborSamNumber));

            updates.Add(update.Set(terminal => terminal.InUse, patch.Available));

            var result = await _terminals.UpdateOneAsync(t => t.SamNumber == samNumber, update.Combine(updates));
            return result.IsAcknowledged;
        }

    }
}
