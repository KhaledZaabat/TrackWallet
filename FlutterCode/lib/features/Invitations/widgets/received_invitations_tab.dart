import 'package:flutter/material.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'invitation_card.dart';

/// Display received invitations with Accept/Decline buttons
///
/// This tab shows all received invitations filtered to Pending status only.
/// Users can accept or decline each invitation with real-time loading feedback.
///
/// Props:
/// - [invitations]: All received invitations from cubit state
/// - [loadingInvitationId]: ID of invitation being processed (null when idle)
/// - [onAccept]: Callback when accept button tapped (receives invitation ID)
/// - [onDecline]: Callback when decline button tapped (receives invitation ID)
///
/// Loading State Control:
/// - Each card receives isLoading = (loadingInvitationId == invitation.id)
/// - Only the card being acted on shows loading state
/// - Other invitations remain interactive
class ReceivedInvitationsTab extends StatelessWidget {
  final List<Invitation> invitations;
  final String? loadingInvitationId;
  final Function(String) onAccept;
  final Function(String) onDecline;
  final Future<void> Function() onRefresh;

  const ReceivedInvitationsTab({
    required this.invitations,
    required this.loadingInvitationId,
    required this.onAccept,
    required this.onDecline,
    required this.onRefresh,
  });

  @override
  Widget build(BuildContext context) {
    // Filter to Pending invitations only
    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
        // Sort by sentAtUtc descending (newest first)
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    return RefreshIndicator(
      onRefresh: onRefresh,
      color: AppColors.primary,
      child: pending.isEmpty
          ? LayoutBuilder(
              builder: (context, constraints) {
                return SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  child: Container(
                    height: constraints.maxHeight,
                    alignment: Alignment.center,
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.mail_outline_rounded,
                          size: 64,
                          color: AppColors.textSecondary.withOpacity(0.3),
                        ),
                        const SizedBox(height: 16),
                        Text(
                          'No pending invitations',
                          style: TextStyle(
                            fontSize: 16,
                            color: AppColors.textSecondary.withOpacity(0.6),
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: 8),
                         Text(
                          'Pull to refresh',
                          style: TextStyle(
                            fontSize: 12,
                            color: AppColors.textSecondary.withOpacity(0.4),
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            )
          : ListView.builder(
              itemCount: pending.length,
              padding: const EdgeInsets.symmetric(vertical: 8),
              itemBuilder: (context, index) {
                final invitation = pending[index];
                final isLoading =
                    loadingInvitationId == invitation.invitationId;

                return InvitationCard(
                  invitation: invitation,
                  isLoading: isLoading,
                  // Received tab: show Accept button
                  onAccept: () => onAccept(invitation.invitationId),
                  // Received tab: show Decline button
                  onDecline: () => onDecline(invitation.invitationId),
                );
              },
            ),
    );
  }
}
