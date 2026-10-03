namespace Omnicart.Domain.Enums;

public enum UserRole
{
    Customer = 1,     // সাধারণ ক্রেতা
    TenantAdmin = 2,  // নির্দিষ্ট স্টোরের মালিক / অ্যাডমিন
    Staff = 3,        // স্টোরের সেলস / সাপোর্ট কর্মী
    SuperAdmin = 4    // সম্পূর্ণ OmniCart প্ল্যাটফর্মের গ্লোবাল মালিক
}
