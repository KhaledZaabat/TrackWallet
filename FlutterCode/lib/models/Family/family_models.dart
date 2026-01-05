class FamilyListResult {
  final bool isSuccess;
  final String? errorMessage;
  final List<FamilyData>? families;

  FamilyListResult._({
    required this.isSuccess,
    this.errorMessage,
    this.families,
  });

  factory FamilyListResult.success({required List<FamilyData> families}) {
    return FamilyListResult._(
      isSuccess: true,
      families: families,
    );
  }

  factory FamilyListResult.failure(String message) {
    return FamilyListResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class FamilyData {
  final String id;
  final String name;
  final double currentBudget;
  final String? familyBio;
  final List<FamilyMemberProfile>? members;

  FamilyData({
    required this.id,
    required this.name,
    required this.currentBudget,
    this.familyBio,
    this.members,
  });

  factory FamilyData.fromJson(Map<String, dynamic> json) {
    return FamilyData(
      id: json['id'],
      name: json['name'],
      currentBudget: (json['currentBudget'] as num).toDouble(),
      familyBio: json['familyBio'],
      members: (json['members'] as List?)
          ?.map((m) => FamilyMemberProfile.fromJson(m))
          .toList(),
    );
  }
}

class FamilyMemberProfile {
  final String userId;
  final String fullName;
  final String? profileImageUrl;
  final bool isParent;

  FamilyMemberProfile({
    required this.userId,
    required this.fullName,
    this.profileImageUrl,
    required this.isParent,
  });

  factory FamilyMemberProfile.fromJson(Map<String, dynamic> json) {
    return FamilyMemberProfile(
      userId: json['userId'],
      fullName: json['fullName'],
      profileImageUrl: json['profileImageUrl'],
      isParent: json['isParent'],
    );
  }
}

// ========== Select Family Models ==========

class SelectFamilyResult {
  final bool isSuccess;
  final String? errorMessage;
  final SelectFamilyData? data;

  SelectFamilyResult._({
    required this.isSuccess,
    this.errorMessage,
    this.data,
  });

  factory SelectFamilyResult.success({
    required String userId,
    required String email,
    required String fullName,
    String? profileImageUrl,
    required FamilyContext familyContext,
    List<BudgetHistoryItem>? budgetHistory,
    List<TransactionItem>? recentTransactions,
  }) {
    return SelectFamilyResult._(
      isSuccess: true,
      data: SelectFamilyData(
        userId: userId,
        email: email,
        fullName: fullName,
        profileImageUrl: profileImageUrl,
        familyContext: familyContext,
        budgetHistory: budgetHistory ?? [],
        recentTransactions: recentTransactions ?? [],
      ),
    );
  }

  factory SelectFamilyResult.failure(String message) {
    return SelectFamilyResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class SelectFamilyData {
  final String userId;
  final String email;
  final String fullName;
  final String? profileImageUrl;
  final FamilyContext familyContext;
  final List<BudgetHistoryItem> budgetHistory;
  final List<TransactionItem> recentTransactions;

  SelectFamilyData({
    required this.userId,
    required this.email,
    required this.fullName,
    this.profileImageUrl,
    required this.familyContext,
    required this.budgetHistory,
    required this.recentTransactions,
  });
}

// ========== Family Context ==========

class FamilyContext {
  final String familyId;
  final String familyName;
  final bool isParent;
  final double? currentBudget;

  FamilyContext({
    required this.familyId,
    required this.familyName,
    required this.isParent,
    this.currentBudget,
  });

  factory FamilyContext.fromJson(Map<String, dynamic> json) {
    return FamilyContext(
      familyId: json['familyId'],
      familyName: json['familyName'],
      isParent: json['isParent'],
      currentBudget: json['currentBudget'] != null
          ? (json['currentBudget'] as num).toDouble()
          : null,
    );
  }
}

// ========== Budget History ==========

class BudgetHistoryItem {
  final double budget;
  final DateTime recordedAtUtc;

  BudgetHistoryItem({
    required this.budget,
    required this.recordedAtUtc,
  });

  factory BudgetHistoryItem.fromJson(Map<String, dynamic> json) {
    return BudgetHistoryItem(
      budget: (json['budget'] as num).toDouble(),
      recordedAtUtc: DateTime.parse(json['recordedAtUtc']),
    );
  }
}

// ========== Transaction Models ==========

class TransactionItem {
  final String transactionId;
  final String? title;
  final double amount;
  final String type; // "Income" or "Expense"
  final DateTime transactedOn;
  final DateTime createdAtUtc;
  final CategoryResponse category;
  final CreatorResponse creator;

  TransactionItem({
    required this.transactionId,
    this.title,
    required this.amount,
    required this.type,
    required this.transactedOn,
    required this.createdAtUtc,
    required this.category,
    required this.creator,
  });

  factory TransactionItem.fromJson(Map<String, dynamic> json) {
    return TransactionItem(
      transactionId: json['transactionId'],
      title: json['title'],
      amount: (json['amount'] as num).toDouble(),
      type: json['type'],
      transactedOn: DateTime.parse(json['transactedOn']),
      createdAtUtc: DateTime.parse(json['createdAtUtc']),
      category: CategoryResponse.fromJson(json['category']),
      creator: CreatorResponse.fromJson(json['creator']),
    );
  }
}

class CategoryResponse {
  final String categoryId;
  final String name;

  CategoryResponse({
    required this.categoryId,
    required this.name,
  });

  factory CategoryResponse.fromJson(Map<String, dynamic> json) {
    return CategoryResponse(
      categoryId: json['categoryId'],
      name: json['name'],
    );
  }
}

class CreatorResponse {
  final String userId;
  final String? fullName;
  final String? profileImageUrl;

  CreatorResponse({
    required this.userId,
    this.fullName,
    this.profileImageUrl,
  });

  factory CreatorResponse.fromJson(Map<String, dynamic> json) {
    return CreatorResponse(
      userId: json['userId'],
      fullName: json['fullName'],
      profileImageUrl: json['profileImageUrl'],
    );
  }
}
