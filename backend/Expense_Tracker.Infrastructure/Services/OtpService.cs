using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace Expense_Tracker.Infrastructure.Services;

public sealed class OtpService(IMemoryCache _cache, OtpSettings otpSettings) : IOtpService, ISingletonService
{




    public string Generate(string key, int digits = 6)
    {
        if (digits is < 4 or > 8)
            throw new ArgumentException("OTP digits must be between 4 and 8.");

        int min = (int)Math.Pow(10, digits - 1);
        int max = (int)Math.Pow(10, digits) - 1;

        string otp = RandomNumberGenerator.GetInt32(min, max).ToString();

        _cache.Set(key, otp,
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(otpSettings.ExpirationInSeconds)));

        return otp;
    }

    public bool Validate(string key, string otp, bool removeOnSuccess = true)
    {
        if (!_cache.TryGetValue<string>(key, out var storedOtp))
            return false;

        bool isValid = storedOtp == otp;
        if (isValid && removeOnSuccess)
            _cache.Remove(key);


        return isValid;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
    public bool Exists(string key)
    {
        return _cache.TryGetValue<string>(key, out _);
    }
}
