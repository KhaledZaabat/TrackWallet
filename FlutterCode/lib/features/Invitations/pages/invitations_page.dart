import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_state.dart';

import 'package:famxpense/features/Invitations/widgets/received_invitations_tab.dart';
import 'package:famxpense/features/Invitations/widgets/sent_invitations_tab.dart';
import 'package:famxpense/features/Invitations/widgets/send_invitation_dialog.dart';

/// Main page for managing family invitations with tabbed interface
///
/// Features:
/// - Tabbed interface: Received (Pending only) and Sent (all statuses)
/// - Accept/Decline buttons for received invitations
/// - Cancel button for pending sent invitations
/// - Floating Action Button to send new invitations
/// - Real-time loading states for individual invitation actions
/// - Error handling with retry mechanism
/// - Tab persistence (switches tabs without reloading data)
///
/// State Management:
/// - InvitationsInitial: Initial state before loading
/// - InvitationsLoading: Loading both lists
/// - InvitationsLoaded: Both lists loaded with tab info
/// - InvitationsError: Error state with message
///
/// Usage:
/// - Place in routes as '/invitations' route
/// - Accessible after family selection
/// - Automatically loads data on page open (via initState)
class InvitationsPage extends StatefulWidget {
  final bool forceGuestMode;
  const InvitationsPage({Key? key, this.forceGuestMode = false}) : super(key: key);

  @override
  State<InvitationsPage> createState() => _InvitationsPageState();
}

class _InvitationsPageState extends State<InvitationsPage> {
  bool _isFamilySelected = false;


