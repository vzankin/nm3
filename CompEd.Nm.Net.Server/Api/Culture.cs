using Microsoft.AspNetCore.Localization;

namespace CompEd.Nm.Net.Api;

public static class Culture
{
    public static void OnGet(HttpResponse response, string culture, string redirect)
    {
        response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture))
        );
        response.Redirect(redirect);
    }
}
