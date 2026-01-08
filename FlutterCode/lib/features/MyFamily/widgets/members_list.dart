import 'package:flutter/material.dart';
import 'package:famxpense/models/Family/family_models.dart';
import 'package:famxpense/features/MyFamily/widgets/member_card.dart';

/// Displays a list of family members using MemberCard widgets
///
/// Handles:
/// - Empty state when no members (unlikely but handled)
/// - ListView of member cards for scrollable list
/// - Proper spacing and layout
///
/// Used in MyFamily page to display all family members.
class MembersListWidget extends StatelessWidget {
  final List<FamilyMember> members;

  const MembersListWidget({
    required this.members,
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    // Handle empty state (unlikely but good practice)
    if (members.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.people_outline,
                size: 64,
                color: Colors.grey[400],
              ),
              const SizedBox(height: 16),
              Text(
                'No family members yet',
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      color: Colors.grey[600],
                    ),
              ),
              const SizedBox(height: 8),
              Text(
                'Invite members to see them here',
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Colors.grey[500],
                    ),
              ),
            ],
          ),
        ),
      );
    }

    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: members.length,
      itemBuilder: (context, index) {
        return MemberCard(member: members[index]);
      },
    );
  }
}
