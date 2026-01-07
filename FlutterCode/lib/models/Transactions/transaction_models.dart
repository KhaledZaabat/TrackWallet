// models/transaction/transaction_models.dart

/// Transaction type enum
enum TransactionType {
  Income,
  Expense;

  String toJson() => name;

  static TransactionType fromJson(String value) {
    return TransactionType.values.firstWhere(
      (type) => type.name == value,
      orElse: () => TransactionType.Expense,
    );
  }
}

/// Creator info
class TransactionCreator {
  final String userId;
  final String? fullName;
  final String? profileImageUrl;

  TransactionCreator({
    required this.userId,
    this.fullName,
    this.profileImageUrl,
  });

  factory TransactionCreator.fromJson(Map<String, dynamic> json) {
    return TransactionCreator(
      userId: json['userId'] as String,
      fullName: json['fullName'] as String?,
      profileImageUrl: json['profileImageUrl'] as String?,
    );
  }

  Map<String, dynamic> toJson() => {
        'userId': userId,
        'fullName': fullName,
        'profileImageUrl': profileImageUrl,
      };
}

/// Transaction category info
class TransactionCategory {
  final String categoryId;
  final String name;

  TransactionCategory({
    required this.categoryId,
    required this.name,
  });

  factory TransactionCategory.fromJson(Map<String, dynamic> json) {
    return TransactionCategory(
      categoryId: json['categoryId'] as String,
      name: json['name'] as String,
    );
  }

  Map<String, dynamic> toJson() => {
        'categoryId': categoryId,
        'name': name,
      };
}

/// Transaction item from API
class TransactionItem {
  final String transactionId;
  final String? title;
  final double amount;
  final TransactionType type;
  final DateTime transactedOn;
  final DateTime createdAtUtc;
  final TransactionCategory category;
  final TransactionCreator creator;
  final String? notes;

  TransactionItem({
    required this.transactionId,
    this.title,
    required this.amount,
    required this.type,
    required this.transactedOn,
    required this.createdAtUtc,
    required this.category,
    required this.creator,
    this.notes,
  });

  factory TransactionItem.fromJson(Map<String, dynamic> json) {
    return TransactionItem(
      transactionId: json['transactionId'] as String,
      title: json['title'] as String?,
      amount: (json['amount'] as num).toDouble(),
      type: TransactionType.fromJson(json['type'] as String),
      transactedOn: DateTime.parse(json['transactedOn'] as String),
      createdAtUtc: DateTime.parse(json['createdAtUtc'] as String),
      category: TransactionCategory.fromJson(
          json['category'] as Map<String, dynamic>),
      creator:
          TransactionCreator.fromJson(json['creator'] as Map<String, dynamic>),
      notes: json['notes'] as String?,
    );
  }

  Map<String, dynamic> toJson() => {
        'transactionId': transactionId,
        'title': title,
        'amount': amount,
        'type': type.toJson(),
        'transactedOn': transactedOn.toIso8601String().split('T')[0],
        'createdAtUtc': createdAtUtc.toIso8601String(),
        'category': category.toJson(),
        'creator': creator.toJson(),
        'notes': notes,
      };

  bool get isIncome => type == TransactionType.Income;
  bool get isExpense => type == TransactionType.Expense;

  @override
  String toString() =>
      'Transaction(id: $transactionId, title: $title, amount: $amount, type: $type)';
}

/// Paginated response for transactions
class TransactionPagedResponse {
  final List<TransactionItem> items;
  final String? nextCursor;
  final bool hasNextPage;

  TransactionPagedResponse({
    required this.items,
    this.nextCursor,
    required this.hasNextPage,
  });

  factory TransactionPagedResponse.fromJson(Map<String, dynamic> json) {
    return TransactionPagedResponse(
      items: (json['items'] as List)
          .map((item) => TransactionItem.fromJson(item as Map<String, dynamic>))
          .toList(),
      nextCursor: json['nextCursor'] as String?,
      hasNextPage: json['hasNextPage'] as bool,
    );
  }

