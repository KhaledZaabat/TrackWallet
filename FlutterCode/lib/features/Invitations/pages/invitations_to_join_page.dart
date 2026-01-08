import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_state.dart';
import 'package:famxpense/features/Invitations/widgets/invitation_card.dart';

/// Page for users to view and accept/decline invitations to JOIN families
///
/// This is shown before family selection, allowing users to:
/// - View pending invitations from families
/// - Accept to join a family
/// - Decline invitations
///
/// Only shows received pending invitations (not sent).
/// Has 2-item navbar (Families, Invitations To Join)
class InvitationsToJoinPage extends StatelessWidget {
  const InvitationsToJoinPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider.value(
      value: getIt<InvitationsCubit>(),
      child: const _InvitationsToJoinView(),
    );
  }
}

class _InvitationsToJoinView extends StatefulWidget {
  const _InvitationsToJoinView();

  @override
  State<_InvitationsToJoinView> createState() => _InvitationsToJoinViewState();
}

class _InvitationsToJoinViewState extends State<_InvitationsToJoinView> {
  @override
  void initState() {
    super.initState();
    // Load invitations when page opens
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        context.read<InvitationsCubit>().loadAll();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Invitations'),
        centerTitle: false,
        backgroundColor: Colors.white,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        foregroundColor: Colors.black87,
      ),
      body: BlocConsumer<InvitationsCubit, InvitationsState>(
        listener: (context, state) {
          if (state is InvitationsError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red[400],
                duration: const Duration(seconds: 3),
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is InvitationsLoading) {
            return const Center(
              child: CircularProgressIndicator(),
            );
          }

          if (state is InvitationsLoaded) {
            // Only show received pending invitations
            final receivedPending = state.receivedInvitations
                .where((inv) => inv.status.name == 'pending')
                .toList();

            if (receivedPending.isEmpty) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(32),
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
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Check back later for family invitations',
                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                              color: Colors.grey[600],
                            ),
                      ),
                    ],
                  ),
                ),
              );
            }

            return SingleChildScrollView(
              physics: const AlwaysScrollableScrollPhysics(),
              child: Column(
                children: [
                  const SizedBox(height: 8),
                  ...receivedPending.map(
                    (invitation) => InvitationCard(
                      invitation: invitation,
                      onAccept: () {
                        context
                            .read<InvitationsCubit>()
                            .acceptInvitation(invitation.invitationId);
                      },
                      onDecline: () {
                        context
                            .read<InvitationsCubit>()
                            .declineInvitation(invitation.invitationId);
                      },
                      isLoading: state.loadingInvitationId == invitation.invitationId,
                    ),
                  ),
                  const SizedBox(height: 80),
                ],
              ),
            );
          }

          if (state is InvitationsError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(
                      Icons.error_outline,
                      size: 64,
                      color: Colors.red[300],
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'Failed to load invitations',
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      state.message,
                      textAlign: TextAlign.center,
                      style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                            color: Colors.grey[600],
                          ),
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      onPressed: () {
                        context.read<InvitationsCubit>().loadAll();
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.blue[600],
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 24,
                          vertical: 12,
                        ),
                      ),
                      icon: const Icon(Icons.refresh),
                      label: const Text('Retry'),
                    ),
                  ],
                ),
              ),
            );
          }

          return const SizedBox.shrink();
        },
      ),
      bottomNavigationBar: _buildNavBar(context),
    );
  }

  /// Build 2-item navbar (Families, Invitations To Join)
  BottomNavigationBar _buildNavBar(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;
    final currentIndex =
        location.startsWith(Routes.invitationsToJoin) ? 1 : 0;

    return BottomNavigationBar(
      currentIndex: currentIndex,
      type: BottomNavigationBarType.fixed,
      backgroundColor: Colors.white,
      selectedItemColor: const Color(0xFF5B7CB5),
      unselectedItemColor: const Color(0xFF5B6B8C).withOpacity(0.6),
      onTap: (index) {
        switch (index) {
          case 0:
            context.go(Routes.selectFamily);
            break;
          case 1:
            context.go(Routes.invitationsToJoin);
            break;
        }
      },
      items: const [
        BottomNavigationBarItem(
          icon: Icon(Icons.people),
          label: 'Families',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.mail),
          label: 'Invitations',
        ),
      ],
    );
  }
}
