import 'package:famxpense/models/Family/family_member.dart';

class FamilyInfo {
  final String id;
  final String name;
  final double currentBudget;
  final String? familyBio;
  final List<FamilyMember>? members;

  FamilyInfo({
    required this.id,
    required this.name,
    required this.currentBudget,
    this.familyBio,
    this.members,
  });

  factory FamilyInfo.fromJson(Map<String, dynamic> json) {
    return FamilyInfo(
      id: json['id'],
      name: json['name'],
      currentBudget: (json['currentBudget'] as num).toDouble(),
      familyBio: json['familyBio'],
      members: (json['members'] as List?)
          ?.map((m) => FamilyMember.fromJson(m))
          .toList(),
    );
  }
}
