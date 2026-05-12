import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/models/Family/family_models.dart';
import 'package:famxpense/l10n/app_localizations.dart';

class MemberCard extends StatelessWidget {
  final FamilyMember member;
  final bool isCurrentUserParent;
  final bool isCurrentUser;
  final bool isOperationInProgress;
  final VoidCallback? onKickPressed;

  const MemberCard({
    required this.member,
    this.isCurrentUserParent = false,
    this.isCurrentUser = false,
    this.isOperationInProgress = false,
    this.onKickPressed,
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

  /// Check if this member can be kicked
  /// - Only non-parents can be kicked
  /// - Cannot kick yourself
  bool get _canBeKicked => isCurrentUserParent && !member.isParent && !isCurrentUser;

  void _showKickConfirmation(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        backgroundColor: AppColors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
        title: Row(
          children: [
            Icon(
              Icons.person_remove_rounded,
              color: AppColors.error,
              size: 28,
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                l10n.removeMember,
                style: GoogleFonts.inter(
                  fontWeight: FontWeight.w700,
                  fontSize: 18,
                ),
              ),
            ),
          ],
        ),
        content: Text(
          l10n.removeMemberConfirmMessage(member.fullName),
          style: GoogleFonts.inter(
            fontSize: 14,
            color: AppColors.textSecondary,
            height: 1.5,
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: Text(
              l10n.cancel,
              style: GoogleFonts.inter(
                color: AppColors.textSecondary,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(dialogContext);
              onKickPressed?.call();
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10),
              ),
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
            ),
            child: Text(
              l10n.remove,
              style: GoogleFonts.inter(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final initials = _getInitials(member.fullName);

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.03),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Row(
          children: [
            Stack(
              children: [
                if (member.profileImageUrl != null &&
                    member.profileImageUrl!.isNotEmpty)
                  CircleAvatar(
                    radius: 28,
                    backgroundImage: NetworkImage(member.profileImageUrl!),
                    onBackgroundImageError: (exception, stackTrace) {},
                  )
                else
                  CircleAvatar(
                    radius: 28,
                    backgroundColor: AppColors.primary,
                    child: Text(
                      initials,
                      style: GoogleFonts.inter(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 15,
                      ),
                    ),
                  ),
                if (isCurrentUser)
                  Positioned(
                    bottom: 0,
                    right: 0,
                    child: Container(
                      padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
                      decoration: BoxDecoration(
                        color: AppColors.success,
                        borderRadius: BorderRadius.circular(6),
                        border: Border.all(color: AppColors.surface, width: 2),
                      ),
                      child: Text(
                        l10n.you,
                        style: GoogleFonts.inter(
                          color: Colors.white,
                          fontSize: 8,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
            const SizedBox(width: 14),

            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          member.fullName,
                          style: GoogleFonts.inter(
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                            color: AppColors.textPrimary,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      if (member.isParent)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 8,
                            vertical: 3,
                          ),
                          decoration: BoxDecoration(
                            color: AppColors.primary.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(6),
                            border: Border.all(
                              color: AppColors.primary.withOpacity(0.3),
                            ),
                          ),
                          child: Text(
                            l10n.parent,
                            style: GoogleFonts.inter(
                              fontSize: 11,
                              fontWeight: FontWeight.w600,
                              color: AppColors.primary,
                            ),
                          ),
                        ),
                    ],
                  ),
                  const SizedBox(height: 4),

                  if (member.userName != null && member.userName!.isNotEmpty)
                    Text(
                      '@${member.userName}',
                      style: GoogleFonts.inter(
                        fontSize: 13,
                        color: AppColors.textSecondary,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),

                  if (member.birthDate != null) ...[
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Icon(
                          Icons.cake_rounded,
                          size: 14,
                          color: AppColors.textSecondary.withOpacity(0.6),
                        ),
                        const SizedBox(width: 4),
                        Text(
                          _formatBirthday(member.birthDate!),
                          style: GoogleFonts.inter(
                            fontSize: 12,
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ],
                ],
              ),
            ),

            if (_canBeKicked) ...[
              const SizedBox(width: 8),
              isOperationInProgress
                  ? const SizedBox(
                      height: 24,
                      width: 24,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        valueColor: AlwaysStoppedAnimation<Color>(AppColors.error),
                      ),
                    )
                  : IconButton(
                      onPressed: () => _showKickConfirmation(context),
                      style: IconButton.styleFrom(
                        backgroundColor: AppColors.error.withOpacity(0.08),
                        padding: const EdgeInsets.all(8),
                      ),
                      icon: Icon(
                        Icons.person_remove_rounded,
                        color: AppColors.error,
                        size: 20,
                      ),
                      tooltip: l10n.removeMember,
                    ),
            ],
          ],
        ),
      ),
    );
  }
}
