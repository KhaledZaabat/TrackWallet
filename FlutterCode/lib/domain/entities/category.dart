import 'package:flutter/material.dart';
import 'package:lucide_icons/lucide_icons.dart';

enum CategoryType {
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
  Miscellaneous,
}

class CategoryData {
  final String categoryId;
  final String name;
  final CategoryType categoryType;

  CategoryData({
    required this.categoryId,
    required this.name,
    required this.categoryType,
  });

  factory CategoryData.fromJson(Map<String, dynamic> json) {
    final name = json['name'] as String;
    final categoryType = _parseCategoryType(name);

    return CategoryData(
      categoryId: json['categoryId'] as String,
      name: name,
      categoryType: categoryType,
    );
  }

  Map<String, dynamic> toJson() => {
        'categoryId': categoryId,
        'name': name,
      };

  IconData get icon => CategoryIconHelper.iconFor(categoryType);

  String get displayName => _formatDisplayName(name);

  static String _formatDisplayName(String name) {
    // Convert camelCase to Title Case with spaces
    final result = name
        .replaceAllMapped(
          RegExp(r'([A-Z])'),
          (match) => ' ${match.group(0)}',
        )
        .trim();
    return result;
  }

  static CategoryType _parseCategoryType(String name) {
    try {
      return CategoryType.values.firstWhere(
        (type) => type.name == name,
        orElse: () => CategoryType.Miscellaneous,
      );
    } catch (e) {
      return CategoryType.Miscellaneous;
    }
  }

  @override
  String toString() =>
      'CategoryData(id: $categoryId, name: $name, type: $categoryType)';

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CategoryData &&
          runtimeType == other.runtimeType &&
          categoryId == other.categoryId;

  @override
  int get hashCode => categoryId.hashCode;
}

