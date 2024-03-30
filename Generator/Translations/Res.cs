// Note: we intentionally keep this namespace as "TPRandomizer" to make it as
// easy as possible to use.
namespace TPRandomizer
{
    using System;
    using System.Globalization;
    using Microsoft.Extensions.DependencyInjection;

    public class Res
    {
        private static Translations translations;

        static Res()
        {
            IServiceProvider provider = Global.GetServiceProvider();
            translations = provider.GetRequiredService<Translations>();
        }

        public static void UpdateCultureInfo(string name)
        {
            CultureInfo newCultureInfo = CultureInfo.GetCultureInfo(name);

            if (
                CultureInfo.CurrentCulture.Equals(newCultureInfo)
                && CultureInfo.CurrentUICulture.Equals(newCultureInfo)
            )
            {
                // Do nothing if new culture matches current culture.
                return;
            }

            // "fr-FR-DOG"
            CultureInfo.CurrentCulture = newCultureInfo;
            CultureInfo.CurrentUICulture = newCultureInfo;

            translations.OnCultureChange();
        }

        public static string Msg()
        {
            return translations.GetGreetingMessage();
        }
    }
}
