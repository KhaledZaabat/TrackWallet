using ErrorOr;
using Expense_Tracker.Domain.Common;

namespace Expense_Tracker.Domain.CategoryFolder;

public sealed class Category : Entity
{
    public CategoryType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string IconName { get; private set; } = string.Empty;

    private Category() { }

    private Category(Guid id, CategoryType type) : base(id)
    {
        Type = type;
        Name = type.ToString();
        IconName = GetIconNameForType(type);
    }

    public static ErrorOr<Category> Create(CategoryType type)
    {
        var category = new Category(Guid.CreateVersion7(), type);
        return category;
    }

    public static ErrorOr<Category> CreateWithId(Guid id, CategoryType type)
    {
        if (id == Guid.Empty)
            return Domain.Errors.DomainErrors.GeneralErrors.InvalidState(nameof(Category), "Category Id cannot be empty.");

        var category = new Category(id, type);
        return category;
    }

    private static string GetIconNameForType(CategoryType type)
    {
        return type switch
        {
            CategoryType.Groceries => "shopping-bag",
            CategoryType.Restaurants => "utensils",
            CategoryType.Cafes => "coffee",
            CategoryType.FastFood => "sandwich",
            CategoryType.Alcohol => "wine",
            CategoryType.Delivery => "truck",
            CategoryType.Snacks => "candy",
            CategoryType.Bakery => "croissant",
            CategoryType.Dessert => "cake",
            CategoryType.Fuel => "fuel",
            CategoryType.PublicTransport => "bus",
            CategoryType.Taxi => "car",
            CategoryType.Parking => "parking-circle",
            CategoryType.VehicleMaintenance => "wrench",
            CategoryType.VehicleInsurance => "shield-check",
            CategoryType.CarWash => "droplets",
            CategoryType.Electricity => "lightbulb",
            CategoryType.Water => "droplet",
            CategoryType.Gas => "flame",
            CategoryType.Internet => "wifi",
            CategoryType.MobilePhone => "smartphone",
            CategoryType.Heating => "flame",
            CategoryType.TrashService => "trash",
            CategoryType.HomeMaintenance => "hammer",
            CategoryType.SecuritySystem => "shield",
            CategoryType.Rent => "home",
            CategoryType.Mortgage => "home",
            CategoryType.PropertyTax => "receipt",
            CategoryType.HOAFees => "badge-dollar-sign",
            CategoryType.HomeInsurance => "shield-check",
            CategoryType.Clothing => "shirt",
            CategoryType.Shoes => "footprints",
            CategoryType.Accessories => "gem",
            CategoryType.Electronics => "monitor",
            CategoryType.Furniture => "sofa",
            CategoryType.HomeDecor => "home",
            CategoryType.PersonalCare => "sparkles",
            CategoryType.Beauty => "brush",
            CategoryType.Movies => "film",
            CategoryType.Music => "music",
            CategoryType.Gaming => "gamepad-2",
            CategoryType.Streaming => "tv",
            CategoryType.Events => "ticket",
            CategoryType.Books => "book-open",
            CategoryType.Hobbies => "palette",
            CategoryType.Subscriptions => "wallet-cards",
            CategoryType.Healthcare => "stethoscope",
            CategoryType.Pharmacy => "pill",
            CategoryType.DentalCare => "smile",
            CategoryType.VisionCare => "eye",
            CategoryType.GymMembership => "dumbbell",
            CategoryType.Sports => "heart-pulse",
            CategoryType.MentalHealth => "brain",
            CategoryType.Education => "graduation-cap",
            CategoryType.Tuition => "receipt",
            CategoryType.Courses => "presentation",
            CategoryType.OfficeSupplies => "pen-tool",
            CategoryType.Software => "layers",
            CategoryType.Savings => "piggy-bank",
            CategoryType.Investments => "trending-up",
            CategoryType.BankFees => "badge-alert",
            CategoryType.LoanPayments => "wallet",
            CategoryType.Taxes => "receipt",
            CategoryType.Flights => "plane",
            CategoryType.Hotels => "bed",
            CategoryType.CarRental => "car",
            CategoryType.TravelActivities => "sun",
            CategoryType.TravelInsurance => "shield",
            CategoryType.Childcare => "baby",
            CategoryType.PetCare => "cat",
            CategoryType.PetFood => "bone",
            CategoryType.VetBills => "stethoscope",
            CategoryType.FamilySupport => "users",
            CategoryType.Gifts => "gift",
            CategoryType.Donations => "helping-hand",
            CategoryType.Miscellaneous => "more-horizontal",
            _ => "more-horizontal"
        };
    }
}

public enum CategoryType
{
    Groceries, Restaurants, Cafes, FastFood, Alcohol, Delivery, Snacks, Bakery, Dessert,
    Fuel, PublicTransport, Taxi, Parking, VehicleMaintenance, VehicleInsurance, CarWash,
    Electricity, Water, Gas, Internet, MobilePhone, Heating, TrashService, HomeMaintenance, SecuritySystem,
    Rent, Mortgage, PropertyTax, HOAFees, HomeInsurance,
    Clothing, Shoes, Accessories, Electronics, Furniture, HomeDecor, PersonalCare, Beauty,
    Movies, Music, Gaming, Streaming, Events, Books, Hobbies, Subscriptions,
    Healthcare, Pharmacy, DentalCare, VisionCare, GymMembership, Sports, MentalHealth,
    Education, Tuition, Courses, OfficeSupplies, Software,
    Savings, Investments, BankFees, LoanPayments, Taxes,
    Flights, Hotels, CarRental, TravelActivities, TravelInsurance,
    Childcare, PetCare, PetFood, VetBills, FamilySupport,
    Gifts, Donations,
    Miscellaneous
}
