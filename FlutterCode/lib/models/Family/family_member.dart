class FamilyMember {
  final String userId;
  final String fullName;
  final String? profileImageUrl;
  final bool isParent;

  FamilyMember({
    required this.userId,
    required this.fullName,
    this.profileImageUrl,
    required this.isParent,
  });

  factory FamilyMember.fromJson(Map<String, dynamic> json) {
    return FamilyMember(
      userId: json['userId'],
      fullName: json['fullName'],
      profileImageUrl: json['profileImageUrl'],
      isParent: json['isParent'],
    );
  }
}
