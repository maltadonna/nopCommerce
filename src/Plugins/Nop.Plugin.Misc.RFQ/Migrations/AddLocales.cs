using System.Globalization;
using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Common;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.RFQ.Migrations;

[NopMigration("2025/10/27 12:41:53:1677555", "Misc.RFQ add the locale")]
public class AddLocales : ForwardOnlyMigration
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var languages = languageService.GetAllLanguagesAsync(true).Result;
        var languageId = languages
            .Where(lang => lang.UniqueSeoCode == new CultureInfo(NopCommonDefaults.DefaultLanguageCulture).TwoLetterISOLanguageName)
            .Select(lang => lang.Id).FirstOrDefault();

        localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.RFQ.ShowCaptchaOnRequestPage"] = "Show CAPTCHA on request page",
            ["Plugins.Misc.RFQ.ShowCaptchaOnRequestPage.Hint"] = "Check to show CAPTCHA on request page, when send the new request a quote.",
            ["Plugins.Misc.RFQ.CaptchaDisabled.Notification"] = "In order to use this functionality, you have to enable the following setting: General settings > CAPTCHA > CAPTCHA enabled."
        }, languageId).Wait();
    }
}