  Map<String, dynamic> toJson() => {
        'items': items.map((item) => item.toJson()).toList(),
        'nextCursor': nextCursor,
        'hasNextPage': hasNextPage,
      };
}

/// Request to create transaction
class CreateTransactionRequest {
  final TransactionType type;
  final String categoryId;
  final double amount;
  final DateTime transactedOn;
  final String? title;
  final String? notes;

  CreateTransactionRequest({
    required this.type,
    required this.categoryId,
    required this.amount,
    required this.transactedOn,
    this.title,
    this.notes,
  });

  Map<String, dynamic> toJson() => {
        'type': type.toJson(),
        'categoryId': categoryId,
        'amount': amount,
        'transactedOn': transactedOn.toIso8601String().split('T')[0],
        'title': title,
        'notes': notes,
      };
}

/// Request to update transaction
class UpdateTransactionRequest {
  final TransactionType type;
  final double? amount;
  final DateTime? transactedOn;
  final String? title;
  final String? notes;
  final String? categoryId;

  UpdateTransactionRequest({
    required this.type,
    this.amount,
    this.transactedOn,
    this.title,
    this.notes,
    this.categoryId,
  });

  Map<String, dynamic> toJson() => {
        'type': type.toJson(),
        'amount': amount,
        'transactedOn': transactedOn?.toIso8601String().split('T')[0],
        'title': title,
        'notes': notes,
        'categoryId': categoryId,
      };
}

class TransactionFilters {
  final TransactionType? transactionType;
  final String? categoryType;
  final double? minAmount;
  final double? maxAmount;
  final String? creatorId;

  const TransactionFilters({
    this.transactionType,
    this.categoryType,
    this.minAmount,
    this.maxAmount,
    this.creatorId,
  });

  /// Check if any filters are active
  bool get hasActiveFilters =>
      transactionType != null ||
      categoryType != null ||
      minAmount != null ||
      maxAmount != null ||
      creatorId != null;

  /// Get count of active filters
  int get activeFilterCount {
    int count = 0;
    if (transactionType != null) count++;
    if (categoryType != null) count++;
    if (minAmount != null || maxAmount != null) count++;
    if (creatorId != null) count++;
    return count;
  }

  /// Create empty filters
  factory TransactionFilters.empty() {
    return const TransactionFilters();
  }

  /// Copy with method
  TransactionFilters copyWith({
    TransactionType? Function()? transactionType,
    String? Function()? categoryType,
    double? Function()? minAmount,
    double? Function()? maxAmount,
    String? Function()? creatorId,
  }) {
    return TransactionFilters(
      transactionType:
          transactionType != null ? transactionType() : this.transactionType,
      categoryType: categoryType != null ? categoryType() : this.categoryType,
      minAmount: minAmount != null ? minAmount() : this.minAmount,
      maxAmount: maxAmount != null ? maxAmount() : this.maxAmount,
      creatorId: creatorId != null ? creatorId() : this.creatorId,
    );
  }

  /// Convert to query parameters for API
  Map<String, dynamic> toQueryParameters() {
    final Map<String, dynamic> params = {};

    if (transactionType != null) {
      params['transactionType'] = transactionType!.name;
    }
    if (categoryType != null) {
      params['categoryType'] = categoryType;
    }
    if (minAmount != null) {
      params['minAmount'] = minAmount;
    }
    if (maxAmount != null) {
      params['maxAmount'] = maxAmount;
    }
    if (creatorId != null) {
      params['creatorId'] = creatorId;
    }

    return params;
  }

  @override
  String toString() {
    return 'TransactionFilters(type: $transactionType, category: $categoryType, min: $minAmount, max: $maxAmount, creator: $creatorId)';
  }
}

/// Model for family user
class FamilyUser {
  final String userId;
  final String? fullName;

  const FamilyUser({
    required this.userId,
    this.fullName,
  });

  factory FamilyUser.fromJson(Map<String, dynamic> json) {
    return FamilyUser(
      userId: json['userId'] as String,
      fullName: json['fullName'] as String?,
    );
  }

  String get displayName => fullName ?? 'Unknown User';
}
