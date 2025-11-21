using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Core.Options;

public class PlanSettingsOptions
{
    public PlanSetting Starter { get; set; } = PlanSetting.CreateStarterDefaults();
    public PlanSetting Pro { get; set; } = PlanSetting.CreateProDefaults();
    public PlanSetting Business { get; set; } = PlanSetting.CreateBusinessDefaults();

    public PlanSetting GetSetting(PlanType plan) =>
        plan switch
        {
            PlanType.Pro => Pro ?? PlanSetting.CreateProDefaults(),
            PlanType.Business => Business ?? PlanSetting.CreateBusinessDefaults(),
            _ => Starter ?? PlanSetting.CreateStarterDefaults()
        };
}

public class PlanSetting
{
    public string DisplayName { get; set; } = string.Empty;
    public int MonthlyTransactions { get; set; }
    public int TeamMembersLimit { get; set; }
    public int BankAccountsLimit { get; set; }
    public int PixKeysLimit { get; set; }
    public int PixQrCodesLimit { get; set; }
    public string PriceText { get; set; } = string.Empty;

    public static PlanSetting CreateStarterDefaults() => new()
    {
        DisplayName = "Grátis",
        MonthlyTransactions = 30,
        TeamMembersLimit = 1,
        BankAccountsLimit = 1,
        PixKeysLimit = 1,
        PixQrCodesLimit = 20,
        PriceText = "R$ 0/mês"
    };

    public static PlanSetting CreateProDefaults() => new()
    {
        DisplayName = "Pro",
        MonthlyTransactions = 1000,
        TeamMembersLimit = 0,
        BankAccountsLimit = 0,
        PixKeysLimit = 0,
        PixQrCodesLimit = 0,
        PriceText = "R$ 399/mês"
    };

    public static PlanSetting CreateBusinessDefaults() => new()
    {
        DisplayName = "Business",
        MonthlyTransactions = int.MaxValue,
        TeamMembersLimit = 0,
        BankAccountsLimit = 0,
        PixKeysLimit = 0,
        PixQrCodesLimit = 0,
        PriceText = "Custom"
    };
}
