import 'package:flutter/material.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'invitation_card.dart';

/// Display sent invitations grouped by status with Cancel button for Pending
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
    final l10n = AppLocalizations.of(context)!;
    
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
              l10n.noPendingInvitations,
              style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                    color: Colors.grey[600],
                  ),
            ),
          ],
        ),
      );
    }

    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    final others = invitations
        .where((inv) => inv.status != InvitationStatus.pending)
        .toList()
        ..sort((a, b) => b.sentAtUtc.compareTo(a.sentAtUtc));

    return ListView(
      children: [
        if (pending.isNotEmpty) ...[
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(
              l10n.received,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
            ),
          ),
          ...pending.map((inv) => InvitationCard(
            invitation: inv,
            isLoading: loadingInvitationId == inv.invitationId,
            onCancel: () => onCancel(inv.invitationId),
          )),
        ],

        if (others.isNotEmpty) ...[
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(
              l10n.sent,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
            ),
          ),
          ...others.map((inv) => InvitationCard(
            invitation: inv,
            isLoading: loadingInvitationId == inv.invitationId,
          )),
        ],
      ],
    );
  }
}
