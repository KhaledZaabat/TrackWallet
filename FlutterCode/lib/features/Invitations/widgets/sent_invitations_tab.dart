import 'package:flutter/material.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';
import 'invitation_card.dart';

/// Display sent invitations grouped by status with Cancel button for Pending
///
/// This tab shows all invitations sent from the current family, grouped and sorted by status.
/// Pending invitations are shown first with a Cancel option for each.
/// Other statuses (Accepted, Declined, Cancelled) are shown in a separate section.
///
/// Props:
/// - [invitations]: All sent invitations from cubit state
/// - [loadingInvitationId]: ID of invitation being processed (null when idle)
/// - [onCancel]: Callback when cancel button tapped (receives invitation ID)
///
/// Loading State Control:
/// - Each card receives isLoading = (loadingInvitationId == invitation.invitationId)
/// - Only the card being acted on shows loading state
/// - Other invitations remain interactive
///
/// Grouping & Sorting:
/// - Pending invitations shown first with Cancel button
/// - Other statuses (Accepted, Declined, Cancelled) grouped together
/// - Within each group, sorted by sentAtUtc descending (newest first)
class SentInvitationsTab extends StatelessWidget {
  final List<Invitation> invitations;
  final String? loadingInvitationId;
  final Function(String) onCancel;

  const SentInvitationsTab({
    required this.invitations,
    required this.loadingInvitationId,
    required this.onCancel,
  });

  @override
  Widget build(BuildContext context) {
    // Empty state: no sent invitations
    if (invitations.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.send_outlined,
              size: 64,
              color: Colors.grey[400],
            ),
            const SizedBox(height: 16),
            Text(
              'You haven\'t sent any invitations',
              style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                    color: Colors.grey[600],
                  ),
            ),
          ],
        ),
      );
    }

    // Group by status: Pending first, then others
    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
        // Sort by sentAtUtc descending (newest first)
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    final others = invitations
        .where((inv) => inv.status != InvitationStatus.pending)
        .toList()
        // Sort by sentAtUtc descending (newest first)
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    // Display grouped list with headers
    return ListView(
      children: [
        // Pending section
        if (pending.isNotEmpty) ...[
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(
              'Pending',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
            ),
          ),
          ...pending.map((inv) => InvitationCard(
            invitation: inv,
            isLoading: loadingInvitationId == inv.invitationId,
            // Sent tab: show Cancel button only for pending status
            onCancel: () => onCancel(inv.invitationId),
          )),
        ],

        // Other statuses section
        if (others.isNotEmpty) ...[
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(
              'Other',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
            ),
          ),
          ...others.map((inv) => InvitationCard(
            invitation: inv,
            isLoading: loadingInvitationId == inv.invitationId,
            // Sent tab: no cancel button for non-pending status
            // InvitationCard will only show status badge
          )),
        ],
      ],
    );
  }
}
