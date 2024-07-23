using CometUserAPI.Entities;
using CometUserAPI.Service;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CometUserAPI.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly CometUserDBContext _dbContext;
        public RefreshHandler(CometUserDBContext context) {
            this._dbContext = context;
        }
        public async Task<string> GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomnumber);
                string refreshToken = Convert.ToBase64String(randomnumber);
                var existingToken = this._dbContext.TblRefreshtokens.FirstOrDefaultAsync(item => item.UserId == username).Result;
                if (existingToken != null)
                {
                    existingToken.RefreshToken = refreshToken;
                } else
                {
                    await this._dbContext.TblRefreshtokens.AddAsync(new TblRefreshtoken
                    {
                        UserId = username,
                        TokenId = new Random().Next().ToString(),
                        RefreshToken = refreshToken
                    });
                }
                await this._dbContext.SaveChangesAsync();

                return refreshToken;
            }
        }
    }
}
