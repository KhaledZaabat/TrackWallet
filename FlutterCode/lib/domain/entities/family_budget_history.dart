import 'package:uuid/v8.dart';

class FamilyBudgetHistory {
  final String id;
  final String familyId;
  final double budget;
  final DateTime recordedAt;

  FamilyBudgetHistory._({
    required this.id,
    required this.familyId,
    required this.budget,
    required this.recordedAt,
  });

  static FamilyBudgetHistory create({
    required String familyId,
    required double budget,
    DateTime? date,
  }) {
    final DateTime today = date != null
        ? DateTime(date.year, date.month, date.day)
        : DateTime.now();

    return FamilyBudgetHistory._(
      id: const UuidV8().generate(),
      familyId: familyId,
      budget: budget,
      recordedAt: today,
    );
  }

  static FamilyBudgetHistory fromJson(Map<String, dynamic> json) {
    return FamilyBudgetHistory._(
      id: json["id"],
      familyId: json["familyId"],
      budget: (json["budget"] as num).toDouble(),
      recordedAt: DateTime.parse(json["recordedAt"]),
    );
  }

  Map<String, dynamic> toJson() => {
        "id": id,
        "familyId": familyId,
        "budget": budget,
        "recordedAt": formatDateOnly(recordedAt),
      };

  static String formatDateOnly(DateTime d) =>
      "${d.year.toString().padLeft(4, '0')}-"
      "${d.month.toString().padLeft(2, '0')}-"
      "${d.day.toString().padLeft(2, '0')}";
}
