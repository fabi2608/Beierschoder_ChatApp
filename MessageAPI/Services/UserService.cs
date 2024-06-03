using MongoDB.Driver;
using MessageAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageAPI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("Users");
        }

        public async Task<User> GetUserAsync(string username, string password)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username) &
                            Builders<User>.Filter.Eq(u => u.Password, password);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var existingUser = await GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
            {
                return false;
            }

            await _usersCollection.InsertOneAsync(user);
            return true;
        }

        private async Task<User> GetUserByUsernameAsync(string username)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Username, username);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }


        public async Task<List<User>> GetAllUsersAsync()
        {
            var filter = Builders<User>.Filter.Empty;
            var users = await _usersCollection.FindAsync(filter).Result.ToListAsync();
            return users;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            try
            {
                var filter = Builders<User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(query, "i"));
                var searchResults = await _usersCollection.Find(filter).ToListAsync();
                return searchResults;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching users: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Username, username);
                var result = await _usersCollection.DeleteOneAsync(filter);

                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}", ex);
            }
        }
    }
}
