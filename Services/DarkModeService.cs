using Microsoft.JSInterop;

namespace microblog_heads.Services;

public class DarkModeService
{
    private readonly IJSRuntime _js;

    public DarkModeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task ApplyDarkModeAsync()
    {
        var isDarkMode = await _js.InvokeAsync<string>("localStorage.getItem", "darkMode");
        if(isDarkMode == "True")
        {
            await _js.InvokeAsync<object>("document.body.classList.add","dark-mode");

        }
        else
        {
            await _js.InvokeAsync<object>("document.body.classList.remove", "dark-mode");
            
        }
    }
}