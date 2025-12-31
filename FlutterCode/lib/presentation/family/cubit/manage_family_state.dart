import 'package:equatable/equatable.dart';
import 'package:famxpense/domain/entities/family.dart';

class FamilyMember extends Equatable {
  final String id;
  final String name;
  final String? avatarUrl;
  final bool isParent;

  const FamilyMember({
    required this.id,
    required this.name,
    this.avatarUrl,
    required this.isParent,
  });

  @override
  List<Object?> get props => [id, name, avatarUrl, isParent];
}

class ManageFamilyState extends Equatable {
  final bool isLoading;
  final String? error;
  final Family? family;
  final double incomeTotal;
  final double expenseTotal;
  final List<FamilyMember> members;

  const ManageFamilyState({
    this.isLoading = false,
    this.error,
    this.family,
    this.incomeTotal = 0,
    this.expenseTotal = 0,
    this.members = const [],
  });

  double get currentBudget => incomeTotal - expenseTotal;

  ManageFamilyState copyWith({
    bool? isLoading,
    String? error,
    Family? family,
    double? incomeTotal,
    double? expenseTotal,
    List<FamilyMember>? members,
  }) {
    return ManageFamilyState(
      isLoading: isLoading ?? this.isLoading,
      error: error,
      family: family ?? this.family,
      incomeTotal: incomeTotal ?? this.incomeTotal,
      expenseTotal: expenseTotal ?? this.expenseTotal,
      members: members ?? this.members,
    );
  }

  @override
  List<Object?> get props =>
      [isLoading, error, family, incomeTotal, expenseTotal, members];
}