class CategoryIconHelper {
  static IconData iconFor(CategoryType type) {
    switch (type) {
      // FOOD & DRINKS
      case CategoryType.Groceries:
        return LucideIcons.shoppingBag;
      case CategoryType.Restaurants:
        return LucideIcons.utensils;
      case CategoryType.Cafes:
        return LucideIcons.coffee;
      case CategoryType.FastFood:
        return LucideIcons.sandwich;
      case CategoryType.Alcohol:
        return LucideIcons.wine;
      case CategoryType.Delivery:
        return LucideIcons.truck;
      case CategoryType.Snacks:
        return LucideIcons.candy;
      case CategoryType.Bakery:
        return LucideIcons.croissant;
      case CategoryType.Dessert:
        return LucideIcons.cake;

      // TRANSPORT
      case CategoryType.Fuel:
        return LucideIcons.fuel;
      case CategoryType.PublicTransport:
        return LucideIcons.bus;
      case CategoryType.Taxi:
        return LucideIcons.car;
      case CategoryType.Parking:
        return LucideIcons.parkingCircle;
      case CategoryType.VehicleMaintenance:
        return LucideIcons.wrench;
      case CategoryType.VehicleInsurance:
        return LucideIcons.shieldCheck;
      case CategoryType.CarWash:
        return LucideIcons.droplets;

      // UTILITIES / BILLS
      case CategoryType.Electricity:
        return LucideIcons.lightbulb;
      case CategoryType.Water:
        return LucideIcons.droplet;
      case CategoryType.Gas:
        return LucideIcons.flame;
      case CategoryType.Internet:
        return LucideIcons.wifi;
      case CategoryType.MobilePhone:
        return LucideIcons.smartphone;
      case CategoryType.Heating:
        return LucideIcons.flame;
      case CategoryType.TrashService:
        return LucideIcons.trash;
      case CategoryType.HomeMaintenance:
        return LucideIcons.hammer;
      case CategoryType.SecuritySystem:
        return LucideIcons.shield;

      // HOUSING
      case CategoryType.Rent:
        return LucideIcons.home;
      case CategoryType.Mortgage:
        return LucideIcons.home;
      case CategoryType.PropertyTax:
        return LucideIcons.receipt;
      case CategoryType.HOAFees:
        return LucideIcons.badgeDollarSign;
      case CategoryType.HomeInsurance:
        return LucideIcons.shieldCheck;

      // SHOPPING
      case CategoryType.Clothing:
        return LucideIcons.shirt;
      case CategoryType.Shoes:
        return LucideIcons.footprints;
      case CategoryType.Accessories:
        return LucideIcons.gem;
      case CategoryType.Electronics:
        return LucideIcons.monitor;
      case CategoryType.Furniture:
        return LucideIcons.sofa;
      case CategoryType.HomeDecor:
        return LucideIcons.home;
      case CategoryType.PersonalCare:
        return LucideIcons.sparkles;
      case CategoryType.Beauty:
        return LucideIcons.brush;

      // ENTERTAINMENT
      case CategoryType.Movies:
        return LucideIcons.film;
      case CategoryType.Music:
        return LucideIcons.music;
      case CategoryType.Gaming:
        return LucideIcons.gamepad2;
      case CategoryType.Streaming:
        return LucideIcons.tv;
      case CategoryType.Events:
        return LucideIcons.ticket;
      case CategoryType.Books:
        return LucideIcons.bookOpen;
      case CategoryType.Hobbies:
        return LucideIcons.palette;
      case CategoryType.Subscriptions:
        return LucideIcons.walletCards;

      // HEALTH
      case CategoryType.Healthcare:
        return LucideIcons.stethoscope;
      case CategoryType.Pharmacy:
        return LucideIcons.pill;
      case CategoryType.DentalCare:
        return LucideIcons.smile;
      case CategoryType.VisionCare:
        return LucideIcons.eye;
      case CategoryType.GymMembership:
        return LucideIcons.dumbbell;
      case CategoryType.Sports:
        return LucideIcons.heartPulse;
      case CategoryType.MentalHealth:
        return LucideIcons.brain;

      // EDUCATION & WORK
      case CategoryType.Education:
        return LucideIcons.graduationCap;
      case CategoryType.Tuition:
        return LucideIcons.receipt;
      case CategoryType.Courses:
        return LucideIcons.presentation;
      case CategoryType.OfficeSupplies:
        return LucideIcons.penTool;
      case CategoryType.Software:
        return LucideIcons.layers;

      // FINANCE
      case CategoryType.Savings:
        return LucideIcons.piggyBank;
      case CategoryType.Investments:
        return LucideIcons.trendingUp;
      case CategoryType.BankFees:
        return LucideIcons.badgeAlert;
      case CategoryType.LoanPayments:
        return LucideIcons.wallet;
      case CategoryType.Taxes:
        return LucideIcons.receipt;

      // TRAVEL
      case CategoryType.Flights:
        return LucideIcons.plane;
      case CategoryType.Hotels:
        return LucideIcons.bed;
      case CategoryType.CarRental:
        return LucideIcons.car;
      case CategoryType.TravelActivities:
        return LucideIcons.sun;
      case CategoryType.TravelInsurance:
        return LucideIcons.shield;

      // FAMILY & PETS
      case CategoryType.Childcare:
        return LucideIcons.baby;
      case CategoryType.PetCare:
        return LucideIcons.cat;
      case CategoryType.PetFood:
        return LucideIcons.bone;
      case CategoryType.VetBills:
        return LucideIcons.stethoscope;
      case CategoryType.FamilySupport:
        return LucideIcons.users;

      // GIFTS / CHARITY
      case CategoryType.Gifts:
        return LucideIcons.gift;
      case CategoryType.Donations:
        return LucideIcons.helpingHand;

      // OTHER
      case CategoryType.Miscellaneous:
        return LucideIcons.moreHorizontal;
    }
  }

