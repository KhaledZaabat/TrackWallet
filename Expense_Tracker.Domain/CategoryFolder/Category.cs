using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Domain.CategoryFolder;


public sealed class Category : Entity
{
    public CategoryType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string IconName { get; private set; } = string.Empty;

    // EF Core constructor
    private Category() { }

    private Category(Guid id, CategoryType type) : base(id)
    {
        Type = type;
        Name = type.ToString();
        IconName = GetIconNameForType(type);
    }

    public static Result<Category> Create(CategoryType type)
    {
        var category = new Category(Guid.CreateVersion7(), type);
        return Result.Success(category);
    }

    public static Result<Category> CreateWithId(Guid id, CategoryType type)
    {
        if (id == Guid.Empty)
            return Result.Failure<Category>(
                DomainError.InvalidState(nameof(Category), "Category Id cannot be empty."));

        var category = new Category(id, type);
        return Result.Success(category);
    }

    private static string GetIconNameForType(CategoryType type)
    {
        return type switch
        {
            // FOOD & DRINKS
            CategoryType.Groceries => "shopping-bag",
            CategoryType.Restaurants => "utensils",
            CategoryType.Cafes => "coffee",
            CategoryType.FastFood => "sandwich",
            CategoryType.Alcohol => "wine",
            CategoryType.Delivery => "truck",
            CategoryType.Snacks => "candy",
            CategoryType.Bakery => "croissant",
            CategoryType.Dessert => "cake",

            // TRANSPORT
            CategoryType.Fuel => "fuel",
            CategoryType.PublicTransport => "bus",
            CategoryType.Taxi => "car",
            CategoryType.Parking => "parking-circle",
            CategoryType.VehicleMaintenance => "wrench",
            CategoryType.VehicleInsurance => "shield-check",
            CategoryType.CarWash => "droplets",

            // UTILITIES / BILLS
            CategoryType.Electricity => "lightbulb",
            CategoryType.Water => "droplet",
            CategoryType.Gas => "flame",
            CategoryType.Internet => "wifi",
            CategoryType.MobilePhone => "smartphone",
            CategoryType.Heating => "flame",
            CategoryType.TrashService => "trash",
            CategoryType.HomeMaintenance => "hammer",
            CategoryType.SecuritySystem => "shield",

            // HOUSING
            CategoryType.Rent => "home",
            CategoryType.Mortgage => "home",
            CategoryType.PropertyTax => "receipt",
            CategoryType.HOAFees => "badge-dollar-sign",
            CategoryType.HomeInsurance => "shield-check",

            // SHOPPING
            CategoryType.Clothing => "shirt",
            CategoryType.Shoes => "footprints",
            CategoryType.Accessories => "gem",
            CategoryType.Electronics => "monitor",
            CategoryType.Furniture => "sofa",
            CategoryType.HomeDecor => "home",
            CategoryType.PersonalCare => "sparkles",
            CategoryType.Beauty => "brush",

            // ENTERTAINMENT
            CategoryType.Movies => "film",
            CategoryType.Music => "music",
            CategoryType.Gaming => "gamepad-2",
            CategoryType.Streaming => "tv",
            CategoryType.Events => "ticket",
            CategoryType.Books => "book-open",
            CategoryType.Hobbies => "palette",
            CategoryType.Subscriptions => "wallet-cards",

            // HEALTH
            CategoryType.Healthcare => "stethoscope",
            CategoryType.Pharmacy => "pill",
            CategoryType.DentalCare => "smile",
            CategoryType.VisionCare => "eye",
            CategoryType.GymMembership => "dumbbell",
            CategoryType.Sports => "heart-pulse",
            CategoryType.MentalHealth => "brain",

            // EDUCATION & WORK
            CategoryType.Education => "graduation-cap",
            CategoryType.Tuition => "receipt",
            CategoryType.Courses => "presentation",
            CategoryType.OfficeSupplies => "pen-tool",
            CategoryType.Software => "layers",

            // FINANCE
            CategoryType.Savings => "piggy-bank",
            CategoryType.Investments => "trending-up",
            CategoryType.BankFees => "badge-alert",
            CategoryType.LoanPayments => "wallet",
            CategoryType.Taxes => "receipt",

            // TRAVEL
            CategoryType.Flights => "plane",
            CategoryType.Hotels => "bed",
            CategoryType.CarRental => "car",
            CategoryType.TravelActivities => "sun",
            CategoryType.TravelInsurance => "shield",

            // FAMILY & PETS
            CategoryType.Childcare => "baby",
            CategoryType.PetCare => "cat",
            CategoryType.PetFood => "bone",
            CategoryType.VetBills => "stethoscope",
            CategoryType.FamilySupport => "users",

            // GIFTS / CHARITY
            CategoryType.Gifts => "gift",
            CategoryType.Donations => "helping-hand",

            // OTHER
            CategoryType.Miscellaneous => "more-horizontal",
            _ => "more-horizontal"
        };
    }


}

public enum CategoryType
{
    // Food & Drinks
    Groceries,
    Restaurants,
    Cafes,
    FastFood,
    Alcohol,
    Delivery,
    Snacks,
    Bakery,
    Dessert,

    // Transportation
    Fuel,
    PublicTransport,
    Taxi,
    Parking,
    VehicleMaintenance,
    VehicleInsurance,
    CarWash,

    // Bills & Utilities
    Electricity,
    Water,
    Gas,
    Internet,
    MobilePhone,
    Heating,
    TrashService,
    HomeMaintenance,
    SecuritySystem,

    // Housing
    Rent,
    Mortgage,
    PropertyTax,
    HOAFees,
    HomeInsurance,

    // Shopping
    Clothing,
    Shoes,
    Accessories,
    Electronics,
    Furniture,
    HomeDecor,
    PersonalCare,
    Beauty,

    // Entertainment
    Movies,
    Music,
    Gaming,
    Streaming,
    Events,
    Books,
    Hobbies,
    Subscriptions,

    // Health
    Healthcare,
    Pharmacy,
    DentalCare,
    VisionCare,
    GymMembership,
    Sports,
    MentalHealth,

    // Education & Work
    Education,
    Tuition,
    Courses,
    OfficeSupplies,
    Software,

    // Finance
    Savings,
    Investments,
    BankFees,
    LoanPayments,
    Taxes,

    // Travel
    Flights,
    Hotels,
    CarRental,
    TravelActivities,
    TravelInsurance,

    // Family & Pets
    Childcare,
    PetCare,
    PetFood,
    VetBills,
    FamilySupport,

    // Gifts & Charity
    Gifts,
    Donations,

    // Other
    Miscellaneous
}