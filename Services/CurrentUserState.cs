using System.Security.Cryptography;
using microblog_heads.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace microblog_heads.Services;

/// <summary>
/// Tracks the signed-in user for the current Blazor circuit and persists the
/// user's id in protected (encrypted) browser session storage so a page
/// refresh keeps the user signed in until the tab is closed.
/// </summary>
public class CurrentUserState
{
    private const string StorageKey = "currentUserId";

    private readonly ProtectedSessionStorage _storage;
    private readonly UserService _userService;
    private bool _initialized;

    public CurrentUserState(ProtectedSessionStorage storage, UserService userService)
    {
        _storage = storage;
        _userService = userService;
    }

    public RegularUser? User { get; private set; }

    public bool IsLoggedIn => User != null;

    public bool IsInitialized => _initialized;

    public event Action? Changed;

    /// <summary>
    /// Restores the signed-in user from session storage. Browser storage is only
    /// available once the component is interactive, so call this from
    /// OnAfterRenderAsync(firstRender: true).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            var result = await _storage.GetAsync<int>(StorageKey);
            if (result.Success)
            {
                User = await _userService.GetUserByIdAsync(result.Value);
            }
        }
        catch (CryptographicException)
        {
            // Stored value was written with a different data-protection key; discard it.
            await _storage.DeleteAsync(StorageKey);
        }

        _initialized = true;
        Changed?.Invoke();
    }

    /// <summary>Throws if the username/password combination is invalid.</summary>
    public async Task LoginAsync(string username, string password)
    {
        User = await _userService.RegularUserLoginAsync(username, password);
        await _storage.SetAsync(StorageKey, User.Id);
        _initialized = true;
        Changed?.Invoke();
    }

    public async Task LogoutAsync()
    {
        User = null;
        await _storage.DeleteAsync(StorageKey);
        Changed?.Invoke();
    }
}
