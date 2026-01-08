import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:famxpense/models/Family/family_models.dart';

/// Displays a single family member card with profile information
///
/// Shows:
/// - Profile picture or initials avatar
/// - Full name and username
/// - Parent/Member role badge
/// - Birthday (if available)
///
/// Used in MembersListWidget to display each family member.
class MemberCard extends StatelessWidget {
  final FamilyMember member;

  const MemberCard({
    required this.member,
    super.key,
  });

  /// Generate initials avatar from full name
  String _getInitials(String fullName) {
    final parts = fullName.trim().split(' ');
    if (parts.isEmpty) return '?';
    if (parts.length == 1) return parts[0][0].toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  /// Format birthday to readable format (Jan 8, 1990)
  String _formatBirthday(DateTime birthDate) {
    return DateFormat('MMM d, yyyy').format(birthDate);
  }

  @override
  Widget build(BuildContext context) {
    final initials = _getInitials(member.fullName);

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          children: [
            // Profile picture or initials avatar
            if (member.profileImageUrl != null &&
                member.profileImageUrl!.isNotEmpty)
              CircleAvatar(
                radius: 32,
                backgroundImage: NetworkImage(member.profileImageUrl!),
                onBackgroundImageError: (exception, stackTrace) {
                  // Fallback to initials if image fails
                },
              )
            else
              CircleAvatar(
                radius: 32,
                backgroundColor: Colors.blue[300],
                child: Text(
                  initials,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: 16,
                  ),
                ),
              ),
            const SizedBox(width: 16),

            // Member details
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Full name and parent badge
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          member.fullName,
                          style:
                              Theme.of(context).textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w600,
                                  ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      const SizedBox(width: 8),
                      if (member.isParent)
                        Chip(
                          label: const Text(
                            'Parent',
                            style: TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          backgroundColor: Colors.orange[100],
                          side: BorderSide(
                            color: Colors.orange[400]!,
                            width: 1,
                          ),
                          padding: const EdgeInsets.symmetric(
                            horizontal: 8,
                            vertical: 2,
                          ),
                        ),
                    ],
                  ),
                  const SizedBox(height: 4),

                  // Username (if available)
                  if (member.userName != null && member.userName!.isNotEmpty)
                    Text(
                      '@${member.userName}',
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                            color: Colors.grey[600],
                          ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  const SizedBox(height: 4),

                  // Birthday (if available)
                  if (member.birthDate != null)
                    Row(
                      children: [
                        Icon(
                          Icons.cake,
                          size: 14,
                          color: Colors.grey[500],
                        ),
                        const SizedBox(width: 4),
                        Text(
                          _formatBirthday(member.birthDate!),
                          style: Theme.of(context).textTheme.bodySmall?.copyWith(
                                color: Colors.grey[600],
                              ),
                        ),
                      ],
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
