import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';

/// Reusable card widget displaying a single invitation with appropriate action buttons
///
/// This widget is used in both ReceivedInvitationsTab and SentInvitationsTab to display
/// invitation details and handle user actions (accept, decline, cancel).
///
/// Props:
/// - [invitation]: The invitation data to display
/// - [onAccept]: Callback when accept button is tapped (null if sent tab or non-pending)
/// - [onDecline]: Callback when decline button is tapped (null if sent tab)
/// - [onCancel]: Callback when cancel button is tapped (null if received tab)
/// - [isLoading]: Whether this invitation's action is currently processing
class InvitationCard extends StatelessWidget {
  final Invitation invitation;
  final VoidCallback? onAccept;
  final VoidCallback? onDecline;
  final VoidCallback? onCancel;
  final bool isLoading;

  const InvitationCard({
    required this.invitation,
    this.onAccept,
    this.onDecline,
    this.onCancel,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    final isSent = onCancel != null || (onAccept == null && onDecline == null);
    final statusColor = _getStatusColor(invitation.status);

    return Card(
      margin: const EdgeInsets.all(8),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header: Invitation title + status badge
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    isSent
                        ? 'Invited ${invitation.inviteeEmail}'
                        : 'Invited by ${invitation.inviterName}',
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.w600,
                        ),
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                const SizedBox(width: 8),
                Chip(
                  label: Text(
                    invitation.status.name.toUpperCase(),
                    style: const TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  backgroundColor: statusColor.withOpacity(0.2),
                  side: BorderSide(color: statusColor, width: 1),
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                ),
              ],
            ),
            const SizedBox(height: 12),

            // Family and role information
            Text(
              'To join ${invitation.familyName} as ${invitation.isParent ? 'parent' : 'member'}',
              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                    color: Colors.grey[700],
                  ),
            ),
            const SizedBox(height: 4),

            // Date information
            Text(
              'Sent on ${_getFormattedDate(invitation.sentAtUtc)}',
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Colors.grey[500],
                  ),
            ),
            const SizedBox(height: 16),

            // Action buttons
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                // Accept button (only for received invitations)
                if (onAccept != null) ...[
                  ElevatedButton(
                    onPressed: isLoading ? null : onAccept,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                      disabledBackgroundColor: Colors.grey[300],
                    ),
                    child: isLoading
                        ? SizedBox(
                            height: 16,
                            width: 16,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              valueColor: AlwaysStoppedAnimation(
                                Colors.grey[600],
                              ),
                            ),
                          )
                        : const Text(
                            'Accept',
                            style: TextStyle(color: Colors.white),
                          ),
                  ),
                  const SizedBox(width: 8),
                ],

                // Decline button (only for received invitations)
                if (onDecline != null) ...[
                  OutlinedButton(
                    onPressed: isLoading ? null : onDecline,
                    style: OutlinedButton.styleFrom(
                      side: BorderSide(
                        color: isLoading ? Colors.grey[300]! : Colors.red,
                      ),
                      disabledForegroundColor: Colors.grey[400],
                    ),
                    child: const Text('Decline'),
                  ),
                  const SizedBox(width: 8),
                ],

                // Cancel button (only for sent pending invitations)
                if (onCancel != null && invitation.status == InvitationStatus.pending) ...[
                  OutlinedButton(
                    onPressed: isLoading ? null : onCancel,
                    style: OutlinedButton.styleFrom(
                      side: BorderSide(
                        color: isLoading ? Colors.grey[300]! : Colors.orange,
                      ),
                      disabledForegroundColor: Colors.grey[400],
                    ),
                    child: isLoading
                        ? SizedBox(
                            height: 16,
                            width: 16,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              valueColor: AlwaysStoppedAnimation(
                                Colors.grey[600],
                              ),
                            ),
                          )
                        : const Text('Cancel'),
                  ),
                ],
              ],
            ),
          ],
        ),
      ),
    );
  }

  /// Get color for status badge based on invitation status
  Color _getStatusColor(InvitationStatus status) {
    switch (status) {
      case InvitationStatus.pending:
        return Colors.orange;
      case InvitationStatus.accepted:
        return Colors.green;
      case InvitationStatus.declined:
        return Colors.red;
      case InvitationStatus.cancelled:
        return Colors.grey;
    }
  }

  /// Format date for display as "MMM dd, yyyy" in local timezone
  String _getFormattedDate(DateTime utcDateTime) {
    final localDateTime = utcDateTime.toLocal();
    return DateFormat('MMM dd, yyyy').format(localDateTime);
  }
}
