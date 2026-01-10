import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

/// Reusable card widget displaying a single invitation with appropriate action buttons
class InvitationCard extends StatelessWidget {
  final Invitation invitation;
  final VoidCallback? onAccept;
  final VoidCallback? onDecline;
  final VoidCallback? onCancel;
  final bool isLoading;

  const InvitationCard({
    super.key,
    required this.invitation,
    this.onAccept,
    this.onDecline,
    this.onCancel,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    final isSent = onCancel != null || (onAccept == null && onDecline == null);
    final statusColor = _getStatusColor(invitation.status);

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: AppColors.border,
          width: 1,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        isSent
                            ? l10n.toLabel(invitation.inviteeEmail)
                            : l10n.fromLabel(invitation.inviterName),
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w700,
                          color: AppColors.textPrimary,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        l10n.joinFamily(invitation.familyName),
                        style: const TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w500,
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 12),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                  decoration: BoxDecoration(
                    color: statusColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                      color: statusColor.withOpacity(0.3),
                      width: 1,
                    ),
                  ),
                  child: Text(
                    invitation.status.name.toUpperCase(),
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
                      color: statusColor,
                      letterSpacing: 0.5,
                    ),
                  ),
                ),
              ],
            ),
            
            const Padding(
                padding: EdgeInsets.symmetric(vertical: 16),
                child: Divider(height: 1, color: AppColors.border),
            ),

            Row(
              children: [
                Icon(
                  Icons.person_outline_rounded, 
                  size: 16, 
                  color: AppColors.textSecondary.withOpacity(0.7)
                ),
                const SizedBox(width: 6),
                Text(
                  invitation.isParent ? l10n.roleParent : l10n.roleMember,
                  style: TextStyle(
                    fontSize: 13,
                    color: AppColors.textSecondary.withOpacity(0.8),
                    fontWeight: FontWeight.w500,
                  ),
                ),
                const Spacer(),
                Icon(
                  Icons.access_time_rounded, 
                  size: 16, 
                  color: AppColors.textSecondary.withOpacity(0.7)
                ),
                 const SizedBox(width: 6),
                Text(
                  _getFormattedDate(invitation.sentAtUtc),
                  style: TextStyle(
                    fontSize: 13,
                    color: AppColors.textSecondary.withOpacity(0.8),
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),

            if (onAccept != null || onDecline != null || (onCancel != null && invitation.status == InvitationStatus.pending)) ...[
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  if (onCancel != null && invitation.status == InvitationStatus.pending)
                     Expanded(child: _buildActionButton(
                      context,
                      label: l10n.cancelInvite,
                      onPressed: onCancel!,
                      isPrimary: false,
                      isLoading: isLoading,
                      color: AppColors.error,
                      icon: Icons.close_rounded,
                    )),

                  if (onDecline != null) ...[
                     Expanded(child: _buildActionButton(
                      context,
                      label: l10n.decline,
                      onPressed: onDecline!,
                      isPrimary: false,
                      isLoading: isLoading,
                      color: AppColors.textSecondary, 
                      icon: Icons.close_rounded,
                    )),
                    const SizedBox(width: 12),
                  ],

                  if (onAccept != null)
                     Expanded(child: _buildActionButton(
                      context,
                      label: l10n.accept,
                      onPressed: onAccept!,
                      isPrimary: true,
                      isLoading: isLoading,
                      color: AppColors.success,
                      icon: Icons.check_rounded,
                    )),
                  
                ],
              ),
            ]
          ],
        ),
      ),
    );
  }

  Widget _buildActionButton(
    BuildContext context, {
    required String label,
    required VoidCallback onPressed,
    required bool isPrimary,
    required bool isLoading,
    required Color color,
    required IconData icon,
  }) {
    return SizedBox(
      height: 44,
      child: ElevatedButton(
        onPressed: isLoading ? null : onPressed,
        style: ElevatedButton.styleFrom(
          backgroundColor: isPrimary ? AppColors.primary : Colors.white,
          foregroundColor: isPrimary ? Colors.white : AppColors.textPrimary,
          elevation: isPrimary ? 2 : 0,
          shadowColor: isPrimary ? AppColors.primary.withOpacity(0.3) : Colors.transparent,
          side: isPrimary 
              ? BorderSide.none 
              : BorderSide(color: AppColors.border, width: 1),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(10),
          ),
          padding: EdgeInsets.zero,
        ),
        child: isLoading
            ? SizedBox(
                height: 18,
                width: 18,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation(
                    isPrimary ? Colors.white : AppColors.textSecondary,
                  ),
                ),
              )
            : Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  if (!isPrimary) ...[
                      Icon(icon, size: 18, color: color),
                       const SizedBox(width: 8),
                  ],
                  Text(
                    label,
                    style: TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: isPrimary ? Colors.white : color,
                    ),
                  ),
                   if (isPrimary) ...[
                       const SizedBox(width: 8),
                      Icon(icon, size: 18, color: Colors.white),
                  ],
                ],
              ),
      ),
    );
  }

  
  Color _getStatusColor(InvitationStatus status) {
    switch (status) {
      case InvitationStatus.pending:
        return Colors.orange.shade700;
      case InvitationStatus.accepted:
        return AppColors.success;
      case InvitationStatus.declined:
        return AppColors.error;
      case InvitationStatus.cancelled:
        return AppColors.textSecondary;
    }
  }

  String _getFormattedDate(DateTime utcDateTime) {
    final localDateTime = utcDateTime.toLocal();
    return DateFormat('MMM dd, yyyy').format(localDateTime);
  }
}
