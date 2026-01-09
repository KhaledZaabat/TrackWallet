import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/models/Family/family_models.dart';
import 'package:famxpense/features/MyFamily/widgets/member_card.dart';

/// Displays a list of family members using MemberCard widgets
/// Includes section header and handles empty state
class MembersListWidget extends StatelessWidget {
  final List<FamilyMember> members;
  final bool isCurrentUserParent;
  final String? currentUserId;
  final String? operationInProgress;
  final Function(String userId)? onKickMember;

  const MembersListWidget({
    required this.members,
    this.isCurrentUserParent = false,
    this.currentUserId,
    this.operationInProgress,
    this.onKickMember,
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    // Handle empty state
    if (members.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.people_outline_rounded,
                size: 64,
                color: AppColors.textSecondary.withOpacity(0.3),
              ),
              const SizedBox(height: 16),
              Text(
                'No family members yet',
                style: GoogleFonts.inter(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: AppColors.textSecondary,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                'Invite members to see them here',
                style: GoogleFonts.inter(
                  fontSize: 14,
                  color: AppColors.textSecondary.withOpacity(0.7),
                ),
              ),
            ],
          ),
        ),
      );
    }

    // Sort members: Parents first, then alphabetically
    final sortedMembers = List<FamilyMember>.from(members)
      ..sort((a, b) {
        if (a.isParent && !b.isParent) return -1;
        if (!a.isParent && b.isParent) return 1;
        return a.fullName.compareTo(b.fullName);
      });

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Section header
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
          child: Row(
            children: [
              Icon(
                Icons.people_rounded,
                size: 18,
                color: AppColors.textSecondary,
              ),
              const SizedBox(width: 8),
              Text(
                'Members',
                style: GoogleFonts.inter(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: AppColors.textSecondary,
                ),
              ),
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                  color: AppColors.primary.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(
                  '${members.length}',
                  style: GoogleFonts.inter(
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                    color: AppColors.primary,
                  ),
                ),
              ),
            ],
          ),
        ),
        // Member cards
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: sortedMembers.length,
          itemBuilder: (context, index) {
            final member = sortedMembers[index];
            return MemberCard(
              member: member,
              isCurrentUserParent: isCurrentUserParent,
              isCurrentUser: member.userId == currentUserId,
              isOperationInProgress: operationInProgress == member.userId,
              onKickPressed: () => onKickMember?.call(member.userId),
            );
          },
        ),
      ],
    );
  }
}
