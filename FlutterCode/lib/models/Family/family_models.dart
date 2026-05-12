import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:equatable/equatable.dart';

class FamilyMember extends Equatable {
  final String userId;
  final String fullName;
  final String? userName;
  final DateTime? birthDate;
  final bool? isMale;
  final String? profileImageUrl;
  final bool isParent;

  const FamilyMember({
    required this.userId,
    required this.fullName,
    this.userName,
    this.birthDate,
    this.isMale,
    this.profileImageUrl,
    required this.isParent,
  });

  factory FamilyMember.fromJson(Map<String, dynamic> json) {
    return FamilyMember(
      userId: json['userId'] as String,
      fullName: json['fullName'] as String,
      userName: json['userName'] as String?,
      birthDate: json['birthDate'] != null
          ? DateTime.parse(json['birthDate'] as String)
          : null,
      isMale: json['isMale'] as bool?,
      profileImageUrl: json['profileImageUrl'] as String?,
      isParent: json['isParent'] as bool? ?? false,
    );
  }

  @override
  List<Object?> get props => [
    userId,
    fullName,
    userName,
    birthDate,
    isMale,
    profileImageUrl,
    isParent,
  ];
}

class FamilyDetails extends Equatable {
  final String id;
  final String name;
  final double currentBudget;
  final String? familyBio;
  final List<FamilyMember> members;

  const FamilyDetails({
    required this.id,
    required this.name,
    required this.currentBudget,
    this.familyBio,
    required this.members,
  });

  factory FamilyDetails.fromJson(Map<String, dynamic> json) {
    return FamilyDetails(
      id: json['id'] as String,
      name: json['name'] as String,
      currentBudget: (json['currentBudget'] as num).toDouble(),
      familyBio: json['familyBio'] as String?,
      members: (json['members'] as List?)
          ?.map((m) => FamilyMember.fromJson(m as Map<String, dynamic>))
          .toList() ??
          [],
    );
  }

  @override
  List<Object?> get props => [id, name, currentBudget, familyBio, members];
}

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
      familyId: json['familyId'] as String,
      familyName: json['familyName'] as String,
      isParent: json['isParent'] as bool,
      currentBudget: json['currentBudget'] != null
          ? (json['currentBudget'] as num).toDouble()
          : null,
    );
  }
}

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
