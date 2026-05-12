import 'package:flutter/material.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/l10n/app_localizations.dart';
import 'invitation_card.dart';

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
    final l10n = AppLocalizations.of(context)!;
    
    final pending = invitations
        .where((inv) => inv.status == InvitationStatus.pending)
        .toList()
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
                          l10n.noPendingInvitations,
                          style: TextStyle(
                            fontSize: 16,
                            color: AppColors.textSecondary.withOpacity(0.6),
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: 8),
                         Text(
                          l10n.checkBackLater,
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
                  onAccept: () => onAccept(invitation.invitationId),
                  onDecline: () => onDecline(invitation.invitationId),
                );
              },
            ),
    );
  }
}
