import 'package:flutter/material.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';
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

  const ReceivedInvitationsTab({
    required this.invitations,
    required this.loadingInvitationId,
    required this.onAccept,
    required this.onDecline,
  });

  @override
  Widget build(BuildContext context) {
    // Filter to Pending invitations only
    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
        // Sort by sentAtUtc descending (newest first)
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    // Empty state: no pending invitations
    if (pending.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.mail_outline,
              size: 64,
              color: Colors.grey[400],
            ),
            const SizedBox(height: 16),
            Text(
              'No pending invitations',
              style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                    color: Colors.grey[600],
                  ),
            ),
          ],
        ),
      );
    }

    // List of pending invitations with action buttons
    return ListView.builder(
      itemCount: pending.length,
      padding: const EdgeInsets.symmetric(vertical: 8),
      itemBuilder: (context, index) {
        final invitation = pending[index];
        final isLoading = loadingInvitationId == invitation.invitationId;

        return InvitationCard(
          invitation: invitation,
          isLoading: isLoading,
          // Received tab: show Accept button
          onAccept: () => onAccept(invitation.invitationId),
          // Received tab: show Decline button
          onDecline: () => onDecline(invitation.invitationId),
        );
      },
    );
  }
}