  static String getGroupName(CategoryType type) {
    if (_foodAndDrinks.contains(type)) return 'Food & Drinks';
    if (_transportation.contains(type)) return 'Transportation';
    if (_billsUtilities.contains(type)) return 'Bills & Utilities';
    if (_housing.contains(type)) return 'Housing';
    if (_shopping.contains(type)) return 'Shopping';
    if (_entertainment.contains(type)) return 'Entertainment';
    if (_health.contains(type)) return 'Health';
    if (_educationWork.contains(type)) return 'Education & Work';
    if (_finance.contains(type)) return 'Finance';
    if (_travel.contains(type)) return 'Travel';
    if (_familyPets.contains(type)) return 'Family & Pets';
    if (_giftsCharity.contains(type)) return 'Gifts & Charity';
    return 'Other';
  }

  static final Set<CategoryType> _foodAndDrinks = {
    CategoryType.Groceries,
    CategoryType.Restaurants,
    CategoryType.Cafes,
    CategoryType.FastFood,
    CategoryType.Alcohol,
    CategoryType.Delivery,
    CategoryType.Snacks,
    CategoryType.Bakery,
    CategoryType.Dessert,
  };

  static final Set<CategoryType> _transportation = {
    CategoryType.Fuel,
    CategoryType.PublicTransport,
    CategoryType.Taxi,
    CategoryType.Parking,
    CategoryType.VehicleMaintenance,
    CategoryType.VehicleInsurance,
    CategoryType.CarWash,
  };

  static final Set<CategoryType> _billsUtilities = {
    CategoryType.Electricity,
    CategoryType.Water,
    CategoryType.Gas,
    CategoryType.Internet,
    CategoryType.MobilePhone,
    CategoryType.Heating,
    CategoryType.TrashService,
    CategoryType.HomeMaintenance,
    CategoryType.SecuritySystem,
  };

  static final Set<CategoryType> _housing = {
    CategoryType.Rent,
    CategoryType.Mortgage,
    CategoryType.PropertyTax,
    CategoryType.HOAFees,
    CategoryType.HomeInsurance,
  };

  static final Set<CategoryType> _shopping = {
    CategoryType.Clothing,
    CategoryType.Shoes,
    CategoryType.Accessories,
    CategoryType.Electronics,
    CategoryType.Furniture,
    CategoryType.HomeDecor,
    CategoryType.PersonalCare,
    CategoryType.Beauty,
  };

  static final Set<CategoryType> _entertainment = {
    CategoryType.Movies,
    CategoryType.Music,
    CategoryType.Gaming,
    CategoryType.Streaming,
    CategoryType.Events,
    CategoryType.Books,
    CategoryType.Hobbies,
    CategoryType.Subscriptions,
  };

  static final Set<CategoryType> _health = {
    CategoryType.Healthcare,
    CategoryType.Pharmacy,
    CategoryType.DentalCare,
    CategoryType.VisionCare,
    CategoryType.GymMembership,
    CategoryType.Sports,
    CategoryType.MentalHealth,
  };

  static final Set<CategoryType> _educationWork = {
    CategoryType.Education,
    CategoryType.Tuition,
    CategoryType.Courses,
    CategoryType.OfficeSupplies,
    CategoryType.Software,
  };

  static final Set<CategoryType> _finance = {
    CategoryType.Savings,
    CategoryType.Investments,
    CategoryType.BankFees,
    CategoryType.LoanPayments,
    CategoryType.Taxes,
  };

  static final Set<CategoryType> _travel = {
    CategoryType.Flights,
    CategoryType.Hotels,
    CategoryType.CarRental,
    CategoryType.TravelActivities,
    CategoryType.TravelInsurance,
  };

  static final Set<CategoryType> _familyPets = {
    CategoryType.Childcare,
    CategoryType.PetCare,
    CategoryType.PetFood,
    CategoryType.VetBills,
    CategoryType.FamilySupport,
  };

  static final Set<CategoryType> _giftsCharity = {
    CategoryType.Gifts,
    CategoryType.Donations,
  };
}
