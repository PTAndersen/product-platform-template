using Microsoft.JSInterop;

public class CookieConsentService
{
    private readonly IJSRuntime _js;

    public CookieConsentService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetUserConsentAsync(bool acceptAll)
    {
        string value = acceptAll ? "all" : "necessary";
        await _js.InvokeVoidAsync("cookieHelper.setCookie", "cookieConsent", value, 365);
    }

    public async Task<bool> HasConsentAsync()
    {
        var consent = await _js.InvokeAsync<string>("cookieHelper.getCookie", "cookieConsent");
        return !string.IsNullOrEmpty(consent);
    }

    public async Task<bool> IsTrackingAllowedAsync()
    {
        var consent = await _js.InvokeAsync<string>("cookieHelper.getCookie", "cookieConsent");
        return consent == "all";
    }

    public async Task ResetCookiesAsync()
    {
        await _js.InvokeVoidAsync("cookieHelper.clearAllCookies");
    }
}