  @override
  void initState() {
    super.initState();
    // Load invitations data after first frame builds
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        // Check if family is selected
        getIt<LocalStorage>().getSelectedFamilyId().then((familyId) {
          setState(() {
            _isFamilySelected = !widget.forceGuestMode && familyId != null && familyId.isNotEmpty;
          });
        });
        context.read<InvitationsCubit>().loadAll();
      }
    });
  }



  @override
  Widget build(BuildContext context) {
    return BlocConsumer<InvitationsCubit, InvitationsState>(
      // Listener: Show snackbars for success/error feedback
      listener: (context, state) {
        if (state is InvitationsError) {
          // Capture cubit reference before showing snackbar to avoid deactivated context
          final cubit = context.read<InvitationsCubit>();
          
          // Show error snackbar with retry button
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(state.message),
              duration: const Duration(seconds: 5),
              action: SnackBarAction(
                label: 'Retry',
                onPressed: () => cubit.loadAll(),
              ),
            ),
          );
        }
      },
      // Builder: Show UI based on state
      builder: (context, state) {
        // Loading state: Show centered progress indicator
        if (state is InvitationsLoading) {
          return Scaffold(
            backgroundColor: AppColors.background,
            appBar: AppBar(
              title: const Text('Family Invitations'),
              backgroundColor: AppColors.surface,
              foregroundColor: AppColors.textPrimary,
              elevation: 0,
              leading: IconButton(
                icon: const Icon(Icons.arrow_back),
                onPressed: () => context.go(Routes.dashboard),
              ),
            ),
            body: const Center(
              child: CircularProgressIndicator(),
            ),
          );
        }

        // Error state: Show error message with retry button
        if (state is InvitationsError) {
          return Scaffold(
             appBar: AppBar(
              title: const Text('Family Invitations'),
              backgroundColor: AppColors.surface,
              foregroundColor: AppColors.textPrimary,
              elevation: 0,
              leading: IconButton(
                icon: const Icon(Icons.arrow_back),
                onPressed: () => context.go(Routes.dashboard),
              ),
            ),
            backgroundColor: AppColors.background,
            body: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.error_outline,
                    size: 64,
                    color: AppColors.error,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    state.message,
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.bodyLarge,
                  ),
                  const SizedBox(height: 24),
                  ElevatedButton(
                    onPressed: () =>
                        context.read<InvitationsCubit>().loadAll(),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: AppColors.primary,
                      foregroundColor: Colors.white,
                    ),
                    child: const Text('Retry'),
                  ),
                ],
              ),
            ),
          );
        }

        // Loaded state: Show full UI with tabs
        if (state is InvitationsLoaded) {
          // For now, using empty string as fallback for currentUserEmail
          // In production, should fetch from localStorage or session
          final currentUserEmail = '';
          // Capture cubit reference for use in callbacks
          final cubit = context.read<InvitationsCubit>();

          // If no family selected, only show Received tab
          // NOTE: We don't render AppBottomNavBar here anymore, it's in the shell.
          if (!_isFamilySelected) {
            return Scaffold(
              backgroundColor: AppColors.background,
              appBar: AppBar(
                title: const Text('Family Invitations'),
                backgroundColor: AppColors.surface,
                foregroundColor: AppColors.textPrimary,
                elevation: 0,
              ),
              body: ReceivedInvitationsTab(
                invitations: state.receivedInvitations,
                loadingInvitationId: state.loadingInvitationId,
                onAccept: (id) =>
                    context.read<InvitationsCubit>().acceptInvitation(id),
                onDecline: (id) =>
                    context.read<InvitationsCubit>().declineInvitation(id),
                onRefresh: () async {
                  await context.read<InvitationsCubit>().loadAll();
                },
              ),
            );
          }

          // Family selected: Show both tabs
          return DefaultTabController(
            length: 2,
            initialIndex: state.selectedTab,
            child: Scaffold(
              backgroundColor: AppColors.background,
              appBar: AppBar(
                title: const Text('Family Invitations'),
                backgroundColor: AppColors.surface,
                foregroundColor: AppColors.textPrimary,
                elevation: 0,
                leading: IconButton(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => context.go(Routes.dashboard),
                ),
                bottom: TabBar(
                  onTap: (index) =>
                      context.read<InvitationsCubit>().switchTab(index),
                  labelColor: AppColors.primary,
                  unselectedLabelColor: AppColors.textSecondary,
                  indicatorColor: AppColors.primary,
                  tabs: const [
                    Tab(text: 'Received'),
                    Tab(text: 'Sent'),
                  ],
                ),
              ),
              body: TabBarView(
                children: [
                  // Received Invitations Tab (Pending only)
                  ReceivedInvitationsTab(
                    invitations: state.receivedInvitations,
                    loadingInvitationId: state.loadingInvitationId,
                    onAccept: (id) =>
                        context.read<InvitationsCubit>().acceptInvitation(id),
                    onDecline: (id) =>
                        context.read<InvitationsCubit>().declineInvitation(id),
                    onRefresh: () async {
                      await context.read<InvitationsCubit>().loadAll();
                    },
                  ),

                  // Sent Invitations Tab (All statuses, grouped)
                  SentInvitationsTab(
                    invitations: state.sentInvitations,
                    loadingInvitationId: state.loadingInvitationId,
                    onCancel: (id) =>
                        context.read<InvitationsCubit>().cancelInvitation(id),
                  ),
                ],
              ),

              // Floating Action Button: Send new invitation
              floatingActionButton: FloatingActionButton(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                onPressed: () {
                  showDialog(
                    context: context,
                    builder: (dialogContext) => SendInvitationDialog(
                      currentUserEmail: currentUserEmail,
                      cubit: cubit,
                    ),
                  );
                },
                tooltip: 'Send Invitation',
                child: const Icon(Icons.mail_outline),
              ),
            ),
          );
        }

        // Initial state: Show loading
        return Scaffold(
            backgroundColor: AppColors.background,
            appBar: AppBar(
              title: const Text('Family Invitations'),
              backgroundColor: AppColors.surface,
              foregroundColor: AppColors.textPrimary,
              elevation: 0,
              leading: IconButton(
                icon: const Icon(Icons.arrow_back),
                onPressed: () => context.go(Routes.dashboard),
              ),
            ),
          body: const Center(
            child: CircularProgressIndicator(),
          ),
        );
      },
    );
  }
}